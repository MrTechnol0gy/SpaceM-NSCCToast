using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfViewScale : MonoBehaviour
{
    private Vector3 baseScale, endScale, newScale;
    private float detectionRange, detectionRangeUpdateSpeed;
    private Transform objectTransform;
    private GameObject enemy;
    AIEnemyForce aIEnemyForce;

    // Start is called before the first frame update
    void Start()
    { 
        objectTransform = transform;
        aIEnemyForce = GetComponentInParent<AIEnemyForce>();
        detectionRange = aIEnemyForce.ProbeDetectionRange();
        detectionRangeUpdateSpeed = aIEnemyForce.ProbeDetectionRangeUpdateSpeed();
        baseScale = objectTransform.localScale;
        UpdateEndScale();
    }

    // Update is called once per frame
    void Update()
    {
        if (detectionRange != aIEnemyForce.ProbeDetectionRange())
        {
            detectionRange = aIEnemyForce.ProbeDetectionRange();
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
        if (Vector3.Distance(endScale, transform.localScale) > 0.05f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, endScale, Time.deltaTime * detectionRangeUpdateSpeed);
        }
    }
}
