using UnityEngine;

public class Compass : MonoBehaviour
{
    public Transform player;
    Vector3 dir;
    void Update()
    {
        // Need to add 180 here as back to front where player starts
        dir.z = player.eulerAngles.y + 180f;
        transform.localEulerAngles = dir;
    }
}
