using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public static List<NavMeshAgent> allEnemies = new List<NavMeshAgent>();
    public static List<NavMeshAgent> allEnvironmentThreats = new List<NavMeshAgent>();
    List<NavMeshAgent> agents = new List<NavMeshAgent>();
    List<NavMeshAgent> unplacedAgents = new List<NavMeshAgent>();


    [Header("Enemy Prefabs")]
    [SerializeField] GameObject enemyForceProbe;
    [SerializeField] float enemyForceHeightFloor = 1f;
    [SerializeField] float enemyForceHeightCeiling = 5f;
    [SerializeField] GameObject elementalTornado;

    // Start is called before the first frame update
    void Start() 
    {
        allEnemies.Clear();             // clean up code
        allEnvironmentThreats.Clear();  // clean up code
        SpawnAgents();
        SpawnEnvironmentalDangers();
    }

    void SpawnAgents()
    {
        float spawnRadius = GameManager.get.spawnRadius;
        int numAgentsToSpawn = GameManager.get.amountOfEnemies;

        // Get NavMeshAgent component once outside the loop
        NavMeshAgent agentTemplate = enemyForceProbe.GetComponent<NavMeshAgent>();

        for (int i = 0; i < numAgentsToSpawn; i++)
        {
            // Instantiate new enemy force probe
            GameObject agentObject = Instantiate(enemyForceProbe, Vector3.zero, Quaternion.identity);

            // Get the NavMeshAgent component from the instantiated object
            NavMeshAgent agent = agentObject.GetComponent<NavMeshAgent>();

            // Set the NavMeshAgent component to use the same values as the template
            agent.radius = agentTemplate.radius;
            agent.height = agentTemplate.height;
            agent.speed = agentTemplate.speed;
            agent.angularSpeed = agentTemplate.angularSpeed;
            agent.acceleration = agentTemplate.acceleration;
            agent.stoppingDistance = agentTemplate.stoppingDistance;
            agent.autoBraking = agentTemplate.autoBraking;
            agent.obstacleAvoidanceType = agentTemplate.obstacleAvoidanceType;
            agent.avoidancePriority = agentTemplate.avoidancePriority;

            // Set the NavMeshAgent component spawn height to the desired range
            agent.baseOffset = Random.Range(enemyForceHeightFloor, enemyForceHeightCeiling);

            // Add the NavMeshAgent to the list of agents
            agents.Add(agent);

            // Add the NavMeshAgent to the list of unplaced agents
            unplacedAgents.Add(agent);
        }

        // Place agents on NavMesh
        for (int i = 0; i < agents.Count; i++)
        {
            bool placed = false;

            // Try to place the agent at a random position in the NavMesh
            for (int j = 0; j < 10 && !placed; j++)
            {
                // Get a random position on the NavMesh within a certain radius
                Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
                Vector3 randomPosition = transform.position + randomDirection;
                NavMeshHit hit;

                if (NavMesh.SamplePosition(randomPosition, out hit, spawnRadius, NavMesh.AllAreas))
                {
                    Debug.Log("Valid position found: " + hit.position);

                    // Set the agent's position to the random position
                    agents[i].Warp(hit.position);
                    agents[i].transform.SetParent(transform);
                    placed = true;

                    // Remove the agent from the list of unplaced agents
                    unplacedAgents.Remove(agents[i]);
                }
                else
                {
                    Debug.Log("No valid position found.");
                }
            }

            // If the agent was not placed, remove it from the list of agents
            if (!placed)
            {
                agents.RemoveAt(i);
                i--;
            }

            // If all agents have been placed, break out of the loop
            if (unplacedAgents.Count == 0)
            {
                break;
            }
        }
    }
    void SpawnEnvironmentalDangers()
    {
        float spawnRadius = GameManager.get.spawnRadius;
        int numAgentsToSpawn = GameManager.get.amountOfEnvironmentalDangers;

        // Get NavMeshAgent component once outside the loop
        NavMeshAgent agentTemplate = elementalTornado.GetComponent<NavMeshAgent>();

        for (int i = 0; i < numAgentsToSpawn; i++)
        {
            // Instantiate new enemy force probe
            GameObject agentObject = Instantiate(elementalTornado, Vector3.zero, Quaternion.identity);

            // Get the NavMeshAgent component from the instantiated object
            NavMeshAgent agent = agentObject.GetComponent<NavMeshAgent>();

            // Set the NavMeshAgent component to use the same values as the template
            agent.radius = agentTemplate.radius;
            agent.height = agentTemplate.height;
            agent.speed = agentTemplate.speed;
            agent.angularSpeed = agentTemplate.angularSpeed;
            agent.acceleration = agentTemplate.acceleration;
            agent.stoppingDistance = agentTemplate.stoppingDistance;
            agent.autoBraking = agentTemplate.autoBraking;
            agent.obstacleAvoidanceType = agentTemplate.obstacleAvoidanceType;
            agent.avoidancePriority = agentTemplate.avoidancePriority;

            // Set the NavMeshAgent component spawn height to the desired range
            agent.baseOffset = 0;

            // Add the NavMeshAgent to the list of agents
            agents.Add(agent);

            // Add the NavMeshAgent to the list of unplaced agents
            unplacedAgents.Add(agent);
        }

        // Place agents on NavMesh
        for (int i = 0; i < agents.Count; i++)
        {
            bool placed = false;

            // Try to place the agent at a random position in the NavMesh
            for (int j = 0; j < 10 && !placed; j++)
            {
                // Get a random position on the NavMesh within a certain radius
                Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
                Vector3 randomPosition = transform.position + randomDirection;
                NavMeshHit hit;

                if (NavMesh.SamplePosition(randomPosition, out hit, spawnRadius, NavMesh.AllAreas))
                {
                    Debug.Log("Valid position found: " + hit.position);

                    // Set the agent's position to the random position
                    agents[i].Warp(hit.position);
                    agents[i].transform.SetParent(transform);
                    placed = true;

                    // Remove the agent from the list of unplaced agents
                    unplacedAgents.Remove(agents[i]);
                }
                else
                {
                    Debug.Log("No valid position found.");
                }
            }

            // If the agent was not placed, remove it from the list of agents
            if (!placed)
            {
                agents.RemoveAt(i);
                i--;
            }

            // If all agents have been placed, break out of the loop
            if (unplacedAgents.Count == 0)
            {
                break;
            }
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

    public List<NavMeshAgent> GetListOfAll()
    {
        return allEnemies;
    }
}
