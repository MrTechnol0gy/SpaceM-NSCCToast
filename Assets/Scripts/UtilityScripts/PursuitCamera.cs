using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PursuitCamera : MonoBehaviour
{
    public GameObject player;
    private Camera virtualCamera;
    private List<GameObject> allEnemies;
    private bool isPursuing = false;
    // Start is called before the first frame update
    void Start()
    {
        virtualCamera = GetComponent<Camera>();
        allEnemies = EnemyManager.get.GetListOfAll();
    }

    // Update is called once per frame
    void Update()
    {
        isPursuing = false;
        foreach (GameObject enemy in allEnemies)
        {
            if (enemy.GetComponent<AIEnemyForce>().currentState == AIEnemyForce.States.attacking || enemy.GetComponent<AIEnemyForce>().currentState == AIEnemyForce.States.chasing)
            {
                isPursuing = true;
                break;
            }
        }

        if (isPursuing)
        {
            //Debug.Log("Pursuit camera is enabled.");
            virtualCamera.enabled = true;
        }
        else
        {
            //Debug.Log("Pursuit camera is disabled.");
            virtualCamera.enabled = false;
        }
    }
}
