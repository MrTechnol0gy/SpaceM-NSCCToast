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


    [Header("Enemy Prefabs")]
    [SerializeField] GameObject enemyForceProbe;
    [SerializeField] GameObject elementalTornado;
    [Header("Spawning Stats")]
    [SerializeField] public float enemyForceMinHeight = 1f;
    [SerializeField] public float enemyForceMaxHeight = 5f;
    [SerializeField] public float elementalTornadoSpawnHeight = 0f;
    
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

        for (int i = 0; i < numEnemiesToSpawn; i++)
        {
            // Choose a random position within the spawn radius
            Vector3 position = new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(enemyForceMinHeight, enemyForceMaxHeight), Random.Range(-spawnRadius, spawnRadius));

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

        for (int i = 0; i < numEnemiesToSpawn; i++)
        {
            // Choose a random position within the spawn radius
            Vector3 position = new Vector3(Random.Range(-spawnRadius, spawnRadius), elementalTornadoSpawnHeight, Random.Range(-spawnRadius, spawnRadius));

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
