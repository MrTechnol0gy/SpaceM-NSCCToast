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
                GameManager.get.CollectIntel();
                //removes this pickup from the list of pickupables in range on the player
                if (Player.get.pickupablesInRange.Contains(gameObject))
                    Player.get.pickupablesInRange.Remove(gameObject);
                // sets this pickup to be inactive
                gameObject.SetActive(false);
            }
        }
    }
}
