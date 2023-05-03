using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pickup : MonoBehaviour
{
    public NavMeshAgent agent;
    [Header("Pickup Configuration")]
    [SerializeField] float distanceThreshold = 2f;
    private void OnTriggerStay(Collider other)
    {        
        if (other.CompareTag("Player"))
        {           
            float distanceToCharacter = Vector3.Distance(transform.position, Player.get.transform.position);

            if (distanceToCharacter <= distanceThreshold)
            {
                GameManager.get.CollectIntel();
                //removes this pickup from the list of pickupables in range on the player
                if (Player.get.pickupablesInRange.Contains(gameObject))
                    Player.get.pickupablesInRange.Remove(gameObject);
                // sets this pickup to be inactive
                gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        // Add the newly created Agent to the list of All agents
        PickUpManager.allPickups.Add(agent);
        Radar.instance.CreateTarget(transform, true);
    }

    private void OnDisable()
    {
        if(PickUpManager.allPickups.Contains(agent)) PickUpManager.allPickups.Remove(agent);
        Radar.instance.RemoveTarget(transform);
    }
}
