using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    public static Thruster get;
    public ParticleSystem thruster;

    void Awake()
    {
        get = this;
    }
    public void StartThrust()
    {
        if (thruster != null)
        {
            //Debug.Log("Thruster is on.");
            thruster.Play();
        }
    }
    public void StopThrust()
    {
        if (thruster != null)
        {
            thruster.Stop();
        }
    }
}
