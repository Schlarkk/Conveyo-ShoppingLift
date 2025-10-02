using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryAfterTime : MonoBehaviour
{
    public float spawnInterval = 4f;       // How fast to destroy

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
        Destroy(gameObject);
    }
}
