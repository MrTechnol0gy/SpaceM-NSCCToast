using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public static List<NavMeshAgent> all = new List<NavMeshAgent>();
    List<NavMeshAgent> agents = new List<NavMeshAgent>();
    List<NavMeshAgent> unplacedAgents = new List<NavMeshAgent>();


    [Header("Enemy Prefabs")]
    [SerializeField] GameObject enemyForceProbe;

    // Start is called before the first frame update
    void Start()
    {
        SpawnAgents();
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
    //     for (int i = 0; i < Enemy.all.Count; i++) {
    //         Enemy.all[i].DoSomething();
    //     }
    // }
}
