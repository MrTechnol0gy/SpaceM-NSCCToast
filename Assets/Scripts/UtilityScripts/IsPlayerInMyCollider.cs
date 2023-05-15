using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsPlayerInMyCollider : MonoBehaviour
{
    private GameObject player;
    private Transform playerTransform;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");   //gets the player gameobject
        playerTransform = player.transform;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == playerTransform)
        {
            AIEnemyForce.get.isPlayerInVisionCone = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform == playerTransform)
        {
            AIEnemyForce.get.isPlayerInVisionCone = false;
        }
    }
}
