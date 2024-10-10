using UnityEngine;

public class ItemKey : Item
{
    public AudioClip pickupClip;

    protected override void Awake() {
        if (audioSource != null && pickupClip != null){
            audioSource.PlayOneShot(pickupClip);
        }
    }

     public override void OnPickUp()
    {
        // Play the pickup sound
        if (audioSource != null && pickupClip != null)
        {
            audioSource.PlayOneShot(pickupClip);
        }

        // Add the key to the manager
        ManagerItem.Instance.AddItem("Key");
        
        Destroy(gameObject);
    }

}
