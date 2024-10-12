using UnityEngine;

public class ItemKey : Item
{
    public AudioClip pickupClip;

    protected override void Awake()
    {
        Debug.Log("Tag in Awake: " + gameObject.tag);  // Check the tag at Awake
        if (audioSource != null && pickupClip != null)
        {
            audioSource.PlayOneShot(pickupClip);
        }
        
        //gameObject.tag = "UFOKey";  // Force the tag in Awake
        //Debug.Log("Forced tag in Awake: " + gameObject.tag);  // Check if the forced tag works
    }

    public override void OnPickUp()
    {
        Debug.Log("OnPickUp called on " + gameObject.name);
        // Play the pickup sound
        if (audioSource != null && pickupClip != null)
        {
            audioSource.PlayOneShot(pickupClip);
        }

        string itemTag = gameObject.tag;

        Debug.Log("Object picked up with tag: " + itemTag);
        // Use the tag of the object instead of a hardcoded "Key"
        ManagerItem.Instance.AddItem(gameObject.tag);
        
        // Use the tag of the object to determine which key was picked up
        switch (itemTag)
        {
            case "RedKey":
                GameManager.Instance.CollectRedKey();
                break;
            case "YellowKey":
                GameManager.Instance.CollectYellowKey();
                break;
            case "BlueKey":
                GameManager.Instance.CollectBlueKey();
                break;
            case "UFOKey":
                GameManager.Instance.CollectUFOKey();
                break;
            default:
                Debug.LogWarning("Picked up an item with an unknown tag: " + itemTag);
                break;
        }
        // Destroy the game object after pickup
        Destroy(gameObject);
    }
}
