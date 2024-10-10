using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerGame : MonoBehaviour
{
 public static ManagerGame Instance;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        ManagerItem.Instance.OnItemCollected += HandleItemCollected;
    }

    private void OnDisable()
    {
        ManagerItem.Instance.OnItemCollected -= HandleItemCollected;
    }

    private void HandleItemCollected(string itemName, int count)
    {
        // Debug purposes
        if(itemName != null){
            Debug.Log($"Item Name: {itemName}, Count: {count}");
        }

        if (itemName == "Key" && count == 2)
        {
            // Trigger the event, e.g., unlock a door
            UnlockSecretDoor();
        }
    }

    private void UnlockSecretDoor()
    {
        // Test impl 
        Debug.Log("Secret door unlocked!");
    }
}
