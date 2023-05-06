using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnAxis : MonoBehaviour
{
    public float rotationSpeed = 50f;
    public bool rotateOnX = false;
    public bool rotateOnY = true;
    public bool rotateOnZ = false;

    // Update is called once per frame
    void Update()
    {
        if (rotateOnX)
            transform.Rotate(Time.deltaTime * rotationSpeed, 0f, 0f);
        if (rotateOnY)
            transform.Rotate(0f, Time.deltaTime * rotationSpeed, 0f);
        if (rotateOnZ)
            transform.Rotate(0f, 0f, Time.deltaTime * rotationSpeed);
    }
}
