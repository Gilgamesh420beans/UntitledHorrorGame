using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
public class Monster2 : MonoBehaviour
{
    public enum MonsterState
    {
        Patrolling,
        Chasing,
        Freezing
    }

    public MonsterState curState = MonsterState.Patrolling;

    public Transform[] patrolPoints;  // Waypoints for patrolling
    private int currentPatrolIndex = 0;

    private Vector3 lastHeardPosition = Vector3.zero;  // The position where the monster last heard the player
    public float chaseSpeed = 5f;
    public float patrolSpeed = 10f;
    public float hearingRange = 40f;   // Range within which the monster can hear footsteps
    public float fieldOfViewAngle = 30f;  // Angle for the monster to detect the player looking at it

    private bool isPlayerLooking = false;
    private Transform playerTransform;

    void Start()
    {
        // Assign player transform
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found in the scene!");
        }

        // Subscribe to the player's footstep event
        FirstPersonController.OnFootstep += OnPlayerFootstep;


        // Check if patrol points are assigned
        if (patrolPoints.Length == 0)
        {
            Debug.LogError("No patrol points assigned!");
        }
    }

    void OnDestroy()
    {
        // unSubscribe to the player's footstep event
        FirstPersonController.OnFootstep -= OnPlayerFootstep;
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        // Check if player is looking at the monster
        CheckIfPlayerIsLooking();

        switch (curState)
        {
            case MonsterState.Patrolling:
                Patrol();
                break;
            case MonsterState.Chasing:
                Chase();
                break;
            case MonsterState.Freezing:
                // Do nothing
                break;
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        MoveTowards(targetPoint.position, patrolSpeed);

        // If reached the patrol point, move to next
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    void Chase()
    {
        if (lastHeardPosition != Vector3.zero)
        {
            MoveTowards(lastHeardPosition, chaseSpeed);

            // If reached the last heard position, go back to patrolling
            if (Vector3.Distance(transform.position, lastHeardPosition) < 0.1f)
            {
                lastHeardPosition = Vector3.zero;
                curState = MonsterState.Patrolling;
            }
        }
        else
        {
            // No last heard position, switch to patrolling
            curState = MonsterState.Patrolling;
        }
    }

    void MoveTowards(Vector3 targetPosition, float speed)
    {
        if (curState != MonsterState.Freezing)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
    }

    void OnPlayerFootstep(Vector3 playerPosition)
    {
        Debug.Log("Monster2 heard a footstep at position: " + playerPosition);

        // Check if the monster can hear the player
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
        if (distanceToPlayer <= hearingRange)
        {
            Debug.Log("Monster2 now chasing footstep at position: " + playerPosition);
            lastHeardPosition = playerPosition;
            curState = MonsterState.Chasing;
        }
        else
        {
            Debug.Log("Player footstep is out of hearing range.");
        }
    }


    void CheckIfPlayerIsLooking()
    {
        Vector3 directionToMonster = transform.position - playerTransform.position;
        float angle = Vector3.Angle(playerTransform.forward, directionToMonster);

        if (angle < fieldOfViewAngle)
        {
            RaycastHit hit;
            Vector3 rayOrigin = playerTransform.position + Vector3.up * 1.5f; // Adjust for player's eye height
            if (Physics.Raycast(rayOrigin, directionToMonster.normalized, out hit))
            {
                if (hit.transform == this.transform)
                {
                    if (!isPlayerLooking)
                    {
                        isPlayerLooking = true;
                        curState = MonsterState.Freezing;
                    }
                    return;
                }
            }
        }

        if (isPlayerLooking)
        {
            isPlayerLooking = false;
            // Return to previous state
            if (lastHeardPosition != Vector3.zero)
            {
                curState = MonsterState.Chasing;
            }
            else
            {
                curState = MonsterState.Patrolling;
            }
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
       

        Gizmos.color = Color.magenta; 
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        // Calculate the size of the maze based on the min and max bounds
        //Vector3 mazeSize = mazeMaxBounds - mazeMinBounds;

        // Draw a wireframe cube representing the maze bounds
        //Gizmos.DrawWireCube(mazeMinBounds + mazeSize / 2, mazeSize);
    }
}
