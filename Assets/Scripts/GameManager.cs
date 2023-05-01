using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager get;                          // singleton reference
    public LayerMask environmentLayerMask;                  //Layermask that holds the environment for raycasts

    [Header("Player Spawn")]
    [SerializeField] public GameObject spawnPoint;

    [Header("Mission Information")]
    [SerializeField] public int totalMissionTime = 20;                 // as minutes
    [SerializeField] public int totalIntelNeeded = 10;                 // intel to complete mission (placeholder for future expansion)
    [SerializeField] public int amountOfPickUps = 10;                  // number of pickups to be found in the level
    [SerializeField] public int amountOfEnemies = 10;                  // number of enemies to be found in the level
    [SerializeField] public float spawnRadius = 100f;                  // spawn radius (should be replaced to match level radius)

    void Awake()
    {
        get = this;
    }

    void Start()
    {
        UIManager.SetIntelTotalRequired(totalIntelNeeded);
    }
}
