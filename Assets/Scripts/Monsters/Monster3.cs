using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static MotherMonster;

public class Monster3 : MonoBehaviour
{
    public enum MonsterState
    {
        Spawning,
        Idle,
        Angry,
        Attacking,
        Stalking,
        Watching,
    }
    private PlayerMovement playerMovement; // Reference to the PlayerMovement script
    public MonsterState currentState;

    private NavMeshAgent agent;
    private Transform playerTransform;

    public float stalkRange = 20f;
    public float attackRange = 3f;
    private bool isAngry = false;

    public float idleDirection;
    public float idleSpeed;
    public float idleTimer;
    public float stalkingDistance;
    public float stalkingSpeed;

    public float noiseTimer;

    private bool playerInForestArea = false;  // Track if the player is in the ForestArea
    private bool monsterInForestArea = true; // Track if the monster is in the ForestArea

    private bool isInitialized = false; // Flag to check if Monster3 is initialized

    private ForestAreaTrigger forestAreaTrigger;
    private Animator crawlerAnimator;


    void Start()
    {
        //IMPORTANT//
        // Reference the Animator component on the true_clown(May need to be tag if issues arise)
        crawlerAnimator = transform.Find("crawler").GetComponent<Animator>();


        agent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = MonsterState.Idle; // Initial state
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
        // Initialize random values for idle behavior
        idleSpeed = Random.Range(2f, 5f);  // Example random speed range
        stalkingDistance = Random.Range(10f, 20f); // Example stalking distance
        stalkingSpeed = Random.Range(3f, 9f);  // Example stalking speed


        GameObject forestAreaObject = GameObject.FindGameObjectWithTag("ForestArea");
        if (forestAreaObject != null)
        {
            forestAreaTrigger = forestAreaObject.GetComponent<ForestAreaTrigger>();
            if (forestAreaTrigger != null)
            {
                forestAreaTrigger.OnPlayerEnteredForestArea += PlayerEnteredForestArea;
                forestAreaTrigger.OnPlayerExitedForestArea += PlayerExitedForestArea;
            }
            else
            {
                Debug.LogError("ForestAreaTrigger component not found on ForestArea object.");
            }
        }
        else
        {
            Debug.LogError("ForestArea object not found in the scene.");
        }
        isInitialized = true;
    }

    void Update()
    {
        switch (currentState)
        {
            case MonsterState.Spawning:
                Debug.Log("Monster is in Spawning state.");
                SpawningState();
                break;
            case MonsterState.Idle:
                Debug.Log("Monster is in Idle state.");
                IdleState();
                break;
            case MonsterState.Angry:
                Debug.Log("Monster is in Angry state.");
                AngryState();
                break;
            case MonsterState.Attacking:
                Debug.Log("Monster is in Attacking state.");
                AttackState();
                break;
            case MonsterState.Stalking:
                Debug.Log("Monster is in Stalking state.");
                StalkState();
                break;
            case MonsterState.Watching:
                Debug.Log("Monster is in Watching state.");
                WatchingState();
                break;
        }
    }

    // Example state methods
    void SpawningState()
    {
        AnimateIdle();
        // Starts at 1/4 size, increases size over 15 seconds to normal size, then enters idle state
        transform.localScale = Vector3.Lerp(Vector3.one * 0.25f, Vector3.one, Time.time / 15f);

        if (transform.localScale == Vector3.one)
        {
            currentState = MonsterState.Idle;
        }
    }

