using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsPlayerInMyCollider : MonoBehaviour
{
    private GameObject player;
    private Transform playerTransform;
    AIEnemyForce aIEnemyForce;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");   //gets the player gameobject
        playerTransform = player.transform;
        aIEnemyForce = GetComponentInParent<AIEnemyForce>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == playerTransform)
        {
            aIEnemyForce.isPlayerInVisionCone = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform == playerTransform)
        {
            aIEnemyForce.isPlayerInVisionCone = false;
        }
    }
}
