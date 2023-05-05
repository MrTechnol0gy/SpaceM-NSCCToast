using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class AIEnvironmentalEffect : MonoBehaviour
{
    public NavMeshAgent environmentEffect;
    private GameObject player;
    private Vector3 playerPOS;
    private Vector3 oldPlayerPOS;
    private VisualEffect visualEffect;
    private float TimeStartedState;             // timer to know when we started a state
    private float durationOfLife;               // var to hold duration of VFX
    private float lastTimeDidPatrolMove;        // holder for patrol timers
    private float lastTimeDidEnemyCheck;        // holder for search check timers
    public LayerMask wallLayer;
    [SerializeField] Collider playerCollider;

    [Header("Environment Effect Stats")]
    [SerializeField] float stoppedTime = 5f;        // how long the AI should remain in a stopped condition   
    [SerializeField] float inactiveTime = 15f;      // how long the AI should remain in an inactive state
    [SerializeField] float patrolDelay = 10f;       // how long the AI will wait before choosing a new patrol path
    [SerializeField] float patrolRadius = 50f;      // how large a sphere the AI will use to select a new patrol point from   
    [SerializeField] float rotationSpeed = 5f;      // how fast the AI can rotate
    
    public enum States
    {
        stopped,        // stopped = 0
        patrolling,     // patrolling = 1
        inactive        // inactive = 2
    }
    private States _currentState = States.inactive;       //sets the starting enemy state
    public States currentState 
    {
        get => _currentState;
        set {
            if (_currentState != value) 
            {
                // Calling ended state for the previous state registered.
                OnEndedState(_currentState);
                
                // Setting the new current state
                _currentState = value;
                
                // Registering here the time we're starting the state
                TimeStartedState = Time.time;
                
                // Call the started state method for the new state.
                OnStartedState(_currentState);
            }
        }
    }
    // OnStartedState is for things that should happen when a state first begins
    public void OnStartedState(States state)
    {
        switch (state) 
        {
            case States.stopped:
                Debug.Log("I am stopped.");
                environmentEffect.isStopped = true;
                visualEffect.enabled = true;
                break;
            case States.patrolling:
                Debug.Log("I am patrolling.");
                break;
            case States.inactive:
                Debug.Log("I am inactive.");
                visualEffect.enabled = false;
                break;
        }
    }
    // OnUpdatedState is for things that occur during the state (main actions)
    public void OnUpdatedState(States state) 
    {
        switch (state) 
        {
            case States.stopped:
                // keeps the effect in place while it establishes it's presence
                if (TimeElapsedSince(TimeStartedState, stoppedTime))
                {
                    currentState = States.patrolling;
                }
                break;
            case States.patrolling:
                // if effect is reaching the end of it's lifespan, changes state to inactive
                if (TimeElapsedSince(TimeStartedState - stoppedTime, durationOfLife))
                {
                    currentState = States.inactive;
                }
                // if effect is still active, perform a new patrol move
                else if (lastTimeDidPatrolMove + patrolDelay < Time.time)
                {
                    lastTimeDidPatrolMove = Time.time;
                    // randomly decides if the effect will move towards the player or a random point on the map
                    int randomNumber = Random.Range(1, 11);
                    if (randomNumber < 6)
                    {
                        // Get a random point on the NavMesh
                        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
                        NavMesh.SamplePosition(transform.position + randomDirection, out NavMeshHit hit, patrolRadius,
                            NavMesh.AllAreas);

                        // Set the agent's destination to the random point on the NavMesh
                        environmentEffect.SetDestination(hit.position);
                    }
                    else
                    {
                        MoveTowardsPlayer();
                    }
                }
                break;
            case States.inactive:
                if (TimeElapsedSince(TimeStartedState, inactiveTime))
                {
                    currentState = States.stopped;
                }
                break; 
        }
    }
    // OnEndedState is for things that should end or change when a state ends; for cleanup
    public void OnEndedState(States state)
    {
        switch (state) 
        {
            case States.stopped:
                environmentEffect.isStopped = false;
                break;
            case States.patrolling:
                break;
            case States.inactive:
                break;
        }
    }
    
    void Start()
    {
        visualEffect = GetComponent<VisualEffect>();
        durationOfLife = visualEffect.GetFloat("Duration");     // gets the duration of the vfx
        player = PlayerFlightControl.get.gameObject;            //gets the player gameobject
        playerCollider = player.GetComponent<Collider>();       // assigns player collider to agent

        OnStartedState(currentState);
    }

    void Update()
    {
        OnUpdatedState(currentState);
    }
    
    // This method can be used to test if a certain time has elapsed since we registered an event time. 
    public bool TimeElapsedSince(float timeEventHappened, float testingTimeElapsed) => !(timeEventHappened + testingTimeElapsed > Time.time);

    private void MoveTowardsPlayer()
    {
        if (lastTimeDidEnemyCheck.IntervalElapsedSince(0.33f))
        {
            lastTimeDidEnemyCheck = Time.time;            
            Debug.Log("Moving towards the player.");
            environmentEffect.SetDestination(Player.get.transform.position);
        }
    }
    private void FacePlayer()
    {
        Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        
    }
    
    // I use Handles.Label to show a label with the current state above the player. Can use it for more debug info as well.
    // I wrap it around a #if UNITY_EDITOR to make sure it doesn't make its way into the build, unity doesn't like using UnityEditor methods in builds.
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //If we're not playing don't draw gizmos.
        if (!Application.isPlaying) return;
        
        //Setting the position for our debug label and the color.
        Vector3 debugPos = transform.position;
        debugPos.y += 2; 
        GUI.color = Color.black;
        UnityEditor.Handles.Label(debugPos,$"{currentState}");
        
        //Let's also do an extra debug if we're stopped to say how long until we're going up.
        debugPos.y += 1; 
        if (currentState == States.patrolling) {
            UnityEditor.Handles.Label(debugPos,$"{patrolDelay - (Time.time - lastTimeDidPatrolMove)} till patrolling to a new location.");
        }
    }
    #endif
}