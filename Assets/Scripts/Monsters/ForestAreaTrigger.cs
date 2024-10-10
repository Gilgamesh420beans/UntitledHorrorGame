using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestAreaTrigger : MonoBehaviour
{
    public delegate void PlayerEnteredForestAreaHandler();
    public event PlayerEnteredForestAreaHandler OnPlayerEnteredForestArea;

    public delegate void PlayerExitedForestAreaHandler();
    public event PlayerExitedForestAreaHandler OnPlayerExitedForestArea;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered ForestArea.");
            OnPlayerEnteredForestArea?.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left ForestArea.");
            OnPlayerExitedForestArea?.Invoke();
        }
    }
}
