using UnityEngine;

public class ItemGreenKey : Item
{
   public override void OnPickUp()
    {
        // If we have deault mehtod, base allows use to reuse rather than rewrite
        // base.OnPickUp();
        // Call ManagerItem instance & add key
        ManagerItem.Instance.AddItem("GreenKey");

    }
}
