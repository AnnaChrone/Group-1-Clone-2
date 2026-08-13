using System.Collections;
using UnityEngine;

public class LaneSpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    [SerializeField] private GameObject[] spawnPrefabs;
    [SerializeField] private float minSpawnInterval = 2.0f;
    [SerializeField] private float maxSpawnInterval = 4.5f;
    [SerializeField] private float speed = 6.0f;
    [SerializeField] private bool moveRight = true;
    [SerializeField] private float spawnXOffset = 20.0f;

    private Vector3 spawnPoint;
    private Vector3 moveDirection;

    private void Start()
    {
        SetupDirections();
        StartCoroutine(SpawnRoutine());
    }

    public void SetupLane(float laneSpeed, bool moveRightDirection)
    {
        this.speed = laneSpeed;
        this.moveRight = moveRightDirection;
        SetupDirections();
    }

    private void SetupDirections()
    {
        float startX = moveRight ? -spawnXOffset : spawnXOffset;
        spawnPoint = new Vector3(startX, transform.position.y + 0.5f, transform.position.z);
        moveDirection = moveRight ? Vector3.right : Vector3.left;
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            SpawnObject();
        }
    }

    private void SpawnObject()
    {
        if (spawnPrefabs == null || spawnPrefabs.Length == 0) return;

        GameObject chosenPrefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
        GameObject spawned = Instantiate(chosenPrefab, spawnPoint, Quaternion.identity, transform);

        if (!moveRight)
        {
            spawned.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        Mover mover = spawned.GetComponent<Mover>();
        if (mover != null)
        {
            mover.Initialize(speed, moveDirection, spawnXOffset + 5f);
        }
    }
}