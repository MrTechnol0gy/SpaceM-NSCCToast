using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameTimer : MonoBehaviour
{   
    private static int currentGameTime;
    private float lastTimeCountedDownTime;
    TimeSpan time;
    private string formattedTime;

    public static GameTimer get;

    void Awake()
    {
        get = this;
    }

    void Start()
    {
        currentGameTime = GameManager.get.totalMissionTime * 60;    // converts totalMissionTime into seconds
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
            time = TimeSpan.FromSeconds(currentGameTime);
            // converts game time into a minutes/seconds format
            formattedTime = string.Format("{0:00} : {1:00} ",(int)time.TotalMinutes, time.Seconds);
            //Send the new time to the UIManager for display
            UIManager.SetTime(formattedTime);
            GameManager.get.GameOverTimeCheck(currentGameTime);
        }
    }

    public void DecreaseTime(int amount)
    {
        if (Player.get.IsShieldActive())
        {
            Player.get.shieldActive = false;
            //Debug.Log("Shield is inactive.");
            return;
        }
        else
        {
            currentGameTime -= amount;
        }
    }

// This method can be used to test if a certain time has elapsed since we registered an event time. 
public bool TimeElapsedSince(float timeEventHappened, float testingTimeElapsed) => !(timeEventHappened + testingTimeElapsed > Time.time);
   
}
