using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player get;    
    public LayerMask pickupableLayerMask;
    public List<GameObject> pickupablesInRange;

    [Header("Player Stats")]
    [SerializeField] float TractorSpeed = 1f;               // the speed at which a tractored object will be drawn towards the player
    [SerializeField] float InteractionRange = 20f;          // the range of the interaction sphere collider
    [SerializeField] public float maxSpeed = 20f;           // gets sent to PlayerFlightControl
    [SerializeField] public float afterburnerSpeed = 30f;   // gets sent to PlayerFlightControl
    [SerializeField] public float cloakSpeed = 2f;          // amount to divide player speed by when under cloak
    [SerializeField] public float probeDuration = 8f;       // duration of distraction probes launched by the player
    [SerializeField] public float probeDelayBetweenShots = 15f; // cooldown for probes
    [SerializeField] float shieldDelay = 10f;               // time it takes for shield to regen
    public bool allowPitch = false;                         // toggle to allow movement on the Y axis to the player
    public bool shieldActive = true;

    private SphereCollider interactionSphere;
    public Material shieldMaterial;
    private float activeDissolveAmount = 0.75f, inactiveDissolveAmount = 1.2f, lerpDuration = 0.2f;
    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;
    void Awake()
    {
        get = this;
    }
    void Start()
    {
        shieldMaterial.SetFloat("_DissolveAmount", activeDissolveAmount);
        particleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
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

            if (pickupablesInRange[closestIndex] != null)
            {
                //Debug.Log("Tractor beam particles are active.");
                particleSystem.Play();
                int numParticlesAlive = particleSystem.GetParticles(particles);

                for (int i = 0; i < numParticlesAlive; i++)
                {
                    Vector3 directionToTarget = pickupablesInRange[closestIndex].transform.position - particles[i].position;
                    particles[i].velocity = directionToTarget.normalized * particleSystem.main.startSpeedMultiplier;
                }

                particleSystem.SetParticles(particles, numParticlesAlive);
            }

            // Lerp only the closest pickupable towards the player
            pickupablesInRange[closestIndex].transform.position = Vector3.Lerp(
                pickupablesInRange[closestIndex].transform.position, targetPos, TractorSpeed * Time.deltaTime);

            // Deactivate the tractor beam when the closest pickupable is close enough to the player
            if (closestDistance < 5f)
            {
                PlayerFlightControl.get.tractorBeamActive = false;
                particleSystem.Stop();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && UIManager.get.isShowingMenu())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (!shieldActive)
        {
            ShieldDown();
            StartCoroutine(RestartShield());
        }
        else if (shieldActive)
        {
            ShieldUp();
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
    public bool IsShieldActive()
    {
        return shieldActive;
    }
    private IEnumerator RestartShield()
    {
        yield return new WaitForSeconds(shieldDelay);
        shieldActive = true;
        //Debug.Log("Shield is reactivated.");
    }
    private void ShieldUp()
    {
        if (shieldMaterial.GetFloat("_DissolveAmount") > 0.8f)
        {
            float t = Mathf.Clamp01(Time.deltaTime / lerpDuration);
            shieldMaterial.SetFloat("_DissolveAmount", Mathf.Lerp(activeDissolveAmount, inactiveDissolveAmount, t));
        }
        else
        {
            shieldMaterial.SetFloat("_DissolveAmount", activeDissolveAmount);
        }
    }
    private void ShieldDown()
    {
        if (shieldMaterial.GetFloat("_DissolveAmount") < 1.15f)
        {
            float t = Mathf.Clamp01(Time.deltaTime / lerpDuration);
            shieldMaterial.SetFloat("_DissolveAmount", Mathf.Lerp(inactiveDissolveAmount, activeDissolveAmount, t));
        }
        else
        {
            shieldMaterial.SetFloat("_DissolveAmount", inactiveDissolveAmount);
        }
    }
}
