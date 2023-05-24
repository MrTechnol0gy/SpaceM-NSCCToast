using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FieldOfViewScale : MonoBehaviour
{
    private float endScale;
    private Vector3 newScale;
    private float detectionRange, detectionRangeUpdateSpeed;
    private Transform objectTransform;
    private GameObject enemy;
    AIEnemyForce aIEnemyForce;
    public ArcMeshGenerator arcMeshGen;

    // Start is called before the first frame update
    void Start()
    { 
        objectTransform = transform;
        aIEnemyForce = GetComponentInParent<AIEnemyForce>();
        detectionRange = aIEnemyForce.ProbeDetectionRange();
        detectionRangeUpdateSpeed = aIEnemyForce.ProbeDetectionRangeUpdateSpeed();
        UpdateEndScale();
        arcMeshMaterial = arcMeshGen.meshRenderer.material;
        originalColorArcMesh = arcMeshMaterial.color;
        angryColor.a = originalColorArcMesh.a;
    }

    public Color angryColor;
    private Color originalColorArcMesh;
    private Material arcMeshMaterial;
    private bool lastWasChasing = false;
    // Update is called once per frame
    void Update()
    {
        bool isChasingOrAttacking = (aIEnemyForce.currentState == AIEnemyForce.States.chasing || aIEnemyForce.currentState == AIEnemyForce.States.attacking);
        if (lastWasChasing != isChasingOrAttacking)
        {
            arcMeshMaterial.DOKill();
            arcMeshMaterial.DOColor(isChasingOrAttacking ? angryColor : originalColorArcMesh, 1).SetEase(Ease.InOutQuad);
            lastWasChasing = isChasingOrAttacking;
        }
        
        if (detectionRange != aIEnemyForce.ProbeDetectionRange())
        {
            detectionRange = aIEnemyForce.ProbeDetectionRange();
            UpdateEndScale();
        }

        VisualizeDetectionRange();
    }

    private void UpdateEndScale() {
        endScale = detectionRange;
    }

    private void VisualizeDetectionRange()
    {
        if ((Mathf.Abs(endScale - arcMeshGen.radius)) > 0.05f)
        {
            arcMeshGen.radius = Mathf.Lerp(arcMeshGen.radius,endScale,Time.deltaTime *detectionRangeUpdateSpeed);
            //transform.localScale = Vector3.Lerp(transform.localScale, endScale, Time.deltaTime * detectionRangeUpdateSpeed);
        }
    }
}
