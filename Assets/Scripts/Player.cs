using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player get;
    public static PlayerFlightControl playerFlightControl;

    void Awake()
    {
        get = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        playerFlightControl = PlayerFlightControl.get;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
