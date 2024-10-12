using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator doorAnimator; // Assign if using animation
    public bool isOpen = false;
    public float radius = 5f;  // The radius within which the door should deactivate
    private Transform playerTransform;

    void Start()
    {
        if (doorAnimator == null)
        {
            //doorAnimator = GetComponent<Animator>();
        }

        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // Calculate the distance between the player and the door
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // If the player is within the radius, deactivate the door
            if (distanceToPlayer <= radius)
            {
                DeactivateDoor();
            }
        }
    }

    public void DeactivateDoor()
    {
        if (gameObject.activeSelf)
        {
            // Deactivate the entire door GameObject
            gameObject.SetActive(false);
            Debug.Log("Door has been deactivated.");
        }
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;

            // If using animation
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("Open");
            }
            else
            {
                // If not using animation, disable the collider
                Collider doorCollider = GetComponent<Collider>();
                if (doorCollider != null)
                {
                    doorCollider.enabled = false;
                }
            }

            // Optional: Log for debugging
            Debug.Log("Door has been opened.");
        }
    }
}
