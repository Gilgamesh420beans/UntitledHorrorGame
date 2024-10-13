using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartPageManager : MonoBehaviour
{
    // This function will be called when the "Play" button is pressed
    public void LoadLevel()
    {
        // Load the Level scene by name
        SceneManager.LoadScene("LevelAdrian");
    }
    public void LoadCredits()
    {
        // Load the Level scene by name
        SceneManager.LoadScene("Creditpage");
    }

    // This function will be called when the "Quit" button is pressed
    public void QuitGame()
    {
        Debug.Log("Quit Pressed");
        // Exit the application
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

    }
}
