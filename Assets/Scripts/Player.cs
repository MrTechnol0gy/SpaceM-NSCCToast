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
    public int intelCollected = 0;                  // the amount of intel pickups collected

    private SphereCollider interactionSphere;
    void Awake()
    {
        get = this;
    }    

    void Start()
    {
        UIManager.SetIntelCurrentAmount(intelCollected);
        // interactionSphere = GetComponentInChildren<SphereCollider>();
        // interactionSphere.radius = InteractionRange;
        // Vector3 colliderCenter = interactionSphere.transform.localPosition;
        // colliderCenter.z = InteractionRange * 1.25f;
        // interactionSphere.transform.localPosition = colliderCenter;
    }

    void Update()
    {        
        if (PlayerFlightControl.get.tractorBeamActive && pickupablesInRange.Count > 0)
        {
            Vector3 targetPos = transform.position;
            pickupablesInRange[0].transform.position = Vector3.Lerp(pickupablesInRange[0].transform.position, targetPos, TractorSpeed * Time.deltaTime);

            if (Vector3.Distance(pickupablesInRange[0].transform.position, targetPos) < 5f)
            {
                PlayerFlightControl.get.tractorBeamActive = false;
            }
        }
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

    public void CollectIntel()
    {
        intelCollected++;
        UIManager.SetIntelCurrentAmount(intelCollected);
    }
}
