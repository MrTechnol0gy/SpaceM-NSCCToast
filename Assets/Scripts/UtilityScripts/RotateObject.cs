using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotateSpeed = 1.0f; // Rotation speed set in inspector

    // Update is called once per frame
    void Update()
    {
        // Rotate the game object around the y-axis at the specified speed
        transform.Rotate(new Vector3(0, rotateSpeed, 0) * Time.deltaTime);
    }
}
