using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;  // Include the NavMesh namespace

public class Monster1 : MonoBehaviour
{
    public enum MonsterState
    {
        Patrolling,
        Chasing,
        Attacking,
        Climbing,
        Dead,
        None
    }

    // Monster Properties
    public MonsterState curState = MonsterState.None;  // Default state

    private NavMeshAgent agent;           // Reference to the NavMeshAgent component
    private Transform playerTransform;    // Reference to the player's transform
    private PlayerMovement playerMovement; // Reference to the PlayerMovement script
    private Rigidbody rb;                   // Reference to Rigidbody for manual control of gravity

    public Transform[] patrolPoints;      // Waypoints for patrolling
    public GameObject explosionPrefab;
    public float moveSpeed = 5f;          // Speed while chasing
    public float patrolSpeed = 8f;        // Speed while patrolling
    public float health = 100f;           // Health of the monster
    public float attackRadius = 3f;       // Attack radius
    public float chaseDistance = 60.0f;   // Distance to start chasing the player

    private int currentPatrolPoint = 0;
    private bool hasDied = false;         // Prevents multiple executions of death actions

    public Vector3 mazeMinBounds;         // Set in the inspector to the bottom-left corner of the maze
    public Vector3 mazeMaxBounds;         // Set in the inspector to the top-right corner of the maze

    void Start()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>(); // Get Rigidbody component
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on the monster.");
        }

        // Set initial speeds
        agent.speed = patrolSpeed;

        // Find the player object and get references
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
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
        if (health <= 0 && curState != MonsterState.Dead)
        {
            curState = MonsterState.Dead;
        }

        switch (curState)
        {
            case MonsterState.Patrolling:
                Patrol();
                CheckForPlayer();
                break;

            case MonsterState.Chasing:
                ChasePlayer();
                CheckAttackPlayer();
                CheckLostPlayer();
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
    }

    // Patrol State: Move between patrol points
    void Patrol()
    {
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

    // Chase State: Move towards the player's horizontal position
    void ChasePlayer()
    {
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
    }

    // Climbing State: Move upwards towards the player
    void Climb()
    {
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

    // Check if the monster should attack the player
    void CheckAttackPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRadius)
        {
            curState = MonsterState.Attacking;
        }
    }

    // Attack State: Attack the player
    void AttackPlayer()
    {
        if (playerMovement != null)
        {
            // Implement attack logic here (e.g., reduce player health or trigger game over)
            playerMovement.Die();
            Debug.Log("Attacking Player");
        }

        // You can decide whether to destroy the monster or switch to another state after attacking
        // For example, return to patrolling or continue chasing
        // curState = MonsterState.Patrolling;
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

    // Death State: Handle the monster's death
    void TriggerDeath()
    {
        Debug.Log("Monster has died.");
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject, 1.5f);  // Destroy after explosion
    }

    // Check if the player is inside the maze
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
