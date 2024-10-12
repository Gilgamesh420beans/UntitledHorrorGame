using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // Movement parameters
    public float walkSpeed = 4f; // Walking speed
    public float sprintSpeed = 6f; // Sprinting speed
    public float crouchSpeed = 2.5f; // Crouch speed
    public float backwardsSpeed = 2.5f; // Backwards speed

    // Jumping
    private float jumpHeight = 1.2f;
    private float gravity = -20f; // Increased gravity for better fall speed
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded; // To detect landing

    // Energy
    public float maxEnergy = 200f;
    public float energySprintDrain = 20f; // per second
    public float energyRecoveryRate = 30f; // per second
    public float energyRecoveryDelay = 1f;
    private float currentEnergy;
    private float energyRecoveryTimer;

    // Health
    public float health = 100f;
    private bool isDead = false;

    // Mouse look
    public float mouseSensitivity = 80f;  // Adjusted for better control
    private float xRotation = 0f;  // To track vertical rotation

    // Crouching
    private bool isCrouching;
    private float crouchScale = 0.9f;  // Scale factor for crouching (Y-axis)
    private float crouchTransitionSpeed = 10f;  // Speed of transition when crouching
    private float crouchCameraHeightAdjustment = 0.9f;  // How much the camera moves during crouch

    // References
    private CharacterController controller;  // Reference to the CharacterController component
    public Transform cameraTarget;
    private Camera mainCamera;
    public Transform head;
    public LayerMask groundMask;  // Layer mask for detecting ground

    // Original values
    private Vector3 originalCameraTargetPosition;
    private Vector3 originalScale;

    // UI elements
    public Image healthbar;
    public Image energybar;
    private float originalHealthbarWidth;
    private float originalEnergybarWidth;

    // Footstep variables
    public float stepInterval = 0.5f;  // Time between footsteps for normal walking
    private float crouchStepInterval = 15f;  // Slower footsteps when crouching
    private float stepCycle = 0f;
    private AudioSource audioSource;
    public AudioClip[] footstepClips;  // Array of footstep sounds
    public AudioClip jumpClip;
    public AudioClip landClip;

    private float jumpCooldownTime = 1f;  // Cooldown duration in seconds
    private float jumpCooldownTimer = 0f;

    // Footstep event
    public delegate void FootstepEventHandler(Vector3 position);
    public static event FootstepEventHandler OnFootstep;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentEnergy = maxEnergy;
        mainCamera = Camera.main;
        originalScale = transform.localScale;
        originalCameraTargetPosition = cameraTarget.localPosition;
        originalHealthbarWidth = healthbar.rectTransform.sizeDelta.x;
        originalEnergybarWidth = energybar.rectTransform.sizeDelta.x;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        wasGrounded = true; // Initialize as grounded
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isDead || GameManager.Instance.isPaused)
        {
            return;  // Skip updating if the game is paused or player is dead
        }

        // Decrease the jump cooldown timer
        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }

        UpdateMovement();        // Move the character
        UpdateGroundDetection(); // Detect if the player is on the ground
        UpdateMouseLook();
        UpdateSprint();
        UpdateCrouch();
        UpdateJump();
        UpdateEnergy();
        UpdateFOV();
        UpdateFootsteps();

        // Check if the player is dead
        if (health <= 0)
        {
            Die();
        }
    }

    void UpdateMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        float speed = walkSpeed;

        // Handle speed adjustments
        if (z < 0)
        {
            speed = backwardsSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftShift) && currentEnergy > 0 && !isCrouching)
        {
            speed = sprintSpeed;
        }
        else if (isCrouching)
        {
            speed = crouchSpeed;
        }

        Vector3 horizontalMove = move * speed;

        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep the player grounded
        }

        // Apply gravity to vertical velocity
        velocity.y += gravity * Time.deltaTime;

        // Combine horizontal and vertical movement
        Vector3 movement = (horizontalMove + Vector3.up * velocity.y) * Time.deltaTime;

        // Move the character
        controller.Move(movement);

        // Update isGrounded after moving
        isGrounded = controller.isGrounded;
    }

    void UpdateGroundDetection()
    {
        // Handle landing sound
        if (!wasGrounded && isGrounded)
        {
            if (landClip != null)
            {
                audioSource.PlayOneShot(landClip);
            }
        }
        wasGrounded = isGrounded; // Track previous ground state
    }

    void UpdateMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        head.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void UpdateSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0 && currentEnergy > 0 && !isCrouching)
        {
            currentEnergy -= energySprintDrain * Time.deltaTime;
            energyRecoveryTimer = energyRecoveryDelay;
        }
        else
        {
            energyRecoveryTimer -= Time.deltaTime;
        }
    }

    void UpdateCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
        }

        if (isCrouching)
        {
            transform.localScale = Vector3.Lerp(transform.localScale,
                new Vector3(originalScale.x, crouchScale, originalScale.z),
                Time.deltaTime * crouchTransitionSpeed);

            cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition,
                new Vector3(originalCameraTargetPosition.x, originalCameraTargetPosition.y - crouchCameraHeightAdjustment, originalCameraTargetPosition.z),
                Time.deltaTime * crouchTransitionSpeed);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * crouchTransitionSpeed);

            cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, originalCameraTargetPosition, Time.deltaTime * crouchTransitionSpeed);
        }
    }

    void UpdateJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && jumpCooldownTimer <= 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            currentEnergy -= 10f;  // Reduce energy on jump
            jumpCooldownTimer = jumpCooldownTime;  // Reset cooldown timer

            // Play jump sound
            if (jumpClip != null)
            {
                audioSource.PlayOneShot(jumpClip);
            }
        }
    }

    void UpdateEnergy()
    {
        if (!Input.GetKey(KeyCode.LeftShift) && energyRecoveryTimer <= 0)
        {
            currentEnergy += energyRecoveryRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        }

        float newEnergyWidth = (currentEnergy / maxEnergy) * originalEnergybarWidth;
        energybar.rectTransform.sizeDelta = new Vector2(newEnergyWidth, energybar.rectTransform.rect.height);
    }

    void UpdateFOV()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0 && currentEnergy > 0 && !isCrouching)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, 75f, Time.deltaTime * 2);
        }
        else
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, 60f, Time.deltaTime * 2);
        }
    }

    void UpdateFootsteps()
    {
        float currentStepInterval = isCrouching ? crouchStepInterval : stepInterval;

        if (controller.velocity.magnitude > 0.1f && isGrounded)
        {
            stepCycle += Time.deltaTime * (controller.velocity.magnitude + (isCrouching ? 0.5f : 1f));

            if (stepCycle >= currentStepInterval)
            {
                PlayFootstepSound();
                stepCycle = 0f;
            }
        }
        else
        {
            stepCycle = 0f;
        }
    }

    void PlayFootstepSound()
    {
        if (footstepClips.Length > 0)
        {
            int n = Random.Range(0, footstepClips.Length);
            AudioClip clip = footstepClips[n];
            audioSource.PlayOneShot(clip);
        }
        OnFootstep?.Invoke(transform.position);
    }

    public void ApplyDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, 100);

        float newWidth = (health / 100f) * originalHealthbarWidth;
        healthbar.rectTransform.sizeDelta = new Vector2(newWidth, healthbar.rectTransform.rect.height);
    }


    public GameObject deadCanvas;  // Reference to the Dead canvas

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        Cursor.lockState = CursorLockMode.None;
        // Freeze the game world
        Time.timeScale = 0f;

        // Display the dead canvas
        if (deadCanvas != null)
        {
            deadCanvas.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ExitZone"))
        {
            GameManager.Instance.FinaliseGame();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (controller != null)
        {
            Gizmos.color = Color.red;
            Vector3 rayOrigin = controller.bounds.center;
            rayOrigin.y -= controller.bounds.extents.y;
            Gizmos.DrawRay(rayOrigin, Vector3.down * 0.2f);
        }
    }
}
