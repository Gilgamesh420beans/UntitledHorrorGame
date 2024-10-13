using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;  // Include the NavMesh namespace
using UnityStandardAssets.Characters.FirstPerson;

public class Monster2 : MonoBehaviour
{
    public enum MonsterState
    {
        Patrolling,
        Chasing,
        Freezing,
        Teleporting, // Makes a noise
        Attacking
    }

    private PlayerMovement playerMovement; // Reference to the PlayerMovement script
    public MonsterState curState = MonsterState.Patrolling;

    public Transform[] patrolPoints;  // Waypoints for patrolling
    private int currentPatrolIndex = 0;

    private Vector3 lastHeardPosition = Vector3.zero;  // The position where the monster last heard the player
    public float chaseSpeed = 7f;
    public float patrolSpeed = 6f;
    public float hearingRange = 60f;   // Range within which the monster can hear footsteps

    public float attackRange = 3f;
    private Transform playerTransform;

    // Variables for wall-following behavior
    private bool isWallFollowing = false;
    private Vector3 wallFollowDirection = Vector3.zero;
    private Vector3 lastWallNormal = Vector3.zero;
    private int stuckCounter = 0; // Counter to keep track of attempts

    private Vector3 currentMovementDirection = Vector3.zero;
    private float requiredClearDistance = 15.0f; // Distance the path must be clear to exit wall-following

    // Variables for detecting if the monster is trapped
    private float trapCheckInterval = 8.0f; // Interval in seconds to check if the monster is trapped
    private float trapTimer = 0.0f; // Timer to keep track of the interval
    private Vector3 positionAtLastCheck; // Position of the monster at the last check
    private float trapRadius = 10.0f; // Radius within which the monster is considered trapped

    private float postTeleportTimer = 0f; // Timer to stop movement after teleport
    private float postTeleportDuration = 2f; // Duration to stop movement after teleport

    private NavMeshAgent agent;  // Reference to the NavMeshAgent component

    private Animator clownAnimator;

    public AudioClip chaseClip;
    private AudioSource audioSource;

    void Start()
    {
        // Create audio source
          if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = chaseClip;
        audioSource.volume = 1.0f; 
        audioSource.loop = true; 



        // Reference the Animator component on the true_clown
        clownAnimator = transform.Find("true_clown_variant").GetComponent<Animator>();

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

        // Subscribe to the player's footstep event
        PlayerMovement.OnFootstep += OnPlayerFootstep;

        // Check if patrol points are assigned
        if (patrolPoints.Length == 0)
        {
            // Debug.LogError("No patrol points assigned!");
        }

        // Initialize positionAtLastCheck
        positionAtLastCheck = transform.position;

        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on the monster.");
        }
        else
        {
            agent.speed = patrolSpeed;
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.updatePosition = true;
            agent.enabled = true;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the player's footstep event
        PlayerMovement.OnFootstep -= OnPlayerFootstep;
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        if (postTeleportTimer > 0)
        {
            postTeleportTimer -= Time.deltaTime;
            return; // Skip further updates until the post-teleport timer runs out
        }
        // Update the trap timer
        trapTimer += Time.deltaTime;

        // Check if it's time to evaluate if the monster is trapped
        if (trapTimer >= trapCheckInterval)
        {
            // Check if the monster is trapped
            if (IsMonsterTrapped())
            {
                // Debug.Log("Monster is trapped. Teleporting to a random waypoint.");
                TeleportToRandomWaypoint();
                // Reset the timer and position
                trapTimer = 0.0f;
                positionAtLastCheck = transform.position;

                // Start post-teleport idle period
                postTeleportTimer = postTeleportDuration;
                return; // Skip other updates this frame
            }
            else
            {
                // Monster has moved sufficiently, reset timer and update position
                trapTimer = 0.0f;
                positionAtLastCheck = transform.position;
            }
        }

        // Switch between states
        switch (curState)
        {
            case MonsterState.Patrolling:
                if (agent != null && !agent.enabled)
                {
                    agent.enabled = true;
                    agent.isStopped = false;
                }
                Patrol();
                break;
            case MonsterState.Chasing:
                if (agent != null && agent.enabled)
                {
                    agent.isStopped = true;
                    agent.enabled = false;
                }
                Chase();
                break;
            case MonsterState.Freezing:
                // Do nothing
                break;
            case MonsterState.Attacking:
                Attack();
                break;
        }
    }

