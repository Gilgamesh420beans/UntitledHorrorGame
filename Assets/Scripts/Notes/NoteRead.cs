using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // Import the TextMeshPro namespace

public class NoteRead : MonoBehaviour
{
    public GameObject player;
    public GameObject noteUI;
    public GameObject compass;
    public AudioClip pickUpClip;
    public GameObject objectToHighlight; 
    public Color highlightColor = Color.yellow;  // The color to use for highlighting
    private Color originalColor;
    private Renderer objectRenderer;   // To control the object's material
    public TextMeshProUGUI interactText;  // Use TextMeshProUGUI for UI Text (TMP)
    public bool isInReach;
    private bool isInteractionActive;
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        objectRenderer = objectToHighlight.GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;

        noteUI.SetActive(false);
        compass.SetActive(true);
        interactText.gameObject.SetActive(false);

        isInReach = false;
        isInteractionActive = false;
    }

    private void Update() {
        if (isInReach && Input.GetKeyDown(KeyCode.E)){
            isInteractionActive = !isInteractionActive;
            noteUI.SetActive(isInteractionActive);
            compass.SetActive(!isInteractionActive); // Hide the compass when noteUI is active
            interactText.gameObject.SetActive(false);
            // Play the audio clip
            audioSource.PlayOneShot(pickUpClip);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInReach = true;
            interactText.gameObject.SetActive(true);
            interactText.text = "[E]";

            // Highlight note
            objectRenderer.material.color = highlightColor;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInReach = false;
            interactText.gameObject.SetActive(false);

            // Reset color to original
            objectRenderer.material.color = originalColor;
        }
    }
}
