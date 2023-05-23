using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaHot : MonoBehaviour
{
    [SerializeField] int lavaDamage = 20;    
    [SerializeField] float comparisonInterval = 3f;   // Comparison interval in seconds
    [SerializeField] float comparisonRange = 2f;         // distance above or below the lava the player will take damage from
    private float timer = 0f;
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
    }
    private void Update()
    {
        timer += Time.deltaTime;

        // Check if the comparison interval has elapsed
        if (timer >= comparisonInterval)
        {
            timer = 0f; // Reset the timer

            // Compare the Y positions
            float y1 = transform.position.y;
            float y2 = player.transform.position.y;

            if (Mathf.Abs(y1 - y2) <= comparisonRange)
            {
                //Debug.Log("Y positions are approximately equal.");
                GameTimer.get.DecreaseTime(lavaDamage);
            }
        }
    }
}
