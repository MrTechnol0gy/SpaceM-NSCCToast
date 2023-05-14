using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PickUpManager : MonoBehaviour
{
    public static PickUpManager get;
    public static List<GameObject> allPickups = new List<GameObject>();
    // List of pickupable objects in the scene
    public List<GameObject> pickups = new List<GameObject>();
    List<GameObject> unplacedPickups = new List<GameObject>();

    [Header("Pickup Prefabs")]
    [SerializeField] GameObject pickupPrefab;
    [SerializeField] float pickupSpawnHeightFloor = 0.5f;
    [SerializeField] float pickupSpawnHeightCeiling = 25f;
    [SerializeField] float pickupRadius = 3f;
    private float spawnRadius;


    void Awake()
    {
        get = this;
    }

    void Start()
    {
        spawnRadius = GameManager.get.spawnRadius;
        allPickups.Clear();
    }

    public void SpawnPickups()
    {
        int numAgentsToSpawn = GameManager.get.amountOfPickUps;
        Vector3 position = Vector3.zero;
        List<GameObject> spawnedPrefabs = RandomTerrainGenerator.get.GetListOfAllTerrain();

        for (int i = 0; i < numAgentsToSpawn; i++)
        {
            bool positionFound = false;
            // Try to find a valid spawn position
            while (!positionFound)
            {
                // Choose a random position within the spawn radius
                position = new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(pickupSpawnHeightFloor, pickupSpawnHeightCeiling), Random.Range(-spawnRadius, spawnRadius));

                // Check if there are any colliders within the spawn radius
                Collider[] colliders = Physics.OverlapSphere(position, pickupRadius);
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
            // Instantiate new enemy force probe
            GameObject agent = Instantiate(pickupPrefab, position, Quaternion.identity);

            // Set the pickup's parent
            agent.transform.SetParent(transform);

            // Add the GameObject to the list of agents
            pickups.Add(agent);
        }
    }
    public List<GameObject> GetListOfAll()
    {
        return allPickups;
    }
}
