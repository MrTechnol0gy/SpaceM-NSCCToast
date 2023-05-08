using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavMeshBaker : MonoBehaviour
{
    public NavMeshSurface[] navMeshes;

    void Start()
    {
        for (int i = 0; i < navMeshes.Length; i++) 
        {
            navMeshes[i].BuildNavMesh ();    
        }  
    }
}
