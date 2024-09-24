using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public enum MonsterState
    {
        Patrolling,
        Chasing,
        Dead,
        None
    }

    // Monster Properties
    public MonsterState curState = MonsterState.None;  // Default state
    public Transform player;
    public Transform[] patrolPoints;  // Waypoints for patrolling
    public GameObject explosionPrefab;
    public float moveSpeed = 2f;       // Speed while chasing
    public float patrolSpeed = 1f;     // Speed while patrolling
    public float health = 100f;        // Health of the monster
    public float detectionRadius = 50f; // Detection range for the player
    public float chaseDistance = 45.0f;  // Distance to start chasing the player

    private int currentPatrolPoint = 0;
    private bool hasDied = false;      // Prevents multiple executions of death actions
    private Pathfinder pathfinder;     // Reference to the Pathfinder script
    private int currentPathIndex = 0;  // Index for path nodes in pathfinding

    public float pathRecalculationInterval = 1.0f; // Time between path recalculations
    private float timeSinceLastPathUpdate = 0.0f; // Track time since last path update

    public Vector3 mazeMinBounds;  // Set in the inspector to the bottom-left corner of the maze
    public Vector3 mazeMaxBounds;  // Set in the inspector to the top-right corner of the maze

    void Start()
    {
        // Get the Pathfinder component attached to the monster
        pathfinder = GetComponent<Pathfinder>();

        Debug.Log("Monster has started. Setting initial state to Patrolling.");

        curState = MonsterState.Patrolling;

        if (patrolPoints.Length > 0)
        {
            currentPatrolPoint = 0;
            Debug.Log("Starting patrol at point: " + patrolPoints[currentPatrolPoint].name);
        }
        else
        {
            Debug.LogError("No patrol points assigned!");
        }
    }

    void Update()
    {
        //Debug.Log($"Current State: {curState}");

        switch (curState)
        {
            case MonsterState.Patrolling:
                Patrol();
                CheckForPlayer();
                break;

            case MonsterState.Chasing:
                ChasePlayer();
                CheckLostPlayer();
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
        Debug.Log("Patrolling...");
        if (Vector3.Distance(transform.position, patrolPoints[currentPatrolPoint].position) < 1f)
        {
            currentPatrolPoint = (currentPatrolPoint + 1) % patrolPoints.Length;
        }

        // Move towards the next patrol point
        transform.position = Vector3.MoveTowards(transform.position, patrolPoints[currentPatrolPoint].position, patrolSpeed * Time.deltaTime);
    }


    // Check if the player is within detection range to start chasing
    void CheckForPlayer()
    {
        Debug.Log("Checking for player...");
        if (Vector3.Distance(transform.position, player.position) < detectionRadius && PlayerInMaze())
        {
            Debug.Log("Player detected! Chasing...");
            curState = MonsterState.Chasing;
            pathfinder.FindPath();  // Recalculate the path to the player
            currentPathIndex = 0;  // Reset path index
        }
    }

    // Chase State: Move towards the player using pathfinding
    void ChasePlayer()
    {
        // Update the timer for path recalculation
        timeSinceLastPathUpdate += Time.deltaTime;

        // Recalculate the path every pathRecalculationInterval seconds
        if (timeSinceLastPathUpdate >= pathRecalculationInterval)
        {
            pathfinder.FindPath(); // Recalculate path to player
            timeSinceLastPathUpdate = 0.0f; // Reset the timer
            currentPathIndex = 0;  // Reset the path index
        }

        if (pathfinder.pathArray == null || pathfinder.pathArray.Count == 0)
        {
            Debug.LogError("No path found!");
            return;
        }

        Node currentNode = (Node)pathfinder.pathArray[currentPathIndex];
        Vector3 targetPosition = new Vector3(currentNode.position.x, transform.position.y, currentNode.position.z);

        // Move towards the current node
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if the monster has reached the current node
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex++;
            if (currentPathIndex >= pathfinder.pathArray.Count)
            {
                Debug.Log("Reached the player!");
                curState = MonsterState.Patrolling;  // Reset to patrolling after reaching the player
            }
        }
    }

        // Check if the player has moved out of the maze or the monster's detection range
        void CheckLostPlayer()
    {
        Debug.Log("Checking if player is lost...");
        if (Vector3.Distance(transform.position, player.position) >= detectionRadius || !PlayerInMaze())
        {
            Debug.Log("Lost the player. Returning to patrol...");
            curState = MonsterState.Patrolling;
        }
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
        Vector3 playerPos = player.position;

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
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        // Calculate the size of the maze based on the min and max bounds
        Vector3 mazeSize = mazeMaxBounds - mazeMinBounds;

        // Draw a wireframe cube representing the maze bounds
        Gizmos.DrawWireCube(mazeMinBounds + mazeSize / 2, mazeSize);
    }
}
