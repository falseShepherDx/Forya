using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private GameObject escCanvas;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private TextMeshProUGUI gameNameText;
    
    private bool isOpen = false;
    private void Awake()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadMusicVolume();
        }
        else
        {
            SetMusicVolume();
        }

        if (PlayerPrefs.HasKey("soundVolume"))
        {
            LoadSFXVolume();
        }
        else
        {
            SetSFXVolume();
        }

        SetGameNameText();
        escCanvas.SetActive(false);
        isOpen = false;
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isOpen = !isOpen;
            escCanvas.SetActive(isOpen);
        }
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        audioMixer.SetFloat("Music",Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("musicVolume",volume);
    }

    public void SetSFXVolume()
    {
        float volume = soundSlider.value;
        audioMixer.SetFloat("SFX",Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("sfxVolume",volume);
    }

    private void LoadMusicVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        SetMusicVolume();
    }

    private void LoadSFXVolume()
    {
        soundSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        SetSFXVolume();
    }

    private void SetGameNameText()
    {
        gameNameText.text = SceneManager.GetActiveScene().name.ToString();
    }
    
}
