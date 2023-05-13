using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager get;                          // singleton reference
    public LayerMask environmentLayerMask;                  // Layermask that holds the environment for raycasts

    [Header("Player Spawn")]
    [SerializeField] public GameObject spawnPoint;

    [Header("Mission Information")]
    [SerializeField] public int totalMissionTime = 20;                 // as minutes
    [SerializeField] public int amountOfPickUps = 10;                  // number of pickups to be found in the level
    [SerializeField] public int amountOfEnemies = 10;                  // number of enemies to be found in the level
    [SerializeField] public int amountOfEnvironmentalDangers = 10;     // number of elemental dangers to be found in the level
    [SerializeField] public float spawnRadius = 100f;                  // spawn radius (should be replaced to match level radius)

    // private variables
    private int intelCollected = 0;                                    // the amount of intel pickups collected
    private int totalIntelCollectedOverTime = 0;                        // the total amount of intel over multiple missions   
    void Awake()
    {
        get = this;
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Workshop")
        {
            StartCoroutine(InitializeLevel());
        }
        else if (SceneManager.GetActiveScene().name == "Level Select")
        {
            UILevelSelect.SetProgressBar(totalIntelCollectedOverTime);
        }
    }

    // InitializeLevel ensures certain methods happen in a certain order to prevent issues during load time
    IEnumerator InitializeLevel()
    {
        UIManager.get.ShowLoadScreen();
        yield return new WaitForEndOfFrame();   // wait for the loading screen to show before starting heavy load items
        
        UIManager.SetLoadText("Generating Terrain");
        yield return new WaitForEndOfFrame();
        RandomTerrainGenerator.get.GenerateTerrain();
        yield return new WaitForEndOfFrame(); // wait for terrain generation to complete

        // UIManager.SetLoadText("Baking Meshes.");
        // yield return new WaitForEndOfFrame();
        // NavMeshBaker.get.BakeNavMeshes();
        // yield return new WaitForEndOfFrame(); // wait for navmesh baking to complete
    
        UIManager.SetLoadText("Dropping Pickups.");
        yield return new WaitForEndOfFrame();
        PickUpManager.get.SpawnPickups();
        yield return new WaitForEndOfFrame();
        
        UIManager.SetLoadText("Enemies approaching.");
        yield return new WaitForEndOfFrame();
        EnemyManager.get.SpawnAgents();
        yield return new WaitForEndOfFrame();
        
        UIManager.SetLoadText("Throwing in some fire tornados, as a treat.");
        yield return new WaitForEndOfFrame();
        EnemyManager.get.SpawnEnvironmentalDangers();
        yield return new WaitForEndOfFrame(); // wait for spawns to complete
        
        UIManager.get.HideLoadScreen();
        yield return new WaitForEndOfFrame();

        UIManager.get.ShowOverlayScreen();
        UIManager.SetIntelCurrentAmount(intelCollected);
    }

    public void CollectIntel()
    {
        intelCollected++;
        UIManager.SetIntelCurrentAmount(intelCollected);
    }
    public void GameOverTimeCheck(int timegiven)
    {
        if (timegiven <= 0)
        {
            UIManager.get.HideOverlayScreen();
            UIManager.get.ShowWinLoseScreen();
        }
    }

    public void StartTime()
    {
        Time.timeScale = 1f;
    }

    public void StopTime()
    {
        Time.timeScale = 0f;
    }

    public void UpdateTotalIntelOverTime()
    {
        totalIntelCollectedOverTime =+ intelCollected;
        intelCollected = 0;
    }
}
