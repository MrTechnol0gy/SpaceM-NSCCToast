using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player get;    
    public LayerMask pickupableLayerMask;
    public List<GameObject> pickupablesInRange;

    [Header("Player Stats")]
    [SerializeField] float TractorSpeed = 1f;       // the speed at which a tractored object will be drawn towards the player
    [SerializeField] float InteractionRange = 20f;  // the range of the interaction sphere collider

    private SphereCollider interactionSphere;
    void Awake()
    {
        get = this;
    }    

    void Start()
    {

    }

    void Update()
    {        
        if (PlayerFlightControl.get.tractorBeamActive && pickupablesInRange.Count > 0)
        {
            Vector3 targetPos = transform.position;
            float closestDistance = Mathf.Infinity;
            int closestIndex = 0;
            
            // Loop through all pickupables in range and find the closest one
            for (int i = 0; i < pickupablesInRange.Count; i++)
            {
                float distance = Vector3.Distance(pickupablesInRange[i].transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            // Lerp only the closest pickupable towards the player
            pickupablesInRange[closestIndex].transform.position = Vector3.Lerp(
                pickupablesInRange[closestIndex].transform.position, targetPos, TractorSpeed * Time.deltaTime);

            // Deactivate the tractor beam when the closest pickupable is close enough to the player
            if (closestDistance < 5f)
            {
                PlayerFlightControl.get.tractorBeamActive = false;
            }
        }

        //Debug.Log("Pickupables in range: " + pickupablesInRange.Count);
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the gameobject that entered our trigger is not in the pickupables layer mask then stop this method here.
        if (!other.gameObject.IsInLayerMask(pickupableLayerMask)) return;
        // If the code got to that point we can do the stuff we want to do to our pickupable
        // Here I'll add it to the list if the list doesn't already contain it.
        if (!pickupablesInRange.Contains(other.gameObject)) pickupablesInRange.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    { 
        // If the gameobject that entered our trigger is not in the pickupables layer mask then stop this method here.
        if (!other.gameObject.IsInLayerMask(pickupableLayerMask)) return;
        // If the code got to that point we can do the stuff we want to do to our pickupable
        // Here I'll remove it from the list if the list contain it.
        if (pickupablesInRange.Contains(other.gameObject)) pickupablesInRange.Remove(other.gameObject);
    }
}
