using System.Collections;
using UnityEngine;

public class LaneSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject[] obstaclePrefabs; // Array of cars or logs
    public float minSpawnInterval = 1.5f;
    public float maxSpawnInterval = 3.5f;
    public float speed = 5f;
    public bool isMovingRight = true;

    private Vector3 spawnPosition;
    private Vector3 moveDirection;

    void Start()
    {
        // Calculate spawn point based on lane position and direction
        float spawnX = isMovingRight ? -15f : 15f;
        spawnPosition = new Vector3(spawnX, transform.position.y, transform.position.z);
        moveDirection = isMovingRight ? Vector3.right : Vector3.left;

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            SpawnObstacle();
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefabs.Length == 0) return;

        // Pick a random obstacle from array
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);

        // Flip mesh rotation if moving left
        if (!isMovingRight)
        {
            instance.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        // Pass movement settings to the obstacle
        Mover mover = instance.GetComponent<Mover>();
        if (mover != null)
        {
            mover.Initialize(speed, moveDirection);
        }
    }
}