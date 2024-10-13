using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster1 : MonoBehaviour
{
    public enum MonsterState
    {
        Patrolling,
        Chasing,
        Climbing, 
        Attacking,
        Dead,
        None
    }
    private PlayerMovement playerMovement; // Reference to the PlayerMovement script
    // Monster Properties
    public MonsterState curState = MonsterState.None;  // Default state
    private NavMeshAgent agent;           // Reference to the NavMeshAgent component
    
    public Transform[] patrolPoints;  // Waypoints for patrolling
    public GameObject explosionPrefab;
    private float moveSpeed = 3f;          // Speed while chasing
    private float patrolSpeed = 2f;        // Speed while patrolling
    public float health = 100f;           // Health of the monster
    public float attackRadius = 3f;       // Attack radius
    public float chaseDistance = 60.0f;   // Distance to start chasing the player

    private int currentPatrolPoint = 0;
    private bool hasDied = false;      // Prevents multiple executions of death actions

    public Vector3 mazeMinBounds;  // Set in the inspector to the bottom-left corner of the maze
    public Vector3 mazeMaxBounds;  // Set in the inspector to the top-right corner of the maze

    private Transform playerTransform;

    private Animator clownAnimator;

    private Rigidbody rb;                   // Reference to Rigidbody for manual control of gravity

    public AudioClip chaseClip;
    private AudioSource audioSource;


    void Start()
    {

        // Create audio source
        // audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = chaseClip;
        audioSource.volume = 0.1f; 
        audioSource.loop = true; 


        //Clown
        // Reference the Animator component on the true_clown
        clownAnimator = transform.Find("true_clown").GetComponent<Animator>();
        
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>(); // Get Rigidbody component
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if(playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>();  // Assign PlayerMovement component
            if (playerMovement == null)
            {
                Debug.LogError("PlayerMovement component not found on the player object.");
            }
        }
        else
        {
            Debug.LogError("Player object not found in the scene!");
        }

        // Initialize patrol points
        if (patrolPoints.Length > 0)
        {
            currentPatrolPoint = 0;
            curState = MonsterState.Patrolling;
            agent.destination = patrolPoints[currentPatrolPoint].position;
        }
        else
        {
            Debug.LogError("No patrol points assigned!");
            curState = MonsterState.None;
        }
    }

    void Update()
    {
        //Debug.Log($"Current State: {curState}");

        switch (curState)
        {
            case MonsterState.Patrolling:
                CheckForPlayer();
                Patrol();
                break;

            case MonsterState.Chasing:
                CheckLostPlayer();
                ChasePlayer();
                break;
            
            case MonsterState.Climbing:
                Climb();
                break;

            case MonsterState.Attacking:
                AttackPlayer();
                break;

            case MonsterState.Dead:
                if (!hasDied)
                {
                    hasDied = true;
                    TriggerDeath();
                }
                break;

            case MonsterState.None:
                // Do nothing in this state
                break;
        }

        if (health <= 0 && curState != MonsterState.Dead)
        {
            curState = MonsterState.Dead;
        }
    }




    // Patrol State: Move between patrol points
    void Patrol()
    {
        // If chase audio playing, stop it
        if (audioSource.isPlaying)
        {
        audioSource.Stop();
        }

        AnimatePatrol();
        if (patrolPoints.Length == 0) return;

        // Set agent speed to patrol speed
        agent.speed = patrolSpeed;

        // If agent has reached the current patrol point, move to the next one
        if (!agent.pathPending && agent.remainingDistance < 2f)
        {
            currentPatrolPoint = (currentPatrolPoint + 1) % patrolPoints.Length;
            agent.destination = patrolPoints[currentPatrolPoint].position;
        }
    }

    // Check if the player is within detection range to start chasing
    void CheckForPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= chaseDistance && PlayerInMaze())
        {
            curState = MonsterState.Chasing;
            agent.speed = moveSpeed;
        }
    }


    // Chase State: Move towards the player using pathfinding
    void ChasePlayer()
    {
        // Play the audio when in chase
            if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    
        AnimateChasePlayer();
        if (playerTransform == null) return;

        Vector3 monsterPosition = transform.position;
        Vector3 playerPosition = playerTransform.position;

        Vector3 directionToPlayer = playerPosition - monsterPosition;

        float horizontalDistanceToPlayer = new Vector2(directionToPlayer.x, directionToPlayer.z).magnitude;
        float verticalDistanceToPlayer = Mathf.Abs(directionToPlayer.y);

        // Set the agent's destination to the player's horizontal position (same Y as monster)
        Vector3 targetPosition = new Vector3(playerPosition.x, monsterPosition.y, playerPosition.z);
        agent.destination = targetPosition;

        // Check if the player is directly above the monster (within thresholds)
        float verticalThreshold = 1.0f;     // Adjust as needed
        float horizontalThreshold = 2.0f;   // Adjust as needed

        if (verticalDistanceToPlayer > verticalThreshold && horizontalDistanceToPlayer < horizontalThreshold)
        {
            // Switch to Climbing state
            curState = MonsterState.Climbing;
            agent.isStopped = true; // Stop the NavMeshAgent
        }

        CheckAttackPlayer();

    }

    void CheckAttackPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRadius)
        {
            curState = MonsterState.Attacking;
        }
    }

    // Climbing State: Move upwards towards the player
    void Climb()
    {
        AnimateChasePlayer();
        //
        //
        //
        //
        //
        //
        // CHANGE HERE IF YOU WANT

        if (playerTransform == null) return;

        // Disable NavMeshAgent while climbing
        if (agent.enabled)
        {
            agent.enabled = false;
        }

        // Disable gravity and make the Rigidbody kinematic
        rb.useGravity = false;
        rb.isKinematic = true;

        Vector3 monsterPosition = transform.position;
        Vector3 playerPosition = playerTransform.position;

        float climbSpeed = 5f; // Adjust as needed

        // Calculate vertical movement
        float step = climbSpeed * Time.deltaTime;
        float newY = Mathf.MoveTowards(monsterPosition.y, playerPosition.y, step);

        transform.position = new Vector3(monsterPosition.x, newY, monsterPosition.z);

        // Check if monster has reached the player's Y position
        if (Mathf.Abs(playerPosition.y - transform.position.y) < 0.5f)
        {
            // Re-enable NavMeshAgent
            agent.enabled = true;
            agent.isStopped = false;

            // Re-enable gravity and set Rigidbody to non-kinematic
            rb.useGravity = true;
            rb.isKinematic = false;
            // Switch back to Chasing state
            curState = MonsterState.Chasing;
        }
    }

    // Check if the player has moved out of the maze or the monster's detection range
    void CheckLostPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > chaseDistance || !PlayerInMaze())
        {
            curState = MonsterState.Patrolling;
            agent.speed = patrolSpeed;
            agent.destination = patrolPoints[currentPatrolPoint].position;
        }
    }

    void AttackPlayer()
    {

         bool attackDone = false;
            if (attackDone == false)
        {
            AnimateAttack();
            
            // Call the Die method after a 1-second delay
            Invoke("PlayerDeath", 1f);
            
            // Set attackDone to true to prevent multiple attacks
            attackDone = true;
        }
        
        //Debug.Log("Attacking Player");
    }

        void PlayerDeath(){
        agent.isStopped = true;
         playerMovement.Die();
    }

    // Death State: Handle the monster's death
    void TriggerDeath()
    {
        Debug.Log("Monster has died.");
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject, 1.5f);  // Destroy after explosion
    }

    // Placeholder: Replace with your actual logic to determine if the player is inside the maze
    bool PlayerInMaze()
    {
        if (playerTransform == null) return false;

        Vector3 playerPos = playerTransform.position;

        // Check if the player's X and Z coordinates are within the maze boundaries (ignore Y)
        return (playerPos.x >= mazeMinBounds.x && playerPos.x <= mazeMaxBounds.x &&
                playerPos.z >= mazeMinBounds.z && playerPos.z <= mazeMaxBounds.z);
    }


    // Apply damage to the monster
    public void ApplyDamage(float damage)
    {
        health -= damage;
        Debug.Log($"Monster took {damage} damage, remaining health: {health}");
        if (health <= 0)
        {
            curState = MonsterState.Dead;
        }
    }

    //////Animation//////

    // Function to trigger animations based on boolean parameters
    void SetClownAnimation(string parameter, bool state)
    {
        if (clownAnimator != null)
        {
            clownAnimator.SetBool(parameter, state);
        }
    }


    void FacePlayer()
    {
    // Calculate the direction to the player
    Vector3 directionToPlayer = playerTransform.position - transform.position;
    // directionToPlayer.y = 0; // Ignore vertical rotation (optional, depending on your game)

    if (directionToPlayer.magnitude > 0.1f) // Check if the player is far enough to face them
    {
        // Calculate the rotation needed to face the player
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

        // Smoothly rotate the monster towards the player
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Adjust the rotation speed with Time.deltaTime * speed factor
        }
    }

    void AnimatePatrol(){
    // Set walking to true when patrolling, disable other animations
    SetClownAnimation("isWalking", true);  // Set walking to true
    SetClownAnimation("isRunning", false); // Ensure running is false
    SetClownAnimation("isIdle", false);    // Ensure idle is false
    }

    void AnimateChasePlayer(){
    // Set walking or running based on the player's distance, speed, or whatever logic you want
    SetClownAnimation("isRunning", true);  // Set running to true when chasing
    SetClownAnimation("isWalking", false); // Ensure walking is false
    SetClownAnimation("isIdle", false);    // Ensure idle is false
    }

    void AnimateAttack(){
        clownAnimator.SetTrigger("Attack");
    }



    ////END ANIMATION METHODS////


    // Optional Gizmo for debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
        Gizmos.color = Color.green;
        // Calculate the size of the maze based on the min and max bounds
        Vector3 mazeSize = mazeMaxBounds - mazeMinBounds;

        // Draw a wireframe cube representing the maze bounds
        Gizmos.DrawWireCube(mazeMinBounds + mazeSize / 2, mazeSize);
    }
}
