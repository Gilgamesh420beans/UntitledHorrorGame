using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ManagerItem : MonoBehaviour
{
    public static ManagerItem Instance;

    // Dictionary to keep track of different item counts
    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();

    // Event to notify when an item is collected
    public event Action<string, int> OnItemCollected;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddItem(string itemName)
    {
        if (itemCounts.ContainsKey(itemName))
            itemCounts[itemName]++;
        else
            itemCounts[itemName] = 1;

        // Invoke the event
        OnItemCollected?.Invoke(itemName, itemCounts[itemName]);
    }

    public int GetItemCount(string itemName)
    {
        return itemCounts.ContainsKey(itemName) ? itemCounts[itemName] : 0;
    }
}
