using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager get;

    [Header("Game Audio")]
    [SerializeField] AudioClip mainTheme;
    [SerializeField] AudioClip gameBGM;
    [SerializeField] AudioClip buttonClick;
    [SerializeField] AudioClip tractorBeam;
    [SerializeField] AudioClip cloakActive;
    [SerializeField] AudioClip distractionProbe;
    public AudioSource audioSourceMain;
    public AudioSource audioSourceSecondary;

    // Start is called before the first frame update
    void Start()
    {
        get = this;

        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            audioSourceMain.clip = mainTheme;
            audioSourceMain.Play();
        }
        else if (SceneManager.GetActiveScene().name == "Workshop")
        {
            audioSourceMain.clip = gameBGM;
            audioSourceMain.Play();
        }
        else if (SceneManager.GetActiveScene().name == "Level Select")
        {
            audioSourceMain.clip = mainTheme;
            audioSourceMain.Play();
        }
        else if (SceneManager.GetActiveScene().name == "Victory")
        {
            audioSourceMain.clip = mainTheme;
            audioSourceMain.Play();
        }
    }
    
    public void PlayClick()
    {
        audioSourceSecondary.clip = buttonClick;
        audioSourceSecondary.volume = 1.0f;
        audioSourceSecondary.Play();
    }
    public void PlayTractorBeam()
    {
        audioSourceSecondary.clip = tractorBeam;
        audioSourceSecondary.volume = 0.8f;
        audioSourceSecondary.Play();
    }
    public void PlayCloakActive()
    {
        audioSourceSecondary.clip = cloakActive;
        audioSourceSecondary.volume = 0.5f;
        audioSourceSecondary.Play();
    }
    public void PlayDistractionProbe()
    {
        audioSourceSecondary.clip = distractionProbe;
        audioSourceSecondary.volume = 0.8f;
        audioSourceSecondary.Play();
    }
}
