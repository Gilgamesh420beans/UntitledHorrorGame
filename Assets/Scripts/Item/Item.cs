using Unity.VisualScripting;
using UnityEngine;

public class Item : MonoBehaviour
{

    private Vector3 rotation;
    private Vector3 startPosition; // To store the initial position for bobbing
    public AudioSource audioSource;

    protected virtual void Awake() {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null){
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // protected virtual void Awake() {
    //     audioSource = GetComponent<AudioSource>();
    //     if (audioSource == null){
    //         audioSource = gameObject.AddComponent<AudioSource>();
    //     }
    // }

    // public virtual void PickUpSound(){
    //     if (audioSource != null && audioSource.clip != null){
    //         audioSource.Play();
    //     }
    // }
    

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
            // Destroy handled in subclasses
        }
    }
}
