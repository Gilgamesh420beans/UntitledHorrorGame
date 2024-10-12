using UnityEngine;

public class Item : MonoBehaviour
{
    private Vector3 rotation;
    private Vector3 startPosition; // To store the initial position for bobbing
    public AudioSource audioSource;

    // Reference to determine if the item should follow the monster
    public bool isFollowingMonster = false;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        Debug.Log("Tag at Start: " + gameObject.tag);
        // Store the initial position to use it as a reference for bobbing
        startPosition = transform.position;
    }

    void Update()
    {
        // If the item is following the monster, disable bobbing
        if (!isFollowingMonster)
        {
            // Bobbing logic (only if not following the monster)
            float bobbingNewY = Mathf.Sin(Time.time * 2f) * 0.2f + startPosition.y;  // Renamed newY to bobbingNewY
            transform.position = new Vector3(startPosition.x, bobbingNewY, startPosition.z);
        }

        // Rotation logic (can still rotate while following)
        rotation = new Vector3(0, 30, 0);
        transform.Rotate(rotation * Time.deltaTime);

        // Debug to ensure the key is not untagged
        if (gameObject.tag == "Untagged")
        {
            Debug.LogWarning($"{gameObject.name} became Untagged unexpectedly");
        }
    }

    public virtual void OnPickUp()
    {
        // To be overridden 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player collided with: " + gameObject.name);
            OnPickUp();
            // Destroy handled in subclasses
        }
    }
}
