using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using TMPro;

public class GameManager : MonoBehaviour
{

    public GameObject monster4Enable;

    // Singleton instance
    public static GameManager Instance;

    public GameObject pauseMenuCanvas;  // Reference to the pause menu canvas
    public bool isPaused = false;  // Tracks if the game is paused
    private MouseController mouseController;
    public GameObject helpContentCanvas;  // Reference to the help content canvas
    private bool isHelpActive = false; // Tracks if help content is active

    // Variables to track key collection status
    public bool redKeyCollected = false;
    public bool yellowKeyCollected = false;
    public bool blueKeyCollected = false;
    public bool UFOKeyCollected = false;


    // References to the UI Text elements for objectives
    public TextMeshProUGUI redKeyText;
    public TextMeshProUGUI yellowKeyText;
    public TextMeshProUGUI blueKeyText;
    public TextMeshProUGUI UFOKeyText;
    public GameObject deathCanvas; // Reference to the death canvas

    // Reference to the door controller
    public DoorController doorController;

    // Reference to the exit zone GameObject
    public GameObject exitZone;

    //All keys collected
    public AudioClip allCollectedClip;
    private AudioSource audioSource;


    private bool gameEnded = false;
    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Objective List

    public void Start()
    {
        //mouseController = GetComponent<MouseController>();
        pauseMenuCanvas.SetActive(false);  // Ensure the pause menu starts inactive
        helpContentCanvas.SetActive(false);  // Ensure the help content is inactive
        UpdateObjectiveTexts();
        // Subscribe to the OnItemCollected event if ManagerItem instance exists
        if (ManagerItem.Instance != null)
        {
            ManagerItem.Instance.OnItemCollected += HandleItemCollected;
            Debug.Log("Successfully subscribed to OnItemCollected event.");
        }
        else
        {
            Debug.LogError("ManagerItem instance is null. Cannot subscribe to OnItemCollected.");
        }
    }

