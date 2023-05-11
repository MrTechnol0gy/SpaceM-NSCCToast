using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetarySelection : MonoBehaviour
{
    public static PlanetarySelection get;
    public int rotationAmount = 4;
    public float rotationDuration = 2f;

    private float anglePerRotation;

    private void Awake()
    {
        get = this;
        anglePerRotation = 360f / rotationAmount;
    }

    public void Rotate()
    {
        Quaternion targetRotation = Quaternion.Euler(0, anglePerRotation, 0) * transform.rotation;
        StartCoroutine(RotateCoroutine(targetRotation, rotationDuration));
    }

    public void ReverseRotate()
    {
        Quaternion targetRotation = Quaternion.Euler(0, -anglePerRotation, 0) * transform.rotation;
        StartCoroutine(RotateCoroutine(targetRotation, rotationDuration));
    }

    private IEnumerator RotateCoroutine(Quaternion targetRotation, float duration)
    {
        float timeElapsed = 0f;
        Quaternion initialRotation = transform.rotation;

        while (timeElapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}