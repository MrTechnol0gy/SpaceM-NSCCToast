using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIEnemyForce : MonoBehaviour
{
    public NavMeshAgent enemy;    
    private GameObject player;
    private Vector3 playerPOS;
    private Vector3 oldPlayerPOS;
    public Vector3 enemyPOS;
    public Transform weapon_hardpoint_1;        //"Weapon Hardpoint", "Transform for the barrel of the weapon"
    private float enemyToPlayerDistance;    
    private bool wallHit = false;
    private bool canShoot = true;
    private float TimeStartedState;             // timer to know when we started a state
    public LayerMask wallLayer;
    [SerializeField] Collider playerCollider;    

    [Header("Enemy Stats")]
    [SerializeField] float stoppedTime = 5f;        // how long the AI should remain in a stopped condition
    [SerializeField] float searchTime = 10f;         // how long the AI should search for the player
    [SerializeField] float patrolDelay = 10f;       // how long the AI will wait before choosing a new patrol path
    [SerializeField] float patrolRadius = 50f;      // how large a sphere the AI will use to select a new patrol point from
    [SerializeField] float detectionRange = 50f;    // how far the AI can see
    [SerializeField] float attackRange = 25f;       // the range at which the AI can attack
    [SerializeField] float rotationSpeed = 5f;      // how fast the AI can rotate
    [SerializeField] GameObject bulletPrefab;       // the projectile prefab for the AI
    [SerializeField] float bulletSpeed = 10f;       // the projectile speed
    [SerializeField] float shootingInterval = 3f;   // duration between shots
    
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
                Debug.Log("I am stopped.");
                GetComponent<Renderer>().material.color = Color.black;
                enemy.isStopped = true;
                break;
            case States.patrolling:
                Debug.Log("I am patrolling.");
                GetComponent<Renderer>().material.color = Color.blue;
                StartCoroutine(Patrol());                               //begins a patrol
                break;
            case States.chasing:
                Debug.Log("I am chasing.");
                GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case States.searching:
                Debug.Log("I am searching.");
                GetComponent<Renderer>().material.color = Color.green;
                oldPlayerPOS = player.transform.position;
                break;
            case States.attacking:
                Debug.Log("I am attacking.");
                GetComponent<Renderer>().material.color = Color.red;
                enemy.isStopped = true;
                break;
        }
    }
    // OnUpdatedState is for things that occur during the state (main actions)
    public void OnUpdatedState(States state) 
    {
        switch (state) 
        {
            case States.stopped:
                if (TimeElapsedSince(TimeStartedState, stoppedTime))
                {
                    currentState = States.patrolling;
                }
                break;
            case States.patrolling:
                // If the player is within detection range, start chasing
                if (PlayerWithinDetectionRange()) 
                {
                    if (IsTargetVisible())
                    {
                        currentState = States.chasing;
                    }
                }
                break;
            case States.chasing:
                FacePlayer();
                MoveTowardsPlayer();
                // If the player is no longer within detection range, start searching
                if (!PlayerWithinDetectionRange()) 
                {
                    currentState = States.searching;
                }
                // If the player is within attack range, start attacking
                else if (PlayerWithinAttackRange()) 
                {
                    currentState = States.attacking;
                }
                break;
            case States.searching:
                MoveTowardsLastSeenPosition();
                if (PlayerWithinDetectionRange()) 
                {
                    if (IsTargetVisible())
                    {
                        currentState = States.chasing;
                    }
                }
                else if (TimeElapsedSince(TimeStartedState, stoppedTime))
                {
                    currentState = States.stopped;
                }
                break;
            case States.attacking:
                FacePlayer();
                // If the player is no longer within attack range, start chasing
                if (!PlayerWithinAttackRange()) 
                {
                    currentState = States.chasing;
                }
                else if (canShoot)
                {
                    StartCoroutine(Shoot());
                    canShoot = false;
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
                enemy.isStopped = false;
                break;
            case States.patrolling:
                //Debug.Log("Patrolling ended.");
                StopCoroutine(Patrol());                            // stops the patrol coroutine
                break;
            case States.chasing:
                break;
            case States.searching:
                break;
            case States.attacking:
                enemy.isStopped = false;
                break;
        }
    }
    
    void Start()
    {
        enemyPOS = this.transform.position;                    //gets starting position; utilized in setting spawnpoint on initialization
        player = GameObject.FindGameObjectWithTag("Player");   //gets the player gameobject        
        playerCollider = player.GetComponent<Collider>();      // assigns player collider to agent        

        OnStartedState(currentState);
    }

    void Update()
    {
        OnUpdatedState(currentState);
    }
    
    // This method can be used to test if a certain time has elapsed since we registered an event time. 
    public bool TimeElapsedSince(float timeEventHappened, float testingTimeElapsed) => !(timeEventHappened + testingTimeElapsed > Time.time);

    IEnumerator Patrol()
    {
        while (_currentState == States.patrolling)
        {
            // Get a random point on the NavMesh
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            NavMeshHit hit;
            NavMesh.SamplePosition(transform.position + randomDirection, out hit, patrolRadius, NavMesh.AllAreas);

            // Set the agent's destination to the random point on the NavMesh
            enemy.SetDestination(hit.position);

            // Wait for the specified delay before moving again
            yield return new WaitForSeconds(patrolDelay);
        }
    }
    // method to check if the player is within detection range
    private bool PlayerWithinDetectionRange()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= detectionRange)
        {
            return true;
        }
        return false;
    }

    // method to check if the player is within attack range
    private bool PlayerWithinAttackRange()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= attackRange) 
        {
            return true;
        }
        return false;
    }
    private void GetEnemyToPlayerDistance()
    {
        playerPOS = player.transform.position;                          //gets player's pos as a vector 3
        enemyPOS = this.transform.position;                             //gets the enemy's pos as a vector 3
        enemyToPlayerDistance = Vector3.Distance(enemyPOS, playerPOS);  //compares the difference between the enemy position and the player position
    }

    private bool IsTargetVisible()
    {        
        Ray ray = new Ray (enemyPOS, playerPOS - enemyPOS);                         //casts a ray from the enemy agent towards the player's position 
        Debug.DrawRay(enemyPOS, (playerPOS - enemyPOS) * 10);                       // visualizes the raycast for debugging
        RaycastHit hitData;
        wallHit = false;
        GetEnemyToPlayerDistance();
        if (Physics.Raycast(ray, out hitData, enemyToPlayerDistance, wallLayer))    //checks for walls between the player and the enemy
        {
            wallHit = true;
            //Debug.Log("Wall has been hit.");
            return false;
        }        
        else
        {
            wallHit = false;
            //Debug.Log("wall has not been hit.");

            if (playerCollider.Raycast(ray, out hitData, detectionRange))         // checks for the player's visibility within "sight" range
            {
                return true;
                //Debug.Log("Player is visible.");
            }
            else
            {
                return false;
                //Debug.Log("Player is not visible.");
            }
        }
    }
    private void MoveTowardsPlayer()
    {
        if (PlayerWithinDetectionRange() && IsTargetVisible())
        {
            Debug.Log("Moving towards the player.");
            enemy.SetDestination(Player.get.transform.position);
        }
    }

    private void MoveTowardsLastSeenPosition()
    {
        enemy.SetDestination(oldPlayerPOS);
    }

    private void FacePlayer()
    {
        Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        
    }
    private IEnumerator Shoot()
    {
            // Create a new bullet
            GameObject bullet = Instantiate(bulletPrefab, weapon_hardpoint_1.transform.position, Quaternion.identity);

            // Calculate the direction towards the player
            Vector3 direction = (player.transform.position - transform.position).normalized;

            // Set the bullet's velocity
            bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;

            // Wait for the specified shooting interval before being able to shoot again
            yield return new WaitForSeconds(shootingInterval);

            canShoot = true;
    }
}