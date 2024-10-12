using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster4 : MonoBehaviour
{
    public enum MonsterState
    {
        Following,
        Attacking,
        Freezing,
        Idle,
        Jumping
    }
    private PlayerMovement playerMovement; // Reference to the PlayerMovement script
    public MonsterState currentState;

    private NavMeshAgent agent;
    private Rigidbody rb; // Rigidbody for applying jump physics
    private Transform playerTransform;
    public float followDistance = 50f;
    public float attackRange = 3f;
    public float freezeDuration = 5f;
    public float jumpDistance = 20f;   // Horizontal jump distance
    public float jumpHeight = 15f;      // How high the monster jumps

    private bool isFrozen = false;
    private float freezeTimer = 0f;

    public GameObject keyObject; // Reference to the key object on the tail
    private bool keyCollected = false;

    // Timer for state update (every 0.1 seconds)
    private float stateUpdateTimer = 0.1f;
    private float stateUpdateInterval = 0.1f;

    // To control the jump cooldown
    private float jumpCooldown = 5f;  // Cooldown between jumps
    private float jumpCooldownTimer = 0f; // Timer to track jump cooldown

    // To control the jump
    private bool isJumping = false;
    private bool jumpInitiated = false;

    // Store original position for out-of-bounds check
    private Vector3 originalPosition;

    // To check if the flashlight is currently shining on the monster
    private bool isFlashlightShining = false;

    // Store original Rigidbody constraints
    private RigidbodyConstraints originalConstraints;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>(); // Assign Rigidbody component

        // Find the player object and its PlayerMovement script
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

        // Lock rotation on x and z axes to keep the monster upright
        originalConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.constraints = originalConstraints;

        // Store original position at the start
        originalPosition = transform.position;

        currentState = MonsterState.Following; // Initial state
    }

    void Update()
    {
        // Update state every 0.1 seconds
        stateUpdateTimer -= Time.deltaTime;
        if (stateUpdateTimer <= 0f)
        {
            UpdateState();
            stateUpdateTimer = stateUpdateInterval; // Reset timer
        }

        // Reduce jump cooldown timer over time
        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }

        if (isFrozen)
        {
            // Only decrease freeze timer if flashlight is not shining
            if (!isFlashlightShining)
            {
                freezeTimer -= Time.deltaTime;
                if (freezeTimer <= 0f)
                {
                    Unfreeze();
                }
            }
            else
            {
                // Reset freeze timer if flashlight is still shining
                freezeTimer = freezeDuration;
            }
        }

        // Reset flashlight shining flag each frame
        isFlashlightShining = false;

        // Reset jumping if the monster has landed
        if (isJumping && rb.velocity.y == 0 && jumpInitiated)
        {
            // Re-enable NavMeshAgent after jump
            isJumping = false;
            agent.enabled = true;
            agent.isStopped = false;
            currentState = MonsterState.Following; // Go back to following after jump
            jumpInitiated = false;
        }

        // Check if out of bounds (y < -10), if so, reset position
        if (transform.position.y < -10f)
        {
            ResetToOriginalPosition();
        }
    }

    // Main state update logic
    void UpdateState()
    {
        Debug.Log("Current State " + currentState);
        switch (currentState)
        {
            case MonsterState.Following:
                FollowState();
                break;
            case MonsterState.Attacking:
                AttackState();
                break;
            case MonsterState.Freezing:
                FreezeState();
                break;
            case MonsterState.Idle:
                IdleState();
                break;
            case MonsterState.Jumping:
                JumpState();
                break;
        }
    }

    // Following state where the monster follows the player
    void FollowState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= followDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);

            if (distanceToPlayer <= attackRange && !isFrozen)
            {
                currentState = MonsterState.Attacking;
                Debug.Log("Monster4 is close enough to attack!");
            }
        }
        else
        {
            agent.isStopped = true;
            // If the player is outside of follow distance, attempt to jump if cooldown allows
            if (jumpCooldownTimer <= 0f)
            {
                currentState = MonsterState.Jumping;
                Debug.Log("Player is outside of follow distance, Monster4 will jump.");
            }
        }
    }

    // Attacking state where the monster will try to attack the player
    void AttackState()
    {
        if (isFrozen)
        {
            // If frozen during attack, switch to freezing state
            currentState = MonsterState.Freezing;
            return;
        }

        Debug.Log("Monster4 is attacking the player!");
        // DO AN ATTACK ANIMATION
        if (playerMovement != null)
        {
            playerMovement.Die();
        }
        // After attacking, switch back to following or idle as needed
        currentState = MonsterState.Following;
    }

    // Freezing state where the monster is frozen when light shines on it
    void FreezeState()
    {
        if (isFrozen)
        {
            // Monster is frozen and doesn't move
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.ResetPath(); // Clear the agent's path to fully stop movement
                agent.enabled = false; // Disable the NavMeshAgent
            }

            rb.velocity = Vector3.zero; // Stop any movement caused by the Rigidbody
            rb.angularVelocity = Vector3.zero; // Stop any unwanted rotations
            rb.isKinematic = true; // Make Rigidbody kinematic to prevent physics movement

            // Key collection
            if (!keyCollected && Vector3.Distance(transform.position, playerTransform.position) < attackRange)
            {
                CollectKey();
            }
        }
    }

    // Idle state: Not used in this case, but can be expanded
    void IdleState()
    {
        agent.isStopped = true;
    }

    // Jumping state: Monster jumps in a random direction when the player leaves range
    void JumpState()
    {
        if (isFrozen)
        {
            // If frozen during jumping, switch to freezing state
            currentState = MonsterState.Freezing;
            return;
        }

        if (!isJumping)
        {
            agent.isStopped = true; // Stop the agent
            agent.enabled = false;  // Disable NavMeshAgent so it doesn't interfere with physics

            // Calculate random direction between -45 and 45 degrees relative to the player
            float randomAngle = Random.Range(-45f, 45f);
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * directionToPlayer;

            // Apply a horizontal jump (in a random direction) and a vertical jump (upward)
            rb.isKinematic = false; // Ensure Rigidbody is not kinematic during jump
            rb.AddForce(randomDirection * jumpDistance + Vector3.up * jumpHeight, ForceMode.VelocityChange);
            isJumping = true;
            jumpInitiated = true;

            Debug.Log($"Monster4 jumps in a random direction (angle: {randomAngle}) and upward.");

            // Reset the jump cooldown timer after jumping
            jumpCooldownTimer = jumpCooldown;
        }
    }

    // Function to freeze the monster
    public void Freeze()
    {
        if (!isFrozen)
        {
            Debug.Log("Monster4 is frozen by the light!");
            isFrozen = true;
            freezeTimer = freezeDuration;
            currentState = MonsterState.Freezing;
        }
        else
        {
            // If already frozen, reset the freeze timer
            freezeTimer = freezeDuration;
        }

        // Indicate that the flashlight is currently shining on the monster
        isFlashlightShining = true;
    }

    // Function to unfreeze the monster after freeze duration ends
    void Unfreeze()
    {
        Debug.Log("Monster4 is no longer frozen.");
        isFrozen = false;
        rb.isKinematic = false; // Allow physics movement again
        rb.constraints = originalConstraints; // Restore original constraints

        if (!agent.enabled)
        {
            agent.enabled = true; // Re-enable the NavMeshAgent
        }
        agent.isStopped = false;
        currentState = MonsterState.Following; // Return to following state after unfreezing
    }

    // Collect key logic when the monster is frozen
    void CollectKey()
    {
        keyCollected = true;
        keyObject.SetActive(false); // Disable or hide the key object when collected
        Debug.Log("Player collected the key from Monster4's tail.");
    }

    // Function to reset the monster's position to its original position
    void ResetToOriginalPosition()
    {
        Debug.Log("Monster4 went out of bounds, resetting to original position.");
        rb.velocity = Vector3.zero; // Reset any velocity
        transform.position = originalPosition; // Reset position
        if (!agent.enabled)
        {
            agent.enabled = true; // Re-enable the NavMeshAgent
        }
        agent.isStopped = false;
        currentState = MonsterState.Following; // Return to following state
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