    void Attack()
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
        // DO AN ATTACK ANIMATION
      
    }

     void PlayerDeath(){
       
         if (playerMovement != null)
        {
            playerMovement.Die();
        }
    }

    bool IsMonsterTrapped()
    {
        // Calculate the distance moved in the last interval
        float distanceMoved = Vector3.Distance(positionAtLastCheck, transform.position);

        // If the monster hasn't moved outside the trap radius, it's considered trapped
        if (distanceMoved < trapRadius)
        {
            // Debug.Log($"Monster has moved {distanceMoved} units in {trapCheckInterval} seconds, which is less than the trap radius of {trapRadius} units.");
            return true;
        }
        else
        {
            // Debug.Log($"Monster has moved {distanceMoved} units in {trapCheckInterval} seconds, which is sufficient.");
            return false;
        }
    }

    void TeleportToRandomWaypoint()
    {
        if (patrolPoints.Length == 0) return; // Ensure there are patrol points

        // Choose a random waypoint to teleport to
        int randomIndex = Random.Range(0, patrolPoints.Length);
        Transform randomWaypoint = patrolPoints[randomIndex];

        // Teleport the monster to the selected waypoint
        transform.position = randomWaypoint.position;
        Debug.Log($"Teleported to waypoint {randomIndex}: {randomWaypoint.position}");

        // Reset the NavMeshAgent
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.ResetPath();
        }
    }

    void Patrol()
    {
        // If chase audio playing, stop it
        if (audioSource.isPlaying)
        {
        audioSource.Stop();
        }

        AnimatePatrol();
        if (patrolPoints.Length == 0 || agent == null || !agent.enabled) return;

        agent.speed = patrolSpeed;

        // If the agent has no path or has reached the current patrol point, set the next patrol point
        if (!agent.hasPath || agent.remainingDistance < 1f)
        {
            agent.destination = patrolPoints[currentPatrolIndex].position;
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    void Chase()
    {
        // Play the audio when in chase
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        AnimateChasePlayer();
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= attackRange)
        {
            // Switch to Attacking state and perform the attack
            curState = MonsterState.Attacking;
            Attack();
  
            return;  // Exit the method after attacking
        }

        if (lastHeardPosition != Vector3.zero)
        {
            MoveTowardsTarget(lastHeardPosition, chaseSpeed);

            // If reached the last heard position, go back to patrolling
            if (Vector3.Distance(transform.position, lastHeardPosition) < 1.0f)
            {
                lastHeardPosition = Vector3.zero;
                curState = MonsterState.Patrolling;

                // Re-enable the NavMeshAgent and resume patrolling
                if (agent != null)
                {
                    agent.enabled = true;
                    agent.isStopped = false;
                }
            }
        }
        else
        {
            // No last heard position, switch to patrolling
            curState = MonsterState.Patrolling;

            // Re-enable the NavMeshAgent and resume patrolling
            if (agent != null)
            {
                agent.enabled = true;
                agent.isStopped = false;
            }
        }
    }

    void MoveTowardsTarget(Vector3 targetPosition, float speed)
    {
        if (curState == MonsterState.Freezing)
            return;

        // Calculate the direction towards the target
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        directionToTarget.y = 0; // Keep movement in the horizontal plane

        float obstacleDetectionDistance = Mathf.Max(1.0f, speed * Time.deltaTime + 0.5f);

        // If wall-following, continue in the wall-following direction
        if (isWallFollowing)
        {
            WallFollow(speed);

            // Check if the path towards the target is clear for the required distance
            if (!IsObstacleInDirection(transform.position, directionToTarget, requiredClearDistance, out RaycastHit _))
            {
                // Path is clear, stop wall-following
                isWallFollowing = false;
                stuckCounter = 0;
            }
            return;
        }

        // Check for obstacle directly ahead
        if (IsObstacleInDirection(transform.position, directionToTarget, obstacleDetectionDistance, out RaycastHit hitInfo))
        {
            // Obstacle detected, start wall-following
            isWallFollowing = true;
            stuckCounter = 0;

            // Get the normal of the wall to determine the wall-following direction
            lastWallNormal = hitInfo.normal;
            wallFollowDirection = Vector3.Cross(lastWallNormal, Vector3.up).normalized;
            // Ensure wall is on the right side
            if (Vector3.Dot(wallFollowDirection, directionToTarget) < 0)
            {
                wallFollowDirection = -wallFollowDirection;
            }
            MoveInDirection(wallFollowDirection, speed);
        }
        else
        {
            // No obstacle ahead, move towards the target
            MoveInDirection(directionToTarget, speed);
        }
    }

    void WallFollow(float speed)
    {
        // Directions relative to current wallFollowDirection
        Vector3[] directions = new Vector3[]
        {
            wallFollowDirection, // Forward
            Quaternion.Euler(0, 45, 0) * wallFollowDirection,  // Forward-Right
            Quaternion.Euler(0, -45, 0) * wallFollowDirection, // Forward-Left
            Quaternion.Euler(0, 90, 0) * wallFollowDirection,  // Right
            Quaternion.Euler(0, -90, 0) * wallFollowDirection, // Left
            Quaternion.Euler(0, 135, 0) * wallFollowDirection, // Backward-Right
            Quaternion.Euler(0, -135, 0) * wallFollowDirection,// Backward-Left
            -wallFollowDirection                               // Backward
        };

        foreach (var dir in directions)
        {
            Vector3 normalizedDir = dir.normalized;
            if (!IsObstacleInDirection(transform.position, normalizedDir, 1.0f, out RaycastHit _))
            {
                wallFollowDirection = normalizedDir;
                MoveInDirection(wallFollowDirection, speed);
                stuckCounter = 0; // Reset counter
                return;
            }
        }

        // All directions blocked, increment stuckCounter
        stuckCounter++;

        if (stuckCounter >= 4)
        {
            // Choose a random direction to get unstuck
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            wallFollowDirection = randomDirection;
            stuckCounter = 0;
        }
    }

    void MoveInDirection(Vector3 direction, float speed)
    {
        // Normalize direction
        direction = direction.normalized;

        float obstacleDetectionDistance = Mathf.Max(1.0f, speed * Time.deltaTime + 0.5f);

        // Move in the specified direction if no obstacle
        if (!IsObstacleInDirection(transform.position, direction, obstacleDetectionDistance, out RaycastHit hitInfo))
        {
            transform.position += direction * speed * Time.deltaTime;

            currentMovementDirection = direction;

            // Update rotation to face movement direction
            if (direction != Vector3.zero)
            {
                // Smooth rotation
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        else
        {
            // Obstacle detected during movement, adjust wall-following
            isWallFollowing = true;
            stuckCounter++;
            lastWallNormal = hitInfo.normal;
            wallFollowDirection = Vector3.Cross(lastWallNormal, Vector3.up).normalized;

            // Ensure wall is on the right side
            if (Vector3.Dot(wallFollowDirection, direction) < 0)
            {
                wallFollowDirection = -wallFollowDirection;
            }

            currentMovementDirection = Vector3.zero;
        }
    }

    bool IsObstacleInDirection(Vector3 origin, Vector3 direction, float distance, out RaycastHit hitInfo)
    {
        // Offset the cast origin slightly in the direction to avoid starting inside an obstacle
        Vector3 castOrigin = origin + Vector3.up * 0.5f + direction.normalized * 0.1f;
        float sphereRadius = 0.5f; // Adjust based on monster size

        // Perform a SphereCast without a layer mask
        bool hit = Physics.SphereCast(castOrigin, sphereRadius, direction, out hitInfo, distance);

        if (hit && hitInfo.collider != null && hitInfo.collider.CompareTag("Obstacle") && hitInfo.collider.gameObject != gameObject)
        {
            Debug.DrawRay(castOrigin, direction * distance, Color.red);
            return true;
        }

        return false;
    }

    void OnPlayerFootstep(Vector3 playerPosition)
    {
        // Check if the monster can hear the player
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
        if (distanceToPlayer <= hearingRange)
        {
            lastHeardPosition = playerPosition;
            curState = MonsterState.Chasing;

            // Disable the NavMeshAgent when starting to chase
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
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


    // void FacePlayer()
    // {
    // // Calculate the direction to the player
    // Vector3 directionToPlayer = playerTransform.position - transform.position;
    // // directionToPlayer.y = 0; // Ignore vertical rotation (optional, depending on your game)

    // if (directionToPlayer.magnitude > 0.1f) // Check if the player is far enough to face them
    // {
    //     // Calculate the rotation needed to face the player
    //     Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

    //     // Smoothly rotate the monster towards the player
    //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Adjust the rotation speed with Time.deltaTime * speed factor
    //     }
    // }

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




    void DrawArrow(Vector3 position, Vector3 direction)
    {
        float arrowHeadLength = 1.0f; // Length of the arrow head
        float arrowHeadAngle = 20.0f; // Angle of the arrow head

        // Draw the shaft of the arrow
        Gizmos.DrawLine(position, position + direction * 2.0f); // Adjust length as needed

        // Calculate the arrow head points
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;

        // Draw the arrow head
        Gizmos.DrawLine(position + direction * 2.0f, position + direction * 2.0f + right * arrowHeadLength);
        Gizmos.DrawLine(position + direction * 2.0f, position + direction * 2.0f + left * arrowHeadLength);
    }

    void OnDrawGizmos()
    {
        // Draw the hearing range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        // Draw patrol points
        if (patrolPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in patrolPoints)
            {
                Gizmos.DrawSphere(point.position, 0.5f);
            }
        }

        // Draw the movement direction arrow
        if (currentMovementDirection != Vector3.zero)
        {
            // Determine the color based on monster's state
            if (isWallFollowing)
            {
                Gizmos.color = Color.magenta; // Wall-following
            }
            else
            {
                Gizmos.color = Color.green; // Clear path to target
            }

            // Draw the arrow
            DrawArrow(transform.position, currentMovementDirection.normalized);
        }
    }
}
