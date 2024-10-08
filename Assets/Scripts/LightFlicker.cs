using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
   public Light lightSource;  // Drag and drop the light object here in the Inspector
    public float minIntensity = 0.5f;
    public float maxIntensity = 2f;
    public float flickerSpeed = 0.1f;

    void Update()
    {
        // Adjust the intensity randomly based on time and speed
        lightSource.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PerlinNoise(Time.time * flickerSpeed, 0f));
    }
}
