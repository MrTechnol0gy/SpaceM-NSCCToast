using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [Header("GameTime")]
    [SerializeField] int totalMissionTime;
    private static int currentGameTime;
    private float lastTimeCountedDownTime;

    public static GameTimer get;

    void Awake()
    {
        get = this;
    }
    void Update()
    {
        //If one second has elapsed since the last time we counted down the game time.
        if (TimeElapsedSince(lastTimeCountedDownTime, 1)) 
        {
            //Count down the game time.
            currentGameTime--;
            //Register the last time we counted down time for the next time elapsed loop.
            lastTimeCountedDownTime = Time.time;
        }
    }

// This method can be used to test if a certain time has elapsed since we registered an event time. 
public bool TimeElapsedSince(float timeEventHappened, float testingTimeElapsed) => !(timeEventHappened + testingTimeElapsed > Time.time);
   
}