    void Update()
    {
        // Check if the ESC key is pressed
        if (deathCanvas != null && deathCanvas.activeSelf)
        {
            // If the death canvas is active, do nothing on pressing Escape
            return;
        } else if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Check if the ESC key is pressed

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


    void UpdateObjectiveTexts()
    {
        redKeyText.text = "Red Key: " + (redKeyCollected ? "Collected" : "Ongoing");
        yellowKeyText.text = "Gold Key: " + (yellowKeyCollected ? "Collected" : "Ongoing");
        blueKeyText.text = "Blue Key: " + (blueKeyCollected ? "Collected" : "Ongoing");
        UFOKeyText.text = "UFO Key: " + (UFOKeyCollected ? "Collected" : "Ongoing");
    }

    public void CollectRedKey()
    {
        redKeyCollected = true;
        UpdateObjectiveTexts();
        CheckAllKeysCollected();
    }

    public void CollectYellowKey()
    {
        yellowKeyCollected = true;
        UpdateObjectiveTexts();
        CheckAllKeysCollected();
    }

    public void CollectBlueKey()
    {
        blueKeyCollected = true;
        UpdateObjectiveTexts();
        CheckAllKeysCollected();
    }

    public void CollectUFOKey()
    {
        UFOKeyCollected = true;
        UpdateObjectiveTexts();
        CheckAllKeysCollected();
    }


    private void CheckAllKeysCollected()
    {

        if (redKeyCollected)
        {
            // Enable the specified GameObject
            if (monster4Enable != null)
            {
                monster4Enable.SetActive(true);
            }
        }

        if (redKeyCollected && yellowKeyCollected &&
            blueKeyCollected && UFOKeyCollected)
        {
            Debug.Log("All keys collected! Opening the door and activating exit zone.");
            // Play the all keys collected clip
            if (audioSource != null)
            {
                audioSource.PlayOneShot(allCollectedClip);
            }
            // Open the door
            if (doorController != null)
            {
                doorController.OpenDoor();
            }
            else
            {
                Debug.LogError("DoorController reference not set in GameManager.");
            }

            // Activate the exit zone
            if (exitZone != null)
            {
                exitZone.SetActive(true);
            }
            else
            {
                Debug.LogError("ExitZone reference not set in GameManager.");
            }
        }
    }

    public void PauseGame()
    {
        // If the help content is active, disable it first
        if (isHelpActive)
        {
            CloseHelp();
        }

        pauseMenuCanvas.SetActive(true);  // Show the pause menu
        Time.timeScale = 0f;  // Freeze the game
        isPaused = true;  // Mark the game as paused

        // Unlock and show the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }



    public void ResumeGame()
    {
        Debug.Log("PauseGame triggered, checking for active AudioSources.");
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source.isPlaying)
            {
                Debug.Log("Playing audio: " + source);
            }
        }

        if (isHelpActive)
        {
            CloseHelp();
        }
        pauseMenuCanvas.SetActive(false);  // Hide the pause menu
        Time.timeScale = 1f;  // Unfreeze the game
        isPaused = false;  // Mark the game as unpaused

        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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


    public void PlayerDied()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            Debug.Log("Player has died. Returning to the main menu.");

            StartCoroutine(ReturnToMainMenu());
            ResetLevel();
        }
    }
    // Function to return to the StartPage
    public void ReturnToMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;  // Ensure the game is running before loading the next scene
        SceneManager.LoadScene("StartPage");
    }

    public void GoToCreditpage()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;  // Ensure the game is running before loading the next scene
        SceneManager.LoadScene("Creditpage");
    }

    public void FinaliseGame()
    {
        StartCoroutine(ReturnToMainMenu());
        ResetLevel();
    }
    public void ResetLevel()
    {
        StartCoroutine(ResetLevelWithDelay(2f)); // Call the coroutine with a 1-second delay
    }
    private IEnumerator ResetLevelWithDelay(float delay)
    {
        // Wait for the specified delay (1 second in this case)
        yield return new WaitForSeconds(delay);

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(1f);  // Optional delay before returning to main menu
        SceneManager.LoadScene("StartPage");  // Load the main menu scene
    }

    public IEnumerator GameEnd()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        yield return new WaitForSeconds(1f);  // Optional delay before returning to main menu

        // Load the credits scene
        SceneManager.LoadScene("Creditpage");  // Make sure the name matches the actual scene name
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
        Debug.Log($"Item collected: {itemName}, Count: {count}");

        if (itemName != null)
        {
            Debug.Log($"Item Name: {itemName}, Count: {count}");
        }

        if (itemName == "Key" && count == 2)
        {
            // Trigger the event, e.g., unlock a door
            UnlockSecretDoor();
        }

        if (itemName == "UFOKey")
        {
            // Trigger the event to make MotherMonster and Monster3 clones angry
            Debug.Log("UFOKey collected! Triggering angry state for MotherMonster and Monster3 clones.");
            TriggerMonstersToBecomeAngry();
        }
    }
    private void TriggerMonstersToBecomeAngry()
    {
        // Find and trigger MotherMonster
        MotherMonster motherMonster = FindObjectOfType<MotherMonster>();
        if (motherMonster != null)
        {
            motherMonster.TriggerAngryMode();
            Debug.Log("MotherMonster has been triggered to become angry.");
        }

        // Find all Monster3 instances and trigger their angry state
        Monster3[] allMonster3s = FindObjectsOfType<Monster3>();
        foreach (Monster3 monster in allMonster3s)
        {
            monster.TriggerAngryMode();
            Debug.Log("Monster3 clone has been triggered to become angry.");
        }
    }
    private void UnlockSecretDoor()
    {
        // Test implementation of unlocking a door
        Debug.Log("Secret door unlocked!");
    }

    // ---- End of ManagerGame functionality ----
}
