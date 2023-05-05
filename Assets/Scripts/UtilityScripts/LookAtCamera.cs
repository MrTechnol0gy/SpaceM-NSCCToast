using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] Camera sceneCamera;
    [SerializeField] bool lockX, lockY, lockZ;
    // Start is called before the first frame update
    void Start()
    {
       sceneCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraPosition = sceneCamera.transform.position;

        // Lock the rotation on the specified axis
        if (lockX) cameraPosition.x = transform.position.x;
        if (lockY) cameraPosition.y = transform.position.y;
        if (lockZ) cameraPosition.z = transform.position.z;
        
        transform.LookAt(cameraPosition);
    }
}
