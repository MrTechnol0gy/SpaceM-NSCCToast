using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pickup : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    
    public GameObject agent;
    
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
                if (Player.get.pickupablesInRange.Contains(this))
                    Player.get.pickupablesInRange.Remove(this);
                // sets this pickup to be inactive
                gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        randomOffset = UnityEngine.Random.value * 100;
        // Add the newly created Agent to the list of All agents
        PickUpManager.allPickups.Add(agent);
        Radar.instance.CreateTarget(transform, true);
    }

    private float randomOffset;
    public float bobSpeed = 0, bobAmplitude = 0;
    private void Update()
    {
        meshRenderer.transform.localPosition = Vector3.up * (Mathf.Sin((Time.time + randomOffset) * bobSpeed) * bobAmplitude);
        meshRenderer.transform.Rotate(0,Time.deltaTime * bobSpeed*10f,0);
    }

    private void OnDisable()
    {
        if(PickUpManager.allPickups.Contains(agent)) PickUpManager.allPickups.Remove(agent);
        Radar.instance.RemoveTarget(transform);
    }
}
