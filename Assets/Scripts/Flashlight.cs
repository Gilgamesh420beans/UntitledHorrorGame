using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public GameObject flashlight;

    public AudioSource turnOn;
    public AudioSource turnOff;

    private bool isOn;  // Use a single boolean to track flashlight state

    // Raycast parameters
    private float raycastRange = 100f;  // Adjust as needed
    public LayerMask layerMask = ~0;  // Include all layers by default

    // For Gizmo visualization
    private Vector3 gizmoOrigin;
    private Vector3 gizmoDirection;
    private bool gizmoHit = false;
    private RaycastHit gizmoHitInfo;


    void Start()
    {
        isOn = false;
        flashlight.SetActive(false);

    }

    void Update()
    {
        // Toggle flashlight on/off when 'F' key is pressed
        if (Input.GetKeyDown(KeyCode.F))
        {
            isOn = !isOn;
            flashlight.SetActive(isOn);

            if (isOn)
            {
                Debug.Log("Flashlight turned on.");
                turnOn.Play();
            }
            else
            {
                Debug.Log("Flashlight turned off.");
                turnOff.Play();
            }
        }

        // Perform raycast when flashlight is on
        if (isOn)
        {
            PerformRaycast();
        }
    }

    void PerformRaycast()
    {
        // Get the camera's position and forward direction
        Transform cameraTransform = Camera.main.transform;

        // Offset the origin to avoid self-collision
        Vector3 origin = cameraTransform.position + cameraTransform.forward * 1f;
        Vector3 direction = cameraTransform.forward;

        // Set the radius for the SphereCast
        float sphereRadius = 1.0f;  // Adjust the radius to suit your needs (e.g., 1.0f for larger coverage)

        RaycastHit hit;

        // Perform the SphereCast
        gizmoOrigin = origin;
        gizmoDirection = direction;

        // Visualize the SphereCast
        Debug.DrawRay(origin, direction * raycastRange, Color.yellow);

        Debug.Log("Performing SphereCast.");

        // SphereCast to hit larger areas
        if (Physics.SphereCast(origin, sphereRadius, direction, out hit, raycastRange, layerMask))
        {
            gizmoHit = true;
            gizmoHitInfo = hit;

            Debug.Log($"SphereCast hit: {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            // Try to get Monster4 component from the collider or its parents
            Monster4 monster = hit.collider.GetComponentInParent<Monster4>();
            if (monster != null)
            {
                // Call the Freeze method on Monster4
                monster.Freeze();
                Debug.Log("Monster4 is being frozen by the flashlight.");
            }
            else
            {
                Debug.Log("Hit object does not have Monster4 script in its hierarchy.");
            }
        }
        else
        {
            gizmoHit = false;
            Debug.Log("SphereCast did not hit anything.");
        }
    }



    // Draw the raycast in the Scene view for debugging
    private void OnDrawGizmos()
    {
        if (isOn)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(gizmoOrigin, gizmoDirection * raycastRange);

            if (gizmoHit)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(gizmoHitInfo.point, 0.2f);
            }
        }
    }
}
