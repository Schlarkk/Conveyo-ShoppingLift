using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;       // The prefab to spawn
    public Transform[] spawnPoints;        // Array of spawn points
    public float spawnInterval = 5f;       // How often to spawn (in seconds)

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnPrefab();
            timer = 0f;
        }
    }

    void SpawnPrefab()
    {
        Vector3 spawnPos;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            spawnPos = spawnPoints[randomIndex].position;
        }
        else
        {
            spawnPos = transform.position;
        }

        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }
}

