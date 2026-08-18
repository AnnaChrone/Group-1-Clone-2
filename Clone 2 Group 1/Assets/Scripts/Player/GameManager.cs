using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Game objects")]
    [SerializeField] private Transform character;
    [SerializeField] private Transform Player;
    [SerializeField] private Transform terrainHolder;

    [Header("Terrain Objects")]
    [SerializeField] private Grass grassPrefab;
    [SerializeField] private Road roadPrefab;
    [SerializeField] private Train trainPrefab;
    [SerializeField] private Log LogPrefab;

    [Header("Camera Settings")]
    [SerializeField] private CameraController cameraController;

    [Header("Game Parameters")]
    [SerializeField] private int spawnDistance = 20;
    [SerializeField] private float moveDuration = 0.2f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI distanceText;
    private int totalDistance = 0; // Track forward distance

    private Vector2Int playerPos;
    private int spawnLocation;
    private List<(float terrainHeight, HashSet<int> locations)> obstacles = new();

    //Chunk system logic
    private enum LaneType
    {
        Grass,
        Road,
        River,
        Train
    }

    private class LaneChunk
    {
        public List<LaneType> Lanes = new List<LaneType>();
    }

    private Queue<LaneChunk> chunkQueue = new Queue<LaneChunk>();
    //CHecks to ensure not too many consecutive lanes occur
    private int consecutiveRoads = 0;
    private int consecutiveGrass = 0;
    private int consecutiveRivers = 0;
    private int consecutiveTrains = 0;
    private int lanesUntilSafeZone = 0;
    private bool inSafeZone = false;


    enum GameState
    {
        Ready,
        Moving,
        Dead
    }
    private GameState gameState;
    private Vector2Int characterPos;

    private void Awake()
    {
        // Hey, Don't call NewLevel here anymore - we'll call it when game starts
        // NewLevel(); 
    }

    public void StartGameLevel()
    {
        NewLevel();
        SetGameActive(true);

        // Start gameplay camera
        if (cameraController != null)
        {
            cameraController.StartGameplayCamera();
        }
    }


    private bool InStartArea(Vector2Int location)
    {
        if((location.y > -5) && (location.y < 0) && (location.x > -6) && (location.x < 6))
        {
            return true;
        }
        return false;
    }
    private void Update()
    {
        if (gameState == GameState.Dead)
            return;

        if (gameState == GameState.Ready)
        {
            Vector2Int moveDirection = Vector2Int.zero;
            bool moved = false;

            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.identity;
                moveDirection.y = 1;
                moved = true;

            }
            else if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.Euler(0, 180, 0);
                moveDirection.y = -1;
                moved = true;

            }
            else if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.Euler(0, -90, 0);
                moveDirection.x = -1;
                moved = true;

            }
            else if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.Euler(0, 90, 0);
                moveDirection.x = 1;
                moved = true;

            }

            if (moveDirection != Vector2Int.zero)
            {
                Vector2Int destination = characterPos + moveDirection;
                if (InStartArea(destination) || ((destination.y >= 0) && !obstacles[destination.y].locations.Contains(destination.x)))
                {

                    int oldY = characterPos.y;
                    int newY = destination.y;

                    characterPos = destination;
                    StartCoroutine(MoveCharacter());

                    if (newY > oldY)
                    {
                        totalDistance += (newY - oldY);
                    }
                    else if (newY < oldY)
                    {
                        totalDistance -= (oldY - newY);
                        // Optional: Prevent negative distance
                        // if (totalDistance < 0) totalDistance = 0;
                    }

                    UpdateDistanceDisplay();

                    // Check if the new position is dangerous
                    CheckForDeathAtPosition(destination);
                }
            }
        }

        while (obstacles.Count < (characterPos.y + spawnDistance))
        {
            SpawnObstacle();
        }
    }

    private void UpdateDistanceDisplay()
    {
        if (distanceText != null)
        {

            distanceText.text = $"{totalDistance}";
        }
    }
    private void NewLevel()
    {
        gameState = GameState.Ready;

        characterPos = new Vector2Int(0, -1);
        character.position = new Vector3(0, 0.2f, -1);

        playerPos = new Vector2Int(0, -1);

        totalDistance = 0;
        UpdateDistanceDisplay();

        // Reset Terrain
        obstacles.Clear();
        chunkQueue.Clear();
        ResetConsecutiveCounters();
        foreach (Transform child in terrainHolder)
        {
            Destroy(child.gameObject);
        }

        // Reset camera
        if (cameraController != null)
        {
            cameraController.ResetCameraPosition();
        }

        spawnLocation = 0;
        for (int i = 0; i < spawnDistance; i++)
        {
            SpawnObstacle();
        }
    }

    public bool IsGameActive()
    {
        return gameState != GameState.Dead && gameState != GameState.Ready;
    }

    
    //PROCEDURAL GENERATION FUNCTIONS
    private void ResetConsecutiveCounters()
    {
        consecutiveRoads = 0;
        consecutiveGrass = 0;
        consecutiveRivers = 0;
        consecutiveTrains = 0;
        lanesUntilSafeZone = 0;
        inSafeZone = false;
    }


    private void SpawnObstacle()
    {
        LaneType selectedType = DetermineLaneType();

        // Create the actual lane based on type
        switch (selectedType)
        {
            case LaneType.Train:
                Train train = Instantiate(trainPrefab, terrainHolder);
                obstacles.Add((0.1f, train.Init(spawnLocation)));
                consecutiveTrains++;
                consecutiveRoads = 0;
                consecutiveGrass = 0;
                consecutiveRivers = 0;
                break;

            case LaneType.River:
                Log log = Instantiate(LogPrefab, terrainHolder);
                obstacles.Add((0.1f, log.Init(spawnLocation)));
                consecutiveRivers++;
                consecutiveRoads = 0;
                consecutiveGrass = 0;
                consecutiveTrains = 0;
                break;

            case LaneType.Road:
                Road road = Instantiate(roadPrefab, terrainHolder);
                obstacles.Add((0.1f, road.Init(spawnLocation)));
                consecutiveRoads++;
                consecutiveGrass = 0;
                consecutiveRivers = 0;
                consecutiveTrains = 0;
                break;

            case LaneType.Grass:
            default:
                Grass grass = Instantiate(grassPrefab, terrainHolder);
                obstacles.Add((0.2f, grass.Init(spawnLocation)));
                consecutiveGrass++;
                consecutiveRoads = 0;
                consecutiveRivers = 0;
                consecutiveTrains = 0;
                break;
        }

        // Track safe zones of grass
        if (selectedType == LaneType.Grass)
        {
            inSafeZone = true;
            lanesUntilSafeZone = 0;
        }
        else
        {
            inSafeZone = false;
            lanesUntilSafeZone++;
        }

        spawnLocation++;
    }

    private LaneType DetermineLaneType()
    {
        //Pre-generated chunks
        if (chunkQueue.Count == 0)
        {
            GenerateNextChunk();
        }

        //Use left over chunk lanes if there are
        if (chunkQueue.Count > 0)
        {
            var currentChunk = chunkQueue.Peek();
            if (currentChunk.Lanes.Count > 0)
            {
                LaneType type = currentChunk.Lanes[0];
                currentChunk.Lanes.RemoveAt(0);

                // If chunk is empty, remove it from the queue
                if (currentChunk.Lanes.Count == 0)
                {
                    chunkQueue.Dequeue();
                }

                return type;
            }
        }

        // Fallback to constraint-based generation - if the constraints kick in
        return GenerateConstrainedLane();
    }

    private void GenerateNextChunk()
    {
        LaneChunk chunk = new LaneChunk();
        int chunkSize = Random.Range(5, 11); // 5-10 lanes per chunk - ensures distance into game

        //Decides chunk theme
        float themeRoll = Random.value;

        // Build the chunk with safe zones and hazard grouping alike
        if (inSafeZone && lanesUntilSafeZone > 2)
        {
            BuildHazardChunk(chunk, chunkSize);
        }
        else if (!inSafeZone && lanesUntilSafeZone > 4)
        {
            //forces safe zone if too many hazards
            BuildSafeChunk(chunk, chunkSize);
        }
        else
        {
            // Mixed chunk
            BuildMixedChunk(chunk, chunkSize);
        }

        chunkQueue.Enqueue(chunk);
    }

    private void BuildSafeChunk(LaneChunk chunk, int size)
    {
        // 2-4 grass lanes for breathing room of player
        int grassCount = Random.Range(2, Mathf.Min(5, size));
        for (int i = 0; i < grassCount; i++)
        {
            chunk.Lanes.Add(LaneType.Grass);
        }

        int hazardCount = Mathf.Min(size - grassCount, 2);
        for (int i = 0; i < hazardCount; i++)
        {
            chunk.Lanes.Add(GetRandomHazardType());
        }
    }

    private void BuildHazardChunk(LaneChunk chunk, int size)
    {
        // Build a hazard-dense chunk
        int remaining = size;

        // Always starts with 1 grass for initial breathing
        if (remaining > 0)
        {
            chunk.Lanes.Add(LaneType.Grass);
            remaining--;
        }

        // Add 2-4 hazard lanes
        int hazardCount = Mathf.Min(Random.Range(2, 5), remaining);
        for (int i = 0; i < hazardCount; i++)
        {
            chunk.Lanes.Add(GetRandomHazardType());
            remaining--;
        }

        // End with 1-2 grass for recovery time
        int endingGrass = Mathf.Min(Random.Range(1, 3), remaining);
        for (int i = 0; i < endingGrass; i++)
        {
            chunk.Lanes.Add(LaneType.Grass);
            remaining--;
        }

        // Fill any remaining with grass lanes
        while (remaining > 0)
        {
            chunk.Lanes.Add(LaneType.Grass);
            remaining--;
        }
    }

    private void BuildMixedChunk(LaneChunk chunk, int size)
    {
        // Create a balanced mix
        int remaining = size;

        // Start with 1-2 grass
        int startGrass = Random.Range(1, 3);
        for (int i = 0; i < Mathf.Min(startGrass, remaining); i++)
        {
            chunk.Lanes.Add(LaneType.Grass);
            remaining--;
        }

        // Alternate hazards and grass
        bool hazardNext = true;
        while (remaining > 0)
        {
            if (hazardNext)
            {
                // Add 1-2 hazards
                int hazardCount = Random.Range(1, 3);
                for (int i = 0; i < Mathf.Min(hazardCount, remaining); i++)
                {
                    chunk.Lanes.Add(GetRandomHazardType());
                    remaining--;
                }
            }
            else
            {
                // Add 1-2 grass
                int grassCount = Random.Range(1, 3);
                for (int i = 0; i < Mathf.Min(grassCount, remaining); i++)
                {
                    chunk.Lanes.Add(LaneType.Grass);
                    remaining--;
                }
            }
            hazardNext = !hazardNext;
        }
    }

    private LaneType GenerateConstrainedLane()
    {
        // Checks if we're exceeding max consecutive lanes of any type

        if (consecutiveRoads >= 3 || consecutiveRivers >= 3 || consecutiveTrains >= 1)
        {
            return LaneType.Grass;
        }

        // Force a hazard if we've had too much grass
        if (consecutiveGrass >= 4)
        {
            return GetRandomHazardType();
        }

        // Normal random selection with weighted probabilities
        float randomValue = Random.value;

        // Adjust probabilities based on recent history of lanes
        float grassWeight = 0.5f;
        float roadWeight = 0.25f;
        float riverWeight = 0.2f;
        float trainWeight = 0.05f;

        // Reduce chance of more hazards if we've had several in a row
        if (consecutiveRoads >= 2) roadWeight *= 0.5f;
        if (consecutiveRivers >= 2) riverWeight *= 0.5f;
        if (consecutiveTrains >= 1) trainWeight = 0f;

        // Increase chance of grass if too many hazrds
        if (consecutiveRoads >= 2 || consecutiveRivers >= 2) grassWeight *= 1.5f;

        // Normalize weights of percentages
        float totalWeight = grassWeight + roadWeight + riverWeight + trainWeight;
        float normalizedRandom = randomValue * totalWeight;

        if (normalizedRandom < grassWeight)
            return LaneType.Grass;
        else if (normalizedRandom < grassWeight + roadWeight)
            return LaneType.Road;
        else if (normalizedRandom < grassWeight + roadWeight + riverWeight)
            return LaneType.River;
        else
            return LaneType.Train;
    }

    private LaneType GetRandomHazardType()
    {
        // Randomly select a hazard type (now weighted)
        float randomValue = Random.value;

        if (randomValue < 0.6f)
            return LaneType.Road; 
        else if (randomValue < 0.85f)
            return LaneType.River;
        else
            return LaneType.Train; // Rare
    }




    private IEnumerator MoveCharacter()
    {
        gameState = GameState.Moving;
        float elapsedTime = 0f;

        float yHeight = 0.2f;

        Vector3 startPos = character.position;
        Vector3 endPos = new(characterPos.x, yHeight, characterPos.y);

        while (elapsedTime < moveDuration)
        {
            float percent = elapsedTime / moveDuration;

            Vector3 newPos = Vector3.Lerp(startPos, endPos, percent);
            newPos.y = yHeight + (0.5f * Mathf.Sin(Mathf.PI * percent));
            character.position = newPos;
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        character.position = endPos;

        if (gameState == GameState.Moving)
        {
            gameState = GameState.Ready;
        }
    }
    public void SetGameActive(bool active)
    {
        if (active)
        {
            gameState = GameState.Ready;
        }
        else
        {
            gameState = GameState.Dead; // Prevents movement
        }
    }
    public bool IsPositionSafe(Vector2Int position)
    {
        if (InStartArea(position))
            return true;

        // Check if position is valid and not blocked by obstacles
        if (position.y >= 0 && position.y < obstacles.Count)
        {
            return !obstacles[position.y].locations.Contains(position.x);
        }

        return false;
    }

    public Vector2Int GetCharacterPosition()
    {
        return characterPos;
    }

    private void CheckForDeathAtPosition(Vector2Int position)
    {
        //PlayerDeathHandler handle collisions
    }

    public void ExitToDesktop()
    {
        Debug.Log("Game is exiting...");

        Application.Quit();

        // If you're in the editor this will run
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
