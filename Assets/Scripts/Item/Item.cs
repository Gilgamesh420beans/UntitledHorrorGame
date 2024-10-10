using UnityEngine;

public class Item : MonoBehaviour
{

    private Vector3 rotation;
    private Vector3 startPosition; // To store the initial position for bobbing
    

    void Start()
    {
        // Store the initial position to use it as a reference for bobbing
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotation logic
        rotation = new Vector3(0, 30, 0);
        transform.Rotate(rotation * Time.deltaTime);

        // Bobbing logic
        // Calculate the new Y position relative to the initial position
        // Pos 2 = height
        // Pos 1 = speed
        float newY = Mathf.Sin(Time.time * 2f) * 0.2f + startPosition.y;

        // Set the object's position with the new calculated Y position
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }



  public virtual void OnPickUp()
    {
        // To be overidden 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPickUp();
            Destroy(gameObject); // Remove the item from the scene
        }
    }
}