    void IdleState()
    {
        if (monsterInForestArea)
        {
            // Check if the agent has reached its destination or if it's time to change direction
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // Generate a random point inside the ForestArea
                Vector3 randomPoint = GetRandomPointInForestArea();
                AnimateIdle();
                // Set the new destination for the NavMeshAgent to the random point
                agent.speed = idleSpeed;
                agent.destination = randomPoint;

                Debug.Log($"IdleState: Moving to a random point {randomPoint} inside the ForestArea.");
            }

            // Check if the player is in the forest area
            if (playerInForestArea)
            {
                Debug.Log("Player entered ForestArea, switching to Stalking state.");
                currentState = MonsterState.Stalking;
            }
        }
        else
        {
            Debug.Log("Monster is not in the ForestArea. Cannot perform idle movement.");
        }
    }

    public void TriggerAngryMode()
    {
        StartCoroutine(TriggerAngryModeCoroutine());
    }

    private IEnumerator TriggerAngryModeCoroutine()
    {
        Debug.Log("Monster3 will be Angry in 5 seconds...");

        // Wait for 5 seconds
        yield return new WaitForSeconds(5f);

        // After the wait, set isAngry and switch to Angry state
        isAngry = true;
        currentState = MonsterState.Angry;

        Debug.Log("Monster3 is now Angry!");
    }


    void AngryState()
    {
        AnimateChasePlayer();
        // Angry logic, chases player when inside ForestArea
        if (playerInForestArea)
        {
            agent.destination = playerTransform.position;

            if (Vector3.Distance(transform.position, playerTransform.position) < attackRange)
            {
                currentState = MonsterState.Attacking;
            }
        }
        else
        {
            // Player left the area, monster stops and watches
            agent.isStopped = true;
            currentState = MonsterState.Watching;
        }
    }

    void AttackState()
    {

        // Attack logic, jumps toward the player
        if (Vector3.Distance(transform.position, playerTransform.position) < attackRange)
        {
            AnimateAttack();
            // DO AN ATTACK ANIMATION
            playerMovement.Die();
            // Perform attack and kill player
        }
    }

    void StalkState()
    {
        if (playerInForestArea)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            // Buffer range for stalking (± 2 units)
            float stalkingBufferMin = stalkingDistance - 2f;
            float stalkingBufferMax = stalkingDistance + 2f;
            // Debugging: Print the current distance between monster and player
            //Debug.Log($"Stalking State: Distance to Player: {distance}");

            if (distance > stalkingBufferMax)
            {
                AnimateSlowCrawl();
                // Player is further than the stalking buffer, move closer
                agent.isStopped = false;
                agent.speed = stalkingSpeed;
                agent.destination = playerTransform.position;

                // Debugging: Print that the monster is moving closer
                // Debug.Log("Stalking State: Moving closer to player.");
            }
            else if (distance < stalkingBufferMin)
            {
                AnimateSlowCrawl();
                // Player is too close, move away from player
                Vector3 directionAwayFromPlayer = transform.position - playerTransform.position;
                Vector3 moveAwayTarget = transform.position + directionAwayFromPlayer.normalized * 2f; // Move away by 2 units

                agent.isStopped = false;
                agent.speed = stalkingSpeed;
                agent.destination = moveAwayTarget;

                // Debugging: Print that the monster is moving away from the player
                //Debug.Log("Stalking State: Moving away from player.");
            }
            else
            {
                // Stay in place if within the stalking buffer range
                AnimateIdle();
                agent.isStopped = true;

                // Debugging: Print that the monster is maintaining distance
                //Debug.Log("Stalking State: Maintaining stalking distance.");
            }
        }
        else
        {
            // Player left the area, monster stops stalking and idles
            currentState = MonsterState.Idle;
            //Debug.Log("Player left ForestArea, switching to Idle state.");
        }
    }


    void WatchingState()
    {
        AnimateIdle();
        // Watches the player from the edge of the ForestArea
        if (!playerInForestArea)
        {
            transform.LookAt(playerTransform);
            // Play noises based on noise timer
        }

        // If player re-enters the area, switch to Angry state
        if (playerInForestArea)
        {
            currentState = MonsterState.Angry;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (forestAreaTrigger != null)
        {
            forestAreaTrigger.OnPlayerEnteredForestArea -= PlayerEnteredForestArea;
            forestAreaTrigger.OnPlayerExitedForestArea -= PlayerExitedForestArea;
        }
    }

    private void PlayerEnteredForestArea()
    {
        playerInForestArea = true;
        Debug.Log("Monster3 detected that player entered ForestArea.");
    }

    private void PlayerExitedForestArea()
    {
        playerInForestArea = false;
        Debug.Log("Monster3 detected that player left ForestArea.");
    }


    Vector3 GetRandomPointInForestArea()
    {
        // Get the bounds of the ForestArea
        Bounds forestBounds = forestAreaTrigger.GetComponent<Collider>().bounds;

        // Generate a random point within the bounds of the ForestArea
        float randomX = Random.Range(forestBounds.min.x, forestBounds.max.x);
        float randomZ = Random.Range(forestBounds.min.z, forestBounds.max.z);

        // Set the Y to the monster's current Y position to keep it grounded
        Vector3 randomPoint = new Vector3(randomX, transform.position.y, randomZ);

        // Ensure that the random point is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            // If the random point isn't valid, return the current position to avoid errors
            Debug.LogWarning("Invalid NavMesh point generated, using current position.");
            return transform.position;
        }
    }


    void SetCrawlerAnimation(string parameter, bool state)
    {
        if (crawlerAnimator != null)
        {
            crawlerAnimator.SetBool(parameter, state);
        }
    }

    void AnimateIdle()
    {
        SetCrawlerAnimation("isCrawling", false);  // Set walking to false
        SetCrawlerAnimation("isFastCrawling", false); // Ensure running is false
        SetCrawlerAnimation("isIdle", true);    // Ensure idle is true
    }

    // Slow Crawl
    void AnimateSlowCrawl()
    {
        // Set walking to true when patrolling, disable other animations
        SetCrawlerAnimation("isCrawling", true);  // Set walking to true
        SetCrawlerAnimation("isFastCrawling", false); // Ensure running is false
        SetCrawlerAnimation("isIdle", false);    // Ensure idle is false
    }

    void AnimateChasePlayer()
    {
        // Set walking or running based on the player's distance, speed, or whatever logic you want
        SetCrawlerAnimation("isCrawling", true);  // Set running to true when chasing
        SetCrawlerAnimation("isFastCrawling", true); // Ensure walking is false
        SetCrawlerAnimation("isIdle", false);    // Ensure idle is false
    }

    void AnimateAttack()
    {
        crawlerAnimator.SetTrigger("Attack");
    }

    private void OnDrawGizmos()
    {
        if (isInitialized == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, stalkRange);
        }
    }
}
