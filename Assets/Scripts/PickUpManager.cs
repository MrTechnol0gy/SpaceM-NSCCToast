using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PickUpManager : MonoBehaviour
{
    public static PickUpManager get;
    public static List<NavMeshAgent> allPickups = new List<NavMeshAgent>();
    // List of pickupable objects in the scene
    public List<NavMeshAgent> pickups = new List<NavMeshAgent>();
    List<NavMeshAgent> unplacedPickups = new List<NavMeshAgent>();

    [Header("Pickup Prefabs")]
    [SerializeField] GameObject pickupPrefab;
    [SerializeField] float pickupSpawnHeightFloor = 0.5f;
    [SerializeField] float pickupSpawnHeightCeiling = 25f;

    void Awake()
    {
        allPickups.Clear();
        get = this;
    }

    public void SpawnPickups()
    {
        float spawnRadius = GameManager.get.spawnRadius;
        int numAgentsToSpawn = GameManager.get.amountOfPickUps;

        // Get NavMeshAgent component once outside the loop
        NavMeshAgent agentTemplate = pickupPrefab.GetComponent<NavMeshAgent>();

        for (int i = 0; i < numAgentsToSpawn; i++)
        {
            // Instantiate new enemy force probe
            GameObject agentObject = Instantiate(pickupPrefab, Vector3.zero, Quaternion.identity);

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
            agent.baseOffset = Random.Range(pickupSpawnHeightFloor, pickupSpawnHeightCeiling);

            // Add the NavMeshAgent to the list of agents
            pickups.Add(agent);

            // Add the NavMeshAgent to the list of unplaced agents
            unplacedPickups.Add(agent);
        }

        // Place agents on NavMesh
        for (int i = 0; i < pickups.Count; i++)
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
                    pickups[i].Warp(hit.position + (Vector3.up * pickups[i].baseOffset));
                    pickups[i].transform.SetParent(transform);
                    placed = true;

                    // Remove the agent from the list of unplaced agents
                    unplacedPickups.Remove(pickups[i]);
                }
                else
                {
                    Debug.Log("No valid position found.");
                }
            }

            // If the agent was not placed, remove it from the list of agents
            if (!placed)
            {
                pickups.RemoveAt(i);
                i--;
            }

            // If all agents have been placed, break out of the loop
            if (unplacedPickups.Count == 0)
            {
                break;
            }
        }
    }
}
