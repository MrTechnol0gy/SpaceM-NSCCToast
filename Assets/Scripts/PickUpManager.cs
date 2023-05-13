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

        for (int i = 0; i < numAgentsToSpawn; i++)
        {
            // Choose a random position within the spawn radius
            Vector3 position = new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(pickupSpawnHeightFloor, pickupSpawnHeightCeiling), Random.Range(-spawnRadius, spawnRadius));

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
