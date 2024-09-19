using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditpageManager : MonoBehaviour
{
    // This function will be called when the button is pressed
    public void LoadStartPage()
    {
        // Load the StartPage scene by name
        SceneManager.LoadScene("StartPage");
    }
}
