using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public GameObject flashlight;

    public AudioSource turnOn;
    public AudioSource turnOff;

    public bool on;
    public bool off;

    void Start()
    {
        
        off = true;
        flashlight.SetActive(false);
    }

    void Update()
    {
        if(off && Input.GetButtonDown("f"))
        {
            Debug.Log("F key was pressed.");
            flashlight.SetActive(true);
            turnOn.Play();
            off = false;
            on = true;
        }
        else if (on && Input.GetButtonDown("f"))
        {
            flashlight.SetActive(false);
            turnOff.Play();
            off = true;
            on = false;
        }

    }
}
