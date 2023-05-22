using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaHot : MonoBehaviour
{
    [SerializeField] int lavaDamage = 20;
    [SerializeField] float damageInterval = 1f;
    private float elapsedTime = 0f;
    private bool isPlayerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            elapsedTime = 0f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isPlayerInside)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= damageInterval)
                {
                    GameTimer.get.DecreaseTime(lavaDamage);
                    elapsedTime = 0f;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            elapsedTime = 0f;
        }
    }
}
