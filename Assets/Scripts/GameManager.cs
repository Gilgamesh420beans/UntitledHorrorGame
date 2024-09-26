using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class GameManager : MonoBehaviour
{
    public GameObject pauseMenuCanvas;  // Reference to the pause menu canvas
    private bool isPaused = false;  // Tracks if the game is paused
    private MouseController mouseController;
    void Start()
    {
        //mouseController = GetComponent<MouseController>();
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

    // Function to resume the game and hide the pause menu
    //TODO:
    /*
        Must contain functionality that resumes the entire scene/game     
     */
    public void ResumeGame()
    {
        pauseMenuCanvas.SetActive(false);  // Hide the pause menu
        FirstPersonController fpsController = FindObjectOfType<FirstPersonController>();
        if (fpsController != null)
        {
            fpsController.EnableCameraMovement(true); // Disable camera movement
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
}
