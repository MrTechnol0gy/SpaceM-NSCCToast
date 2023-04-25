using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Player Spawn")]
    [SerializeField] public GameObject spawnPoint;




    public Component gameTimer;
    public static Player player;
    public static Component uiManager;

    // Start is called before the first frame update
    void Start()
    {
        player = Player.get;
        gameTimer = GameTimer.get; // practice using statics and gets instead of using references
        uiManager = UIManager.get;
    }

}
