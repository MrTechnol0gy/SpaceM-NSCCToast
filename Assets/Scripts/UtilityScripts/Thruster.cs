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
        thruster.Play();
    }
    public void StartThrust()
    {
        if (thruster != null)
        {
            //Debug.Log("Thruster is on.");
            var em = thruster.emission;
            em.enabled = true;
           // thruster.Play();
        }
    }
    public void StopThrust()
    {
        if (thruster != null)
        {
            var em = thruster.emission;
            em.enabled = false;
            //thruster.Stop();
        }
    }
}
