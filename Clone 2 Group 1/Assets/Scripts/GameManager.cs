using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game objects")]
    [SerializeField] private Transform Player;
    [SerializeField] private Transform terrainHolder;

    [Header("Terrain Objects")]
    [SerializeField] private Grass grassPrefab;
    [SerializeField] private Road roadPrefab;

    [Header("Game Parameters")]
    [SerializeField] private int spawnDistance = 20;

    private Vector2Int playerPos;
    private int spawnLocation;
    private List<(float terrainHeight, HashSet<int> locations)> obstacles = new();

    private void Awake()
    {
        NewLevel();
    }

    private void Update()
    {
        while (obstacles.Count < (playerPos.y + spawnDistance))
            {
            SpawnObstacle();
        }
    }

    private void NewLevel()
    {
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
        //Spawns more roads the further u get
        float roadProbability = Mathf.Lerp(0.5f, 0.9f, spawnLocation / 250f);

        if (Random.value < roadProbability)
        {
            Road road = Instantiate(roadPrefab, terrainHolder);
            obstacles.Add((0.1f, road.Init(spawnLocation)));
        }
        else
        {

            Grass grass = Instantiate(grassPrefab, terrainHolder);
            obstacles.Add((0.2f, grass.Init(spawnLocation)));
        }



        spawnLocation++;
    }
}
