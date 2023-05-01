using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("Pickup Configuration")]
    [SerializeField] float distanceThreshold = 2f;
    private void OnTriggerStay(Collider other)
    {        
        if (other.CompareTag("Player"))
        {           
            float distanceToCharacter = Vector3.Distance(transform.position, Player.get.transform.position);

            if (distanceToCharacter <= distanceThreshold)
            {
                Player.get.CollectIntel();
                // PickUpManager.get.RemovePickupable(this);
                gameObject.SetActive(false);
            }
        }
    }
}
