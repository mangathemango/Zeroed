using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// * Spawns enemies in the game
/// ! This is not a complete system. It's just a simple system that spawns enemies at random positions within a given area.
/// ! This class is here solely to debug the shooting system.
/// </summary>
public class EnemySpawner : Singleton<EnemySpawner>
{
    public Transform corner1;
    public Transform corner2;
    public GameObject enemyPrefab;
    private GameObject enemyContainer;
    public float spawnRate = 1;

    private bool readyToSpawn = true;
    // Start is called before the first frame update
    void Start()
    {
        enemyContainer = GameObject.Find("Enemies");
    }

    // Update is called once per frame
    void Update()
    {
        if (readyToSpawn)
        {
            SpawnEnemy();
            readyToSpawn = false;
            Invoke("ResetReadyToSpawn", spawnRate);
        }
    }

    void ResetReadyToSpawn() {
        readyToSpawn = true;
    }

    void SpawnEnemy() {
        Vector3 spawnPosition = new Vector3(Random.Range(corner1.position.x, corner2.position.x), 2, Random.Range(corner1.position.z, corner2.position.z));
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, enemyContainer.transform);
    }
}
