using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Game objects")]
    [SerializeField] private Transform character;//new
    [SerializeField] private Transform Player;
    [SerializeField] private Transform terrainHolder;

    [Header("Terrain Objects")]
    [SerializeField] private Grass grassPrefab;
    [SerializeField] private Road roadPrefab;
    [SerializeField] private Train trainPrefab;
    [SerializeField] private Log LogPrefab;


    [Header("Game Parameters")]
    [SerializeField] private int spawnDistance = 20;
    [SerializeField] private float moveDuration = 0.2f;//new

    private Vector2Int playerPos;
    private int spawnLocation;
    private List<(float terrainHeight, HashSet<int> locations)> obstacles = new();


    enum GameState
    {
        Ready,
        Moving,
        Dead
    }
    private GameState gameState;
    private Vector2Int characterPos;//new

    private void Awake()
    {
        NewLevel();
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
        if (gameState == GameState.Ready)
        {
            Vector2Int moveDirection = Vector2Int.zero;
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.identity;
                moveDirection.y = 1;
            }
            else if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.Euler(0, 180, 0);
                moveDirection.y = -1;
            }
            else if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.Euler(0, -90, 0);
                moveDirection.x = -1;
            }
            else if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.Euler(0, 90, 0);
                moveDirection.x = 1;
            }

            if (moveDirection != Vector2Int.zero)
            {
                Vector2Int destination = characterPos + moveDirection;
                if (InStartArea(destination) || ((destination.y >= 0) && !obstacles[destination.y].locations.Contains(destination.x)))
                {
                    characterPos = destination;
                    StartCoroutine(MoveCharacter());
                }
            }
        }

        while (obstacles.Count < (characterPos.y + spawnDistance))
            {
            SpawnObstacle();
        }
        
        //Camera
        Vector3 cameraPosition = new(character.position.x + 2, 4, character.position.z - 3);
        
        cameraPosition.x = Mathf.Clamp(cameraPosition.x, -1, 5);
        
        Camera.main.transform.position = cameraPosition;

    }
    private void NewLevel()
    {
        gameState = GameState.Ready;

        characterPos = new Vector2Int(0, -1);
        character.position = new Vector3(0, 0.2f, -1);//new 


        playerPos = new Vector2Int(0,-1);
        //Reset Terrain
        obstacles.Clear();
        foreach (Transform child in terrainHolder)
        {
            Destroy(child.gameObject);
        }

        spawnLocation = 0;
        for (int i = 0; i < spawnDistance; i++)
        {
            SpawnObstacle();
        }
    }

    private void SpawnObstacle()
    {
        float randomValue = Random.value;

        if (randomValue < 0.1f)
        {
            // 10% Train
            Train train = Instantiate(trainPrefab, terrainHolder);
            obstacles.Add((0.1f, train.Init(spawnLocation)));
        }
        else if (randomValue < 0.25f)
        {
            // 15% Log
            Log log = Instantiate(LogPrefab, terrainHolder);
            obstacles.Add((0.1f, log.Init(spawnLocation)));
        }
        else if (randomValue < 0.5f)
        {
            // 25% Road
            Road road = Instantiate(roadPrefab, terrainHolder);
            obstacles.Add((0.1f, road.Init(spawnLocation)));
        }
        else
        {
            // 50% Grass
            Grass grass = Instantiate(grassPrefab, terrainHolder);
            obstacles.Add((0.2f, grass.Init(spawnLocation)));
        }

        spawnLocation++;
    }

    private IEnumerator MoveCharacter()//new
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
}
