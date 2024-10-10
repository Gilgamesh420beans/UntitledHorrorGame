using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MotherMonster : MonoBehaviour
{
    public enum MotherMonsterState
    {
        Idle,
        Spawning,
        Angry,
        Watching,
        Unlimited,
        Attacking
    }

    public MotherMonsterState currentState = MotherMonsterState.Idle;

    public GameObject monster3Prefab; // The prefab for Monster3
    public Transform spawnPoint; // The spawn point for Monster3
    public float minSpawnInterval = 180f; // Minimum 3 minutes
    public float maxSpawnInterval = 300f; // Maximum 5 minutes
    public float baseChaseSpeed = 10f;
    public float baseAttackRange = 3f;
    public float growthDuration = 30f; // Time taken to grow to maximum (can be changed later)
    public float maxSizeMultiplier = 3f; // Max size multiplier when angry
    public float attackDistance = 5f; // The range to trigger an attack

    private Transform playerTransform;
    private bool playerInForestArea = false;
    private bool isAngry = false;
    private NavMeshAgent agent;

    private float timeToNextSpawn;
    private float currentChaseSpeed;
    private float currentAttackRange;
    private float growthProgress = 0f; // How far along in the growth process (0 to 1)
    private Vector3 originalSize;
    private float originalSpeed;
    private float originalAttackRange;

    private ForestAreaTrigger forestAreaTrigger; // Reference to the ForestAreaTrigger component

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        originalSize = transform.localScale;
        originalSpeed = baseChaseSpeed;
        originalAttackRange = baseAttackRange;

        currentChaseSpeed = originalSpeed;
        currentAttackRange = originalAttackRange;

        StartCoroutine(SpawnTimer());

        // Find and subscribe to the ForestAreaTrigger events
        GameObject forestAreaObject = GameObject.FindGameObjectWithTag("ForestArea");
        if (forestAreaObject != null)
        {
            forestAreaTrigger = forestAreaObject.GetComponent<ForestAreaTrigger>();
            if (forestAreaTrigger != null)
            {
                forestAreaTrigger.OnPlayerEnteredForestArea += HandlePlayerEnteredForestArea;
                forestAreaTrigger.OnPlayerExitedForestArea += HandlePlayerExitedForestArea;
            }
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case MotherMonsterState.Idle:
                IdleState();
                break;
            case MotherMonsterState.Spawning:
                SpawningState();
                break;
            case MotherMonsterState.Angry:
                AngryState();
                break;
            case MotherMonsterState.Watching:
                WatchingState();
                break;
            case MotherMonsterState.Unlimited:
                UnlimitedState();
                break;
            case MotherMonsterState.Attacking:
                AttackState();
                break;
        }
    }

    void IdleState()
    {
        Debug.Log($"MotherMonster is idle. Time to next spawn: {timeToNextSpawn:F2} seconds");

        if (timeToNextSpawn <= 0)
        {
            Debug.Log("MotherMonster is spawning Monster3.");
            currentState = MotherMonsterState.Spawning;
        }

        if (isAngry)
        {
            currentState = MotherMonsterState.Angry;
        }
    }

    void SpawningState()
    {
        Instantiate(monster3Prefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("MotherMonster spawned Monster3.");
        timeToNextSpawn = Random.Range(minSpawnInterval, maxSpawnInterval);
        currentState = MotherMonsterState.Idle;
    }

    void AngryState()
    {
        if (playerInForestArea)
        {
            agent.isStopped = false;
            agent.speed = currentChaseSpeed;
            agent.destination = playerTransform.position;
            GrowOverTime();

            // Use currentAttackRange to trigger an attack
            if (Vector3.Distance(transform.position, playerTransform.position) <= currentAttackRange)
            {
                currentState = MotherMonsterState.Attacking;
            }
        }
        else
        {
            agent.isStopped = true;
            currentState = MotherMonsterState.Watching;
        }
    }

    void WatchingState()
    {
        GrowOverTime();

        if (playerInForestArea)
        {
            currentState = MotherMonsterState.Angry;
        }
    }

    void UnlimitedState()
    {
        agent.isStopped = false;
        agent.speed = currentChaseSpeed;
        agent.destination = playerTransform.position;

        // Use currentAttackRange to trigger an attack
        if (Vector3.Distance(transform.position, playerTransform.position) <= currentAttackRange)
        {
            currentState = MotherMonsterState.Attacking;
        }
    }

    void AttackState()
    {
        Debug.Log("MotherMonster is attacking the player!");
        // Add attack logic here, such as reducing player health or triggering game over.
    }

    // Function to handle growth logic
    void GrowOverTime()
    {
        if (growthProgress < 1f)
        {
            growthProgress += Time.deltaTime / growthDuration;
            transform.localScale = Vector3.Lerp(originalSize, originalSize * maxSizeMultiplier, growthProgress);
            currentChaseSpeed = Mathf.Lerp(originalSpeed, originalSpeed * maxSizeMultiplier, growthProgress);

            // Set attack range to 2.5x the current size of the monster
            currentAttackRange = 2.5f * transform.localScale.magnitude;

            Debug.Log($"Growing: Size = {transform.localScale}, Speed = {currentChaseSpeed}, Attack Range = {currentAttackRange}");
        }

        if (growthProgress >= 1f && transform.localScale == originalSize * maxSizeMultiplier)
        {
            Debug.Log("MotherMonster has reached max size and is now Unlimited.");
            currentState = MotherMonsterState.Unlimited;
        }
    }


    IEnumerator SpawnTimer()
    {
        while (true)
        {
            if (currentState == MotherMonsterState.Idle && timeToNextSpawn > 0)
            {
                timeToNextSpawn -= Time.deltaTime;
            }
            yield return null;
        }
    }

    // Event handler for player entering the forest area
    private void HandlePlayerEnteredForestArea()
    {
        playerInForestArea = true;
        Debug.Log("Player entered the forest area via event in MotherMonster.");
    }

    // Event handler for player exiting the forest area
    private void HandlePlayerExitedForestArea()
    {
        playerInForestArea = false;
        Debug.Log("Player exited the forest area via event in MotherMonster.");
    }

    public void TriggerAngryMode()
    {
        isAngry = true;
        currentState = MotherMonsterState.Angry;
        Debug.Log("MotherMonster is now Angry!");
    }

    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position, 1f);
        }

        // Display the attack range
        if (Application.isPlaying)
        {
            // During runtime, use the currentAttackRange
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, currentAttackRange);
        }
        else
        {
            // In the editor (before runtime), use the base attack range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, baseAttackRange);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to avoid memory leaks
        if (forestAreaTrigger != null)
        {
            forestAreaTrigger.OnPlayerEnteredForestArea -= HandlePlayerEnteredForestArea;
            forestAreaTrigger.OnPlayerExitedForestArea -= HandlePlayerExitedForestArea;
        }
    }
}
