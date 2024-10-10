using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance;

    public GameObject pauseMenuCanvas;  // Reference to the pause menu canvas
    private bool isPaused = false;  // Tracks if the game is paused
    private MouseController mouseController;
    public GameObject helpContentCanvas;  // Reference to the help content canvas
    private bool isHelpActive = false; // Tracks if help content is active

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        //mouseController = GetComponent<MouseController>();
        pauseMenuCanvas.SetActive(false);  // Ensure the pause menu starts inactive
        helpContentCanvas.SetActive(false);  // Ensure the help content is inactive
    }

    void Update()
    {
        // Check if the ESC key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenuCanvas.SetActive(true);  // Show the pause menu
        FirstPersonController fpsController = FindObjectOfType<FirstPersonController>();
        if (fpsController != null)
        {
            fpsController.EnableCameraMovement(false); // Disable camera movement
        }
        Time.timeScale = 0f;  // Freeze the game
        isPaused = true;  // Mark the game as paused
    }

    public void ResumeGame()
    {
        pauseMenuCanvas.SetActive(false);  // Hide the pause menu
        FirstPersonController fpsController = FindObjectOfType<FirstPersonController>();
        if (fpsController != null)
        {
            fpsController.EnableCameraMovement(true); // Enable camera movement
        }
        Time.timeScale = 1f;  // Unfreeze the game
        isPaused = false;  // Mark the game as unpaused
    }

    // Function to return to the StartPage
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;  // Ensure the game is running before loading the next scene
        SceneManager.LoadScene("StartPage");
    }

    // Function to quit the game
    public void QuitGame()
    {
        Application.Quit();

        // For testing purposes inside the Unity editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif



    }
    public void ShowHelp()
    {
        // Disable pause menu content and show the help content
        pauseMenuCanvas.SetActive(false);
        helpContentCanvas.SetActive(true);
        isHelpActive = true;  // Track that help content is active
    }

    public void CloseHelp()
    {
        // Hide help content and show the pause menu again
        helpContentCanvas.SetActive(false);
        pauseMenuCanvas.SetActive(true);
        isHelpActive = false;
    }
    // ---- Start of ManagerGame functionality ----

    private void OnEnable()
    {
        if (ManagerItem.Instance != null)
        {
            ManagerItem.Instance.OnItemCollected += HandleItemCollected;
        }
    }

    private void OnDisable()
    {
        if (ManagerItem.Instance != null)
        {
            ManagerItem.Instance.OnItemCollected -= HandleItemCollected;
        }
    }

    private void HandleItemCollected(string itemName, int count)
    {
        // Debug purposes
        if (itemName != null)
        {
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
        // Test implementation of unlocking a door
        Debug.Log("Secret door unlocked!");
    }

    // ---- End of ManagerGame functionality ----
}
