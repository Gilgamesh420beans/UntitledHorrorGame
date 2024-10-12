using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster1 : MonoBehaviour
{
    public enum MonsterState
    {
        Patrolling,
        Chasing,
        Attacking,
        Dead,
        None
    }
    private PlayerMovement playerMovement; // Reference to the PlayerMovement script
    // Monster Properties
    public MonsterState curState = MonsterState.None;  // Default state
    public Transform player;
    public Transform[] patrolPoints;  // Waypoints for patrolling
    public GameObject explosionPrefab;
    public float moveSpeed = 2f;            // Speed while chasing
    public float patrolSpeed = 1f;          // Speed while patrolling
    public float health = 100f;             // Health of the monster
    public float attackRadius = 2f;         // attack radius 
    public float chaseDistance = 45.0f;     // Distance to start chasing the player

    private int currentPatrolPoint = 0;
    private bool hasDied = false;      // Prevents multiple executions of death actions
    private Pathfinder pathfinder;     // Reference to the Pathfinder script
    private int currentPathIndex = 0;  // Index for path nodes in pathfinding

    public float pathRecalculationInterval = 1.0f; // Time between path recalculations
    private float timeSinceLastPathUpdate = 0.0f; // Track time since last path update

    public Vector3 mazeMinBounds;  // Set in the inspector to the bottom-left corner of the maze
    public Vector3 mazeMaxBounds;  // Set in the inspector to the top-right corner of the maze

    private Transform playerTransform;

    private Animator clownAnimator;

    
    void Start()
    {

        //Clown
        // Reference the Animator component on the true_clown
        clownAnimator = transform.Find("true_clown").GetComponent<Animator>();

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
                CheckForPlayer();
                Patrol();
                break;

            case MonsterState.Chasing:
                CheckLostPlayer();
                ChasePlayer();
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

        AnimatePatrol();
        FollowPath(patrolSpeed);
        //Debug.Log("Patrolling...");

        // Resume pathfinding after losing the player
        pathfinder.ResumePathfinding();

        // Check if we need to find a new path (i.e., no current path or reached the goal)
        if (pathfinder.pathArray == null || currentPathIndex >= pathfinder.pathArray.Count)
        {
            // If the goal (patrol point) is reached, switch to the next patrol point
            currentPatrolPoint = (currentPatrolPoint + 1) % patrolPoints.Length;
            //Debug.Log("Switching to the next patrol point: " + patrolPoints[currentPatrolPoint].name);

            // Calculate path to the next patrol point
            pathfinder.FindPath("astar", patrolPoints[currentPatrolPoint].gameObject);

            // Check if a valid path was found
            if (pathfinder.pathArray == null || pathfinder.pathArray.Count == 0)
            {
                Debug.LogError("No path found for patrolling to point: " + patrolPoints[currentPatrolPoint].name);
                return;  // Skip this patrol point or handle it as needed
            }
            currentPathIndex = 0;  // Reset the path index for the new path
        }

        // Follow the path to the current patrol point
        FollowPath(patrolSpeed);
    }

    // Check if the player is within detection range to start chasing
    void CheckForPlayer()
    {
        //Debug.Log("Checking for player...");

        // Check if the player is within the maze and reachable
        if (PlayerInMaze() && IsPlayerReachable())
        {
            Debug.Log("Player detected! Chasing...");
            curState = MonsterState.Chasing;
            pathfinder.FindPath("astar", GameObject.FindGameObjectWithTag("Player"));  // Recalculate the path to the player
            currentPathIndex = 0;  // Reset path index
        }
        else
        {
            // Player is unreachable or outside the maze, revert to patrol mode
            //Debug.LogWarning("Player unreachable, reverting to patrol mode.");
            curState = MonsterState.Patrolling;
            pathfinder.ResumePathfinding();  // Resume pathfinding for patrol
        }
    }


    bool IsPlayerReachable()
    {
        // Get the player's position
        Vector3 playerPos = player.position;

        // Get the grid index of the player position
        int playerGridIndex = GridManager.instance.GetGridIndex(playerPos);

        // Ensure the player is within bounds and the grid is not an obstacle
        if (playerGridIndex == -1)
        {
            //Debug.LogError("Player is out of bounds.");
            return false;
        }

        Node playerNode = GridManager.instance.nodes[playerGridIndex % GridManager.instance.numOfColumns, playerGridIndex / GridManager.instance.numOfColumns];

        // Check if the player node is an obstacle
        if (playerNode == null || playerNode.bObstacle)
        {
            //Debug.LogError("Player is inside an obstacle or unreachable.");
            return false;
        }

        return true;  // Player is reachable
    }

    // Follow the calculated path
    void FollowPath(float speed)
    {
        // Check if pathfinding is paused and skip following the path if it is
        if (pathfinder.pathfindingState == Pathfinder.PathfindingState.Paused)
        {
            //Debug.Log("Pathfinding is paused, skipping movement.");
            return;  // Skip movement while pathfinding is paused
        }

        if (pathfinder.pathArray == null || pathfinder.pathArray.Count == 0)
        {
            //Debug.LogError("No path found for patrolling!");
            return;
        }

        Node currentNode = (Node)pathfinder.pathArray[currentPathIndex];
        Vector3 targetPosition = new Vector3(currentNode.position.x, transform.position.y, currentNode.position.z);

        // Move towards the current node in the path
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Check if the monster has reached the current node
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            currentPathIndex++;

            // goal reached)
            if (currentPathIndex >= pathfinder.pathArray.Count)
            {
                //Debug.Log("Reached goal.");
            }
        }
    }

    // Chase State: Move towards the player using pathfinding
    void ChasePlayer()
    {
        // Ensure the monster faces the player
        FacePlayer();
        AnimateChasePlayer();

        timeSinceLastPathUpdate += Time.deltaTime;

        if (timeSinceLastPathUpdate >= pathRecalculationInterval)
        {
            pathfinder.FindPath("astar", GameObject.FindGameObjectWithTag("Player")); // Recalculate path to player
            timeSinceLastPathUpdate = 0.0f; // Reset the timer
            currentPathIndex = 0;  // Reset the path index
        }

        FollowPath(moveSpeed);

        ///////////////////////////////    
        // ADRIAN LOOK HERE!!!!!!
        // Attack player if in range
        if (Vector3.Distance(transform.position, player.position) < attackRadius)
        {
            AttackPlayer();
        }
        ///////////////////////////////

    }

   

    // Check if the player has moved out of the maze or the monster's detection range
    void CheckLostPlayer()
    {
        //Debug.Log("Checking if player is lost...");
        if (!PlayerInMaze() || !IsPlayerReachable())
        {
            Debug.Log("Lost the player. Returning to patrol...");
            curState = MonsterState.Patrolling;
            // When the player is lost, switch target to the next patrol point
            currentPatrolPoint = (currentPatrolPoint + 1) % patrolPoints.Length;
            pathfinder.FindPath("astar", patrolPoints[currentPatrolPoint].gameObject);
            pathfinder.ResumePathfinding();
        }
    }

    void AttackPlayer()
    {
        // DO AN ATTACK ANIMATION
        AnimateAttack();
        playerMovement.Die();
        //Debug.Log("Attacking Player");
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
    Vector3 directionToPlayer = player.position - transform.position;
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

        Gizmos.color = Color.green;
        // Calculate the size of the maze based on the min and max bounds
        Vector3 mazeSize = mazeMaxBounds - mazeMinBounds;

        // Draw a wireframe cube representing the maze bounds
        Gizmos.DrawWireCube(mazeMinBounds + mazeSize / 2, mazeSize);
    }
}
