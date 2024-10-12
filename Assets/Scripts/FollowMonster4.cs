using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMonster4 : MonoBehaviour
{
    public GameObject monster4;   // Reference to Monster4
    public Vector3 offset = new Vector3(0, 1, 0);  // Default offset (adjustable in Inspector)

    private void Start()
    {
        if (monster4 == null)
        {
            Debug.LogError("Monster4 is not assigned in the FollowMonster4 script.");
        }
        else
        {
            Debug.Log("Monster4 successfully assigned in FollowMonster4 script.");
        }
    }

    private void Update()
    {
        if (monster4 != null)
        {
            // Follow the monster4 position with an offset
            Vector3 targetPosition = monster4.transform.position + offset;
            transform.position = targetPosition;

            //Debug.Log($"Key is following Monster4 at position: {targetPosition}");
        }
        else
        {
            Debug.LogWarning("Monster4 reference is missing or has been removed.");
        }
    }
}
