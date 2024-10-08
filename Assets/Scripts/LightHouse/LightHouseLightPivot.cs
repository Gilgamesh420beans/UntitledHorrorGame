using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightHouseLightPivot : MonoBehaviour
{
 public float rotationSpeed = 30.0f; // Adjust the rotation speed here

    void Update()
    {
        // Rotate the GameObject around its Y-axis (vertical axis)
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
