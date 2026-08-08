using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game objects")]
    [SerializeField] private Transform terrainHolder;

    [Header("Terrain Objects")]
    [SerializeField] private Grass grassPrefab;

    [Header("Game Parameters")]
    [SerializeField] private int spawnDistance = 20;


    private int spawnLocation;
    private List<(float terrainHeight, HashSet<int> locations)> obstacles = new();

    private void Awake()
    {
        NewLevel();
    }

    private void NewLevel()
    {
        
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
        Grass grass = Instantiate(grassPrefab, terrainHolder);
        obstacles.Add((0.2f, grass.Init(spawnLocation)));

        spawnLocation++;
    }
}
