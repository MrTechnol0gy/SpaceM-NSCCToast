using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetarySelection : MonoBehaviour
{
    public static PlanetarySelection get;
    public List<GameObject> prefabsList;
    public float rotationDuration = 2f;
    private int rotationAmount;
    private float anglePerRotation;
    private int currentSelection = 0;

    private void Awake()
    {
        get = this;
        rotationAmount = prefabsList.Count;
        anglePerRotation = 360f / rotationAmount;
        SetPlanetName();
    }

    private void SetPlanetName()
    {
        UILevelSelect.SetPlanetName(prefabsList[currentSelection].name);
    }
    private void ModifyCurrentSelectionUp()
    {
        currentSelection++;
        if (currentSelection >= prefabsList.Count)
        {
            currentSelection = 0;
        }
        SetPlanetName();
    }
    private void ModifyCurrentSelectionDown()
    {
        currentSelection--;
        if (currentSelection < 0)
        {
            currentSelection = prefabsList.Count - 1;
        }
        SetPlanetName();
    }

    public void Rotate()
    {
        Quaternion targetRotation = Quaternion.Euler(0, anglePerRotation, 0) * transform.rotation;
        StartCoroutine(RotateCoroutine(targetRotation, rotationDuration));
        ModifyCurrentSelectionUp();
    }

    public void ReverseRotate()
    {
        Quaternion targetRotation = Quaternion.Euler(0, -anglePerRotation, 0) * transform.rotation;
        StartCoroutine(RotateCoroutine(targetRotation, rotationDuration));
        ModifyCurrentSelectionDown();
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