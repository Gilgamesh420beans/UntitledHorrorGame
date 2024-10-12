using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityStandardAssets.Characters.FirstPerson;  // Import the TextMeshPro namespace

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
    public float interactionDistance = 5f;  // Max distance for interaction
    public Camera playerCamera;  // Reference to player's camera for raycasting
    private bool hasPlayedAudio = false;   // To track whether the audio has been played

    private FirstPersonController fpsController;  // Reference to FirstPersonController script
    private Vector2 originalTextPosition;  // Store the original position of the interact text

    public Vector2 noteOpenTextPosition = new Vector2(0, 100);  // Position to move interactText when note is open

    void Start()
    {
        objectRenderer = objectToHighlight.GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;

        noteUI.SetActive(false);
        compass.SetActive(true);
        interactText.gameObject.SetActive(false);

        isInReach = false;
        isInteractionActive = false;

        // Get the FirstPersonController component from the player object
        fpsController = player.GetComponent<FirstPersonController>();

        // Store the original position of the interactText
        originalTextPosition = interactText.rectTransform.anchoredPosition;
    }

    private void Update() 
    {
        RaycastHit hit;
        // Perform raycast from the player's camera forward direction
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionDistance))
        {
            // Check if the raycast hits the object we want to interact with
            if (hit.collider.gameObject == objectToHighlight)
            {
                isInReach = true;
                interactText.gameObject.SetActive(true);
                interactText.text = "[E]";

                // Highlight object
                objectRenderer.material.color = highlightColor;

                // Check for interaction input
                if (Input.GetKeyDown(KeyCode.E))
                {
                    isInteractionActive = !isInteractionActive;
                    noteUI.SetActive(isInteractionActive);
                    compass.SetActive(!isInteractionActive); // Hide compass when noteUI is active
                    interactText.gameObject.SetActive(false);

                    // Lock/Unlock player movement based on interaction
                    LockPlayerMovement(isInteractionActive);

                    // Adjust the interact text position
                    if (isInteractionActive)
                    {
                        // Move interact text when the note is open, dirty way to hide text, fixes conflict
                        interactText.rectTransform.anchoredPosition = noteOpenTextPosition;
                    }
                    else
                    {
                        // Restore the original text position
                        interactText.rectTransform.anchoredPosition = originalTextPosition;
                    }

                    // Play the audio only if it hasn't been played yet
                    if (!hasPlayedAudio)
                    {
                        audioSource.PlayOneShot(pickUpClip);
                        hasPlayedAudio = true;  // Mark audio as played
                    }
                }
            }
            else
            {
                ResetInteraction();
            }
        }
        else
        {
            ResetInteraction();
        }
    }

    void LockPlayerMovement(bool lockMovement)
    {
        // Enable or disable the FPS controller based on interaction state
        if (fpsController != null)
        {
            fpsController.enabled = !lockMovement;
        }
    }

    void ResetInteraction()
    {
        isInReach = false;
        interactText.gameObject.SetActive(false);

        // Reset color to original
        objectRenderer.material.color = originalColor;
    }
}
