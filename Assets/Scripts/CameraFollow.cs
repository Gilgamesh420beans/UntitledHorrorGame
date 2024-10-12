using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

// Separating camera rotation from the character ensures smooth movement by avoiding conflicts 
// between their motions. LateUpdate is used for the camera to follow the character smoothly, updating 
// after all movements to prevent stuttering, especially when using a CharacterController
public class CameraFollow : MonoBehaviour
{
    public string targetTag = "CameraTarget"; // Camera target tag

    private Transform cameraTarget;

    private void Start()
    {
        FindTarget();
    }

    private void FindTarget()
    {
        cameraTarget = GameObject.FindGameObjectWithTag(targetTag)?.transform;
        if (cameraTarget == null)
        {
            Debug.LogError("Camera target not found. Ensure the object is tagged as 'CameraTarget'.");
            enabled = false; // Disable the script if the target is not found
        }
    }

    private void LateUpdate()
    {
        if (cameraTarget != null)
        {
            transform.position = cameraTarget.position;
            transform.rotation = cameraTarget.rotation;
        }
    }
}