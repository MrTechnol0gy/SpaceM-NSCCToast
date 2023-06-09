using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AIEnemyForce : MonoBehaviour
{
    public CharacterController moveController;
    public LayerMask obstacleLayers;
    public GameObject enemy;    
    private GameObject player, distractionProbe;
    private Vector3 playerPOS;
    private Vector3 oldPlayerPOS;
    private Vector3 lastDesiredPositionGoing;
    private Vector3 intendedDestination;
    public Vector3 enemyPOS;
    private Vector3 enemyStoppedPOS, position, patrolDestination;
    public Transform weapon_hardpoint_1;        //"Weapon Hardpoint", "Transform for the barrel of the weapon"
    private float enemyToPlayerDistance;
    private bool canShoot = true;
    private Coroutine closeDistanceCoroutine;
    private float TimeStartedState;             // timer to know when we started a state
    private float TimeStartedDetectionUpdate;   // time to know when we started a detection range update
    private float lastTimeDidPatrolMove;        // holder for patrol timers
    private float lastTimeDidEnemyCheck;        // holder for search check timers    
    private float spawnRadius;                  // level area, will be grabbed from the Game Manager on Start
    private float enemyForceMinHeight, enemyForceMaxHeight;
    private float baseDetectionRange;
    private float distractedDuration;           // how long the enemy will remain distracted, as determined by the Distraction Probe
    private bool isDistracted = false;
    private bool isPlayerCloaked = false;       // holder for player cloak status
    public bool isPlayerInVisionCone = false;
    public LayerMask wallLayer;
    [SerializeField] Collider playerCollider;    

    [Header("Enemy Stats")]
    [SerializeField] float stoppedTime = 5f;        // how long the AI should remain in a stopped condition
    [SerializeField] float searchTime = 10f;         // how long the AI should search for the player
    [SerializeField] float patrolDelay = 10f;       // how long the AI will wait before choosing a new patrol path
    //[SerializeField] float patrolRadius = 50f;      // how large a sphere the AI will use to select a new patrol point from
    [SerializeField] public float detectionRange = 50f;    // how far the AI can see
    [SerializeField] public float detectionRangeUpdateSpeed = 1f; // how fast the AI updates their detection range
    [SerializeField] float detectionRangeUpdateFrequency = 3f; // how often the probe updates their detection range
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
        distracted      // distracted = 5
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
                enemyStoppedPOS = position;
                break;
            case States.patrolling:
                //Debug.Log("I am patrolling.");
                GetComponent<Renderer>().material.color = Color.blue;
                // Get a random point in the level
                patrolDestination = new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(enemyForceMinHeight, enemyForceMaxHeight), Random.Range(-spawnRadius, spawnRadius));                
                break;
            case States.chasing:
                //Debug.Log("I am chasing.");
                GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case States.searching:
                //Debug.Log("I am searching.");
                GetComponent<Renderer>().material.color = Color.green;
                break;
            case States.attacking:
                //Debug.Log("I am attacking.");
                GetComponent<Renderer>().material.color = Color.red;
                break;
            case States.distracted:
                //Debug.Log("I am distracted");
                GetComponent<Renderer>().material.color = Color.white;
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
                else if (isDistracted)
                {
                    currentState = States.distracted;
                }
                break;
            case States.patrolling:
                if (lastTimeDidPatrolMove + patrolDelay < Time.time)
                {
                    lastTimeDidPatrolMove = Time.time;
                }
                //If the player is within detection range, start chasing
                if (IsTargetVisible()) 
                {
                    //Debug.Log("I am patrolling and the player is visible");
                    currentState = States.chasing;
                }
                else if (isDistracted)
                {
                    currentState = States.distracted;
                }
                else
                {
                    // Gradually move towards the random position
                    // Debug.Log("Current location is " + transform.position + " destination is " + patrolDestination + " and movespeed is " + moveSpeed + ".");
                    Vector3 directionOfMove = (patrolDestination - transform.position).normalized;
                    lastDesiredPositionGoing = patrolDestination;
                    desiredDirectionOfMove = Vector3.Lerp(desiredDirectionOfMove, directionOfMove, Time.deltaTime * 6);
                    moveController.Move(desiredDirectionOfMove * moveSpeed * Time.deltaTime);
                    if (desiredDirectionOfMove == Vector3.zero) desiredDirectionOfMove = Vector3.forward;
                    transform.forward = Vector3.Lerp(transform.forward, desiredDirectionOfMove, Time.deltaTime * rotationSpeed);

                    if (Physics.CheckSphere(transform.position + (transform.forward * 2), 0.4f,obstacleLayers, QueryTriggerInteraction.Ignore)) {
                        OnStartedState(States.patrolling);
                    }
                }
                break;
            case States.chasing:
                if (IsTargetVisible())
                {
                    Vector3 directionOfPlayer = (player.transform.position - transform.position).normalized;
                    lastDesiredPositionGoing = player.transform.position;
                    desiredDirectionOfMove = Vector3.Lerp(desiredDirectionOfMove, directionOfPlayer, Time.deltaTime * 6);
                    moveController.Move(desiredDirectionOfMove * pursuitSpeed * Time.deltaTime);
                    if (desiredDirectionOfMove == Vector3.zero) desiredDirectionOfMove = Vector3.forward;
                    transform.forward = Vector3.Lerp(transform.forward, desiredDirectionOfMove, Time.deltaTime * rotationSpeed);
                }
                // If the player is no longer within detection range, start searching
                if (!IsTargetVisible())
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
                Vector3 directionOflastKnown = (lastKnownPosition - transform.position).normalized;
                lastDesiredPositionGoing = lastKnownPosition;
                desiredDirectionOfMove = Vector3.Lerp(desiredDirectionOfMove, directionOflastKnown, Time.deltaTime * 6);
                moveController.Move(desiredDirectionOfMove * moveSpeed * Time.deltaTime);
                if (desiredDirectionOfMove == Vector3.zero) desiredDirectionOfMove = Vector3.forward;
                transform.forward = Vector3.Lerp(transform.forward, desiredDirectionOfMove, Time.deltaTime * rotationSpeed);                
                if (IsTargetVisible()) 
                {
                    currentState = States.chasing;
                }
                else if (TimeElapsedSince(TimeStartedState, searchTime))
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
                    StopCloseDistanceCoroutine();
                    StartCoroutine(Shoot());
                    canShoot = false;
                }
                else if (!canShoot)
                {
                    StartCloseDistanceCoroutine();
                }
                break;
            case States.distracted:
                if (distractedDuration > 0)
                {
                    distractedDuration -= Time.deltaTime;
                    if (distractionProbe != null)
                    {
                        intendedDestination = distractionProbe.transform.position;
                        FaceProbe();
                        Vector3 directionOfMove = (distractionProbe.transform.position - transform.position).normalized;
                        desiredDirectionOfMove = Vector3.Lerp(desiredDirectionOfMove, directionOfMove, Time.deltaTime * 6);
                        moveController.Move(desiredDirectionOfMove * moveSpeed * Time.deltaTime);
                    }
                    else 
                    {
                        FaceProbe();
                        Vector3 directionOfMove = (intendedDestination - transform.position).normalized;
                        desiredDirectionOfMove = Vector3.Lerp(desiredDirectionOfMove, directionOfMove, Time.deltaTime * 6);
                        moveController.Move(desiredDirectionOfMove * moveSpeed * Time.deltaTime);
                    }
                }
                else if (distractedDuration <= 0)
                {
                    distractedDuration = 0;
                    currentState = States.stopped;
                }
                break;
        }
    }


    private Vector3 desiredDirectionOfMove;
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
            case States.distracted:
                isDistracted = false;
                break;
        }
    }
    
    void Start()
    {
        enemyPOS = this.transform.position;                    //gets starting position; utilized in setting spawnpoint on initialization
        player = GameObject.FindGameObjectWithTag("Player");   //gets the player gameobject        
        playerCollider = player.GetComponent<Collider>();      // assigns player collider to agent        
        baseDetectionRange = detectionRange;
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
        if (TimeElapsedSince(TimeStartedDetectionUpdate, detectionRangeUpdateFrequency))
        {
            TimeStartedDetectionUpdate = Time.time;
            DetectionRangeChange();
        }
    }
    
    // This method can be used to test if a certain time has elapsed since we registered an event time. 
    public bool TimeElapsedSince(float timeEventHappened, float testingTimeElapsed) => !(timeEventHappened + testingTimeElapsed > Time.time);

    private Vector3 lastKnownPosition;

    // method to check if the player is within attack range
    private bool PlayerWithinAttackRange()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= attackRange) 
        {
            return true;
        }
        //Debug.Log("my position is " + transform.position + " player pos is " + player.transform.position);
        //Debug.Log("Player is not within attack range." + attackRange + " distance " + distance);
        return false;
    }
    private void GetEnemyToPlayerDistance()
    {
        playerPOS = player.transform.position;                          //gets player's pos as a vector 3
        enemyPOS = this.transform.position;                             //gets the enemy's pos as a vector 3
        enemyToPlayerDistance = Vector3.Distance(enemyPOS, playerPOS);  //compares the difference between the enemy position and the player position
    }
    private float GetEnemyToPlayerDistanceForClosing()
    {
        playerPOS = player.transform.position;                          //gets player's pos as a vector 3
        enemyPOS = this.transform.position;                             //gets the enemy's pos as a vector 3
        enemyToPlayerDistance = Vector3.Distance(enemyPOS, playerPOS);  //compares the difference between the enemy position and the player position
        return enemyToPlayerDistance;
    }

    private bool IsTargetVisible()
    {        
        // checks if the player is cloaked first, and if so leaves the method and returns false
        if (PlayerCloakCheck())
        {
            //Debug.Log("Player is cloaked.");
            return false;
        }
        // checks if the player has entered the vision cone, and if not leaves the method and returns false
        else if (!isPlayerInVisionCone)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    // SetDistracted takes in the distracted duration and a reference to the distractionProbe for use in the state machine
    public void SetDistracted(float duration, GameObject distractionObject)
    {
        distractedDuration = duration;
        distractionProbe = distractionObject;
        isDistracted = true;
    }
    private void FacePlayer() {
        Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - weapon_hardpoint_1.transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
    private void FaceProbe()
    {
        Quaternion targetRotation = Quaternion.LookRotation(intendedDestination - weapon_hardpoint_1.transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
    private IEnumerator CloseDistance()
{
    float elapsedTime = 0;

    if (GetEnemyToPlayerDistanceForClosing() > 20f && elapsedTime < shootingInterval)
    {
        Vector3 directionOfPlayer = (player.transform.position - transform.position).normalized;
        moveController.Move(directionOfPlayer * moveSpeed * Time.deltaTime);
        transform.forward = Vector3.Lerp(transform.forward, directionOfPlayer, Time.deltaTime * rotationSpeed);

        elapsedTime += Time.deltaTime;
        yield return null;
    }
}
    private void StartCloseDistanceCoroutine()
    {
        closeDistanceCoroutine = StartCoroutine(CloseDistance());
    }
    private void StopCloseDistanceCoroutine()
    {
        if (closeDistanceCoroutine != null)
        {
            StopCoroutine(closeDistanceCoroutine);
            closeDistanceCoroutine = null;
        }
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
    private void DetectionRangeChange()
    {
        float playerSpeed = PlayerFlightControl.get.GetSpeed();
        if (playerSpeed <= Player.get.maxSpeed/2)
        {
            detectionRange = baseDetectionRange/2;
            //Debug.Log("My detection range is " + detectionRange);
        }
        else if (playerSpeed > Player.get.maxSpeed)
        {
            detectionRange = baseDetectionRange*2;
            //Debug.Log("My detection range is " + detectionRange);
        }
        else
        {
            detectionRange = baseDetectionRange;
            //Debug.Log("My detection range is " + detectionRange);
        }
    }

    public float ProbeDetectionRange()
    {
        return detectionRange;
    }
    public float ProbeDetectionRangeUpdateSpeed()
    {
        return detectionRangeUpdateSpeed;
    }

    private IEnumerator Shoot()
    {
    // Create a new bullet
    GameObject bullet = Instantiate(bulletPrefab, weapon_hardpoint_1.transform.position, Quaternion.identity);

    // Calculate the direction towards the predicted position of the player
    Vector3 playerPosition = player.transform.position;
    Vector3 playerVelocity = player.GetComponent<Rigidbody>().velocity;
    float timeToReachPlayer = Vector3.Distance(bullet.transform.position, playerPosition) / bulletSpeed;
    Vector3 predictedPosition = playerPosition + playerVelocity * timeToReachPlayer;
    Vector3 direction = (predictedPosition - bullet.transform.position).normalized;

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
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,lastDesiredPositionGoing);
        Gizmos.DrawSphere(lastDesiredPositionGoing, 5);
    }
    #endif
}