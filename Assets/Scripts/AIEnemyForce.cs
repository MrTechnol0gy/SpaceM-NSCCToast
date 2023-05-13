using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AIEnemyForce : MonoBehaviour
{
    public GameObject enemy;    
    private GameObject player;
    private Vector3 playerPOS;
    private Vector3 oldPlayerPOS;
    public Vector3 enemyPOS;
    public Transform weapon_hardpoint_1;        //"Weapon Hardpoint", "Transform for the barrel of the weapon"
    private float enemyToPlayerDistance;
    private bool wallHit = false;
    private bool canShoot = true;
    private float TimeStartedState;             // timer to know when we started a state
    private float lastTimeDidPatrolMove;        // holder for patrol timers
    private float lastTimeDidEnemyCheck;        // holder for search check timers    
    private float spawnRadius;                  // level area, will be grabbed from the Game Manager on Start
    private float enemyForceMinHeight, enemyForceMaxHeight;
    private bool isPlayerCloaked = false;       // holder for player cloak status
    public LayerMask wallLayer;
    [SerializeField] Collider playerCollider;    

    [Header("Enemy Stats")]
    [SerializeField] float stoppedTime = 5f;        // how long the AI should remain in a stopped condition
    [SerializeField] float searchTime = 10f;         // how long the AI should search for the player
    [SerializeField] float patrolDelay = 10f;       // how long the AI will wait before choosing a new patrol path
    [SerializeField] float patrolRadius = 50f;      // how large a sphere the AI will use to select a new patrol point from
    [SerializeField] float detectionRange = 50f;    // how far the AI can see
    [SerializeField] float attackRange = 25f;       // the range at which the AI can attack
    [SerializeField] float moveSpeed = 12f;         // movespeed for the enemy
    [SerializeField] float pursuitSpeed = 18f;      // speed the enemy moves when it detects the player
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
                //Debug.Log("I am stopped.");
                GetComponent<Renderer>().material.color = Color.black;
                break;
            case States.patrolling:
                //Debug.Log("I am patrolling.");
                GetComponent<Renderer>().material.color = Color.blue;                
                break;
            case States.chasing:
                //Debug.Log("I am chasing.");
                GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case States.searching:
                //Debug.Log("I am searching.");
                GetComponent<Renderer>().material.color = Color.green;
                oldPlayerPOS = player.transform.position;
                break;
            case States.attacking:
                //Debug.Log("I am attacking.");
                GetComponent<Renderer>().material.color = Color.red;
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
                 if (lastTimeDidPatrolMove + patrolDelay < Time.time)
                {
                    lastTimeDidPatrolMove = Time.time;
                    // Get a random point in the level
                    Vector3 position = new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(enemyForceMinHeight, enemyForceMaxHeight), Random.Range(-spawnRadius, spawnRadius));

                    // Set the agent's destination to the random point in the level
                    transform.position = Vector3.Lerp(transform.position, position, moveSpeed * Time.deltaTime);
                }

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
        enemyPOS = this.transform.position;                    //gets starting position; utilized in setting spawnpoint on initialization
        player = GameObject.FindGameObjectWithTag("Player");   //gets the player gameobject        
        playerCollider = player.GetComponent<Collider>();      // assigns player collider to agent        
        
        spawnRadius = GameManager.get.spawnRadius;
        enemyForceMinHeight = EnemyManager.get.enemyForceMinHeight;
        enemyForceMaxHeight = EnemyManager.get.enemyForceMaxHeight;
        Radar.instance.CreateTarget(transform,false);
        // Add the newly created Agent to the list of All agents
        EnemyManager.allEnemies.Add(enemy);
        OnStartedState(currentState);
    }

    private void OnDestroy() {
        if(EnemyManager.allEnemies.Contains(enemy)) EnemyManager.allEnemies.Remove(enemy);
        Radar.instance.RemoveTarget(transform);
    }

    void Update()
    {
        OnUpdatedState(currentState);
    }
    
    // This method can be used to test if a certain time has elapsed since we registered an event time. 
    public bool TimeElapsedSince(float timeEventHappened, float testingTimeElapsed) => !(timeEventHappened + testingTimeElapsed > Time.time);

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
        // checks if the player is cloaked first, and if so leaves the method and returns false
        if (PlayerCloakCheck())
        {
            return false;
        }
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
        if (lastTimeDidEnemyCheck.IntervalElapsedSince(0.33f))
        {
            lastTimeDidEnemyCheck = Time.time;
            if (PlayerWithinDetectionRange() && IsTargetVisible())
            {
                Debug.Log("Moving towards the player.");
                Vector3 targetPosition = player.transform.position;
                transform.position = Vector3.Lerp(transform.position, targetPosition, pursuitSpeed * Time.deltaTime);
            }
        }
    }

    private void MoveTowardsLastSeenPosition()
    {
        Vector3 targetPosition = oldPlayerPOS;
        transform.position = Vector3.Lerp(transform.position, targetPosition, pursuitSpeed * Time.deltaTime);
    }

    private void FacePlayer()
    {
        Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - weapon_hardpoint_1.transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        
    }
    private bool PlayerCloakCheck()
    {
        isPlayerCloaked = PlayerFlightControl.get.cloakActive;
        if (isPlayerCloaked)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private IEnumerator Shoot()
    {
            // Create a new bullet
            GameObject bullet = Instantiate(bulletPrefab, weapon_hardpoint_1.transform.position, Quaternion.identity);

            // Calculate the direction towards the player
            Vector3 direction = (player.transform.position - bullet.transform.position).normalized;

            // Set the bullet's velocity
            bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;

            // Wait for the specified shooting interval before being able to shoot again
            yield return new WaitForSeconds(shootingInterval);

            canShoot = true;
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