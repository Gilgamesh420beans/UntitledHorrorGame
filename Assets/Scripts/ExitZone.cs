using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the exit zone. Loading credits scene.");

            // Call the GoToCreditpage method from GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToCreditpage();
            }
            else
            {
                Debug.LogError("GameManager instance not found.");
            }
        }
    }
}
