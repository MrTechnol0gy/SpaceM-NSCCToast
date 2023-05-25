using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager get;
    public static List<GameObject> allEnemies = new List<GameObject>();
    public static List<GameObject> allEnvironmentThreats = new List<GameObject>();
    List<GameObject> agents = new List<GameObject>();
    List<GameObject> unplacedAgents = new List<GameObject>();
    public GameObject anchor;
    public float minDistanceFromSpawn = 25f;

    [Header("Enemy Prefabs")]
    [SerializeField] GameObject enemyForceProbe;
    [SerializeField] GameObject elementalTornado;
    [Header("Spawning Stats")]
    [SerializeField] public float enemyForceMinHeight = 1f;
    [SerializeField] public float enemyForceMaxHeight = 5f;
    [SerializeField] float enemyForceRadius = 5f;
    [SerializeField] public float elementalTornadoSpawnHeight = 0f;
    [SerializeField] float elementalTornadoRadius = 8f;
    
    private float spawnRadius;

    void Awake()
    {
        get = this;
    }

    void Start() 
    {
        spawnRadius = GameManager.get.spawnRadius;
        allEnemies.Clear();             // clean up code
        allEnvironmentThreats.Clear();  // clean up code
    }

    public void SpawnAgents()
    {
        int numEnemiesToSpawn = GameManager.get.amountOfEnemies;
        Vector3 position = Vector3.zero;
        List<GameObject> spawnedPrefabs = RandomTerrainGenerator.get.GetListOfAllTerrain();

        for (int i = 0; i < numEnemiesToSpawn; i++)
        {
            bool positionFound = false;
            // Try to find a valid spawn position
            while (!positionFound)
            {
                 // Choose a random position within the spawn radius
                position = new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(enemyForceMinHeight, enemyForceMaxHeight), Random.Range(-spawnRadius, spawnRadius));

                // Check if there are any colliders within the spawn radius
                Collider[] colliders = Physics.OverlapSphere(position, enemyForceRadius);
                bool collisionFound = false;
                foreach (Collider collider in colliders)
                {
                    float prefabToSpawnDistance = Vector3.Distance(position, anchor.transform.position);
                    Debug.Log("distance to spawn is " + prefabToSpawnDistance);
                    if (spawnedPrefabs.Contains(collider.gameObject))
                    {
                        // A spawned object is already at this position
                        collisionFound = true;
                        break;
                    }
                    else if (prefabToSpawnDistance < minDistanceFromSpawn)
                    {
                        collisionFound = true;
                        break;
                    }
                }
                // If there are no collisions, we've found a valid spawn position
                if (!collisionFound)
                {
                    positionFound = true;
                }
            }
            // Instantiate new enemy force probe
            GameObject agent = Instantiate(enemyForceProbe, position, Quaternion.identity);

            // Set the pickup's parent
            agent.transform.SetParent(transform);

            // Add the GameObject to the list of agents
            agents.Add(agent);
        }
    }
    public void SpawnEnvironmentalDangers()
    {
        int numEnemiesToSpawn = GameManager.get.amountOfEnvironmentalDangers;
        Vector3 position = Vector3.zero;
        List<GameObject> spawnedPrefabs = RandomTerrainGenerator.get.GetListOfAllTerrain();

        for (int i = 0; i < numEnemiesToSpawn; i++)
        {
            bool positionFound = false;
            // Try to find a valid spawn position
            while (!positionFound)
            {
                 // Choose a random position within the spawn radius
                position = new Vector3(Random.Range(-spawnRadius, spawnRadius), elementalTornadoSpawnHeight, Random.Range(-spawnRadius, spawnRadius));

                // Check if there are any colliders within the spawn radius
                Collider[] colliders = Physics.OverlapSphere(position, elementalTornadoRadius);
                bool collisionFound = false;
                foreach (Collider collider in colliders)
                {
                    if (spawnedPrefabs.Contains(collider.gameObject))
                    {
                        // A spawned object is already at this position
                        collisionFound = true;
                        break;
                    }
                }
                // If there are no collisions, we've found a valid spawn position
                if (!collisionFound)
                {
                    positionFound = true;
                }
            }
            // Instantiate new elemental tornado
            GameObject agent = Instantiate(elementalTornado, position, Quaternion.identity);

            // Set the pickup's parent
            agent.transform.SetParent(transform);

            // Add the GameObject to the list of agents
            agents.Add(agent);
        }
    }
    
    // private void OnEnable() 
    // {
    //     all.Add(this);
    // }

    // private void OnDisable() 
    // {
    //     all.Remove(this);
    // }

    // public void DoAllEnemies()
    // {
    //     for (int i = 0; i < EnemyManager.all.Count; i++) {
    //         EnemyManager.all[i].DoSomething();
    //     }
    // }

    public List<GameObject> GetListOfAll()
    {
        return allEnemies;
    }
}
