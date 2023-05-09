using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavMeshBaker : MonoBehaviour
{
    public static NavMeshBaker get;
    public NavMeshSurface[] navMeshes;

    void Awake()
    {
       get = this;
    }

    public void BakeNavMeshes()
    {
        for (int i = 0; i < navMeshes.Length; i++) 
        {
            navMeshes[i].BuildNavMesh ();    
        }  
    }
}
