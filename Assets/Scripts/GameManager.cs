using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Player Spawn")]
    [SerializeField] public GameObject spawnPoint;




    public Component gameTimer;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        gameTimer = GetComponent<GameTimer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
