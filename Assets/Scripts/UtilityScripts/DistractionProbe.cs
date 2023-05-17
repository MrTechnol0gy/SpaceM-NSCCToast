using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistractionProbe : MonoBehaviour
{
    public float probeDuration; // Duration of the distraction effect

    void Start()
    {
        probeDuration = Player.get.probeDuration;
    }
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has an enemy component
        AIEnemyForce enemy = other.gameObject.GetComponentInParent<AIEnemyForce>();
        if (enemy != null)
        {
            enemy.SetDistracted(probeDuration, gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Destroy the probe
        Destroy(gameObject);
    }
}
