using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class MouseController : MonoBehaviour
{
    private MouseLook mouseLook;

    void Start()
    {
        // Get the MouseLook instance from FirstPersonController
        FirstPersonController fpsController = FindObjectOfType<FirstPersonController>();
        if (fpsController != null)
        {
            mouseLook = fpsController.GetMouseLook();
        }

        // Lock the cursor at the start
        LockCursor();
    }

    void Update()
    {
        // Unlock the cursor when the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        // Lock the cursor when the left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
    }

    public void LockCursor()
    {
        if (mouseLook != null)
        {
            mouseLook.SetCursorLock(true);
            // No need to call UpdateCursorLock() if SetCursorLock() handles it
        }
    }

    public void UnlockCursor()
    {
        if (mouseLook != null)
        {
            mouseLook.SetCursorLock(false);
            // No need to call UpdateCursorLock() if SetCursorLock() handles it
        }
    }
}
