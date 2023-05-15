using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfViewScale : MonoBehaviour
{
    private Vector3 baseScale, endScale, newScale;
    private float detectionRange, detectionRangeUpdateSpeed;
    private float lerpTime = 0.0f;
    private Transform objectTransform;

    // Start is called before the first frame update
    void Start()
    {
        objectTransform = transform;
        detectionRange = AIEnemyForce.get.detectionRange;
        detectionRangeUpdateSpeed = AIEnemyForce.get.detectionRangeUpdateSpeed;
        baseScale = objectTransform.localScale;
        UpdateEndScale();
    }

    // Update is called once per frame
    void Update()
    {
        if (detectionRange != AIEnemyForce.get.detectionRange)
        {
            detectionRange = AIEnemyForce.get.detectionRange;
            UpdateEndScale();
        }

        VisualizeDetectionRange();
    }

    private void UpdateEndScale()
    {
        endScale = new Vector3(detectionRange, detectionRange, detectionRange);
    }

    private void VisualizeDetectionRange()
    {
        lerpTime += Time.deltaTime;
        lerpTime = Mathf.Clamp(lerpTime, 0.0f, detectionRangeUpdateSpeed); // Clamp the lerp to prevent overshooting
        float t = lerpTime / detectionRangeUpdateSpeed; // Calculate the interpolation time
        newScale = Vector3.Lerp(baseScale, endScale, t);
        // Apply the new scale to the object's transform
        objectTransform.localScale = newScale;

        // Check if the lerp is complete
        if (lerpTime >= detectionRangeUpdateSpeed)
        {
            lerpTime = 0.0f;
            baseScale = newScale;
        }
    }
}
