using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIEnemyForce : MonoBehaviour
{
    public NavMeshAgent enemy;    
    private GameObject playerAgent;
    private Vector3 playerPOS;
    private Vector3 oldPlayerPOS;
    public Vector3 enemyPOS;
    private float enemyToPlayerDistance;
    private float closeProximity = 5;
    private float distantProximity = 25;
    private bool playerVisible = false;
    private bool wallHit = false;
    private float TimeStartedState;             // timer to know when we started a state
    public LayerMask wallLayer;
    [SerializeField] Collider playerCollider;    
    
    public enum States
    {
        stopped,        // stopped = 0
        patrolling,     // patrolling = 1
        chasing,        // chasing = 2
        searching,      // searching = 3
        attacking,      // attacking = 4
    }
    private States _currentState = States.stopped;       //sets the starting enemy state    
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
                GetComponent<Renderer>().material.color = Color.red;
                break;
            case States.patrolling:
                GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case States.chasing:
                GetComponent<Renderer>().material.color = Color.blue;
                break;
            case States.searching:
                GetComponent<Renderer>().material.color = Color.green;
                break;
            case States.attacking:
                GetComponent<Renderer>().material.color = Color.black;
                break;
        }
    }
    // OnUpdatedState is for things that occur during the state (main actions)
    public void OnUpdatedState(States state) 
    {
        switch (state) 
        {
            case States.stopped:

                break;
            case States.patrolling:
 
                break;
            case States.chasing:
 
                break;
            case States.searching:
 
                break;
            case States.attacking:
 
                break;
        }
    }
    // OnEndedState is for things that should end or change when a state ends; for cleanup
    public void OnEndedState(States state) 
    {
        switch (state) 
        {
            case States.stopped:
                break;
            case States.patrolling:
                break;
            case States.chasing:
                break;
            case States.searching:
                break;
            case States.attacking:
                break;
        }
    }
    
    void Start()
    {
        enemyPOS = this.transform.position;                         //gets starting position; utilized in setting spawnpoint on initialization
        playerAgent = GameObject.FindGameObjectWithTag("Player");   //gets the player gameobject        
        playerCollider = playerAgent.GetComponent<Collider>();      // assigns player collider to agent        

        OnStartedState(currentState);
    }

    void Update()
    {
        OnUpdatedState(currentState);
    }
    public void GetEnemyToPlayerDistance()
    {
        playerPOS = playerAgent.transform.position;                     //gets player's pos as a vector 3
        enemyPOS = this.transform.position;                             //gets the enemy's pos as a vector 3
        enemyToPlayerDistance = Vector3.Distance(enemyPOS, playerPOS);  //compares the difference between the enemy position and the player position
    }

    public void IsTargetVisible()
    {        
        Ray ray = new Ray (enemyPOS, playerPOS - enemyPOS);                         //casts a ray from the enemy agent towards the player's position 
        Debug.DrawRay(enemyPOS, (playerPOS - enemyPOS) * 10);                       // visualizes the raycast for debugging
        RaycastHit hitData;
        wallHit = false;
        playerVisible = false;
        if (Physics.Raycast(ray, out hitData, enemyToPlayerDistance, wallLayer))    //checks for walls between the player and the enemy
        {
            wallHit = true;
            //Debug.Log("Wall has been hit.");
        }        
        else
        {
            wallHit = false;
            //Debug.Log("wall has not been hit.");

            if (playerCollider.Raycast(ray, out hitData, distantProximity))         // checks for the player's visibility within "sight" range
            {
                playerVisible = true;
                //Debug.Log("Player is visible.");
            }
            else
            {
                playerVisible = false;
                //Debug.Log("Player is not visible.");
            }
        }
    }
    // This method can be used to test if a certain time has elapsed since we registered an event time. 
    public bool TimeElapsedSince(float timeEventHappened, float testingTimeElapsed) => !(timeEventHappened + testingTimeElapsed > Time.time);
}