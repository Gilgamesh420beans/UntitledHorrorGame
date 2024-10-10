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

    public MonsterState currentState = MonsterState.Idle;

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
    private float jumpCooldown = 5f;  // Cooldown of 2 seconds between jumps
    private float jumpCooldownTimer = 0f; // Timer to track jump cooldown

    // To control the jump
    private bool isJumping = false;
    private bool jumpInitiated = false;

    // Store original position for out-of-bounds check
    private Vector3 originalPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>(); // Assign Rigidbody component
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Lock rotation on x and z axes to keep the monster upright
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

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
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0f)
            {
                Unfreeze();
            }
        }

        // Reset jumping if the monster has landed
        if (isJumping && rb.velocity.y == 0 && jumpInitiated)
        {
            // Re-enable NavMeshAgent after jump
            isJumping = false;
            agent.enabled = true;
            currentState = MonsterState.Following; // Go back to following after jump
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
            agent.destination = playerTransform.position;

            if (distanceToPlayer <= attackRange && !isFrozen)
            {
                currentState = MonsterState.Attacking;
                Debug.Log("Monster4 is close enough to attack!");
            }
        }
        else
        {
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
        Debug.Log("Monster4 is attacking the player!");

        // Add attack logic here (e.g., reduce player health or trigger game over)
        if (Vector3.Distance(transform.position, playerTransform.position) > attackRange)
        {
            currentState = MonsterState.Following;
        }
    }

    // Freezing state where the monster is frozen when light shines on it
    void FreezeState()
    {
        if (isFrozen)
        {
            // Monster is frozen and doesn't move
            agent.isStopped = true;
            agent.ResetPath(); // Clear the agent's path to fully stop movement
            rb.velocity = Vector3.zero; // Stop any movement caused by the Rigidbody
            rb.angularVelocity = Vector3.zero; // Stop any unwanted rotations

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
        if (!isJumping)
        {
            agent.isStopped = true; // Stop the agent
            agent.enabled = false;  // Disable NavMeshAgent so it doesn't interfere with physics

            // Calculate random direction between -45 and 45 degrees relative to the player
            float randomAngle = Random.Range(-45f, 45f);
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * directionToPlayer;

            // Apply a horizontal jump (in a random direction) and a vertical jump (upward)
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
    }

    // Function to unfreeze the monster after freeze duration ends
    void Unfreeze()
    {
        Debug.Log("Monster4 is no longer frozen.");
        isFrozen = false;
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
        agent.enabled = true; // Re-enable the NavMeshAgent
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
