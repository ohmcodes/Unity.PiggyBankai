using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer audioMixer;
    public Slider musicSlider, fxSlider, masterSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadAudioSettings();
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("Music", ConvertToDecibels(volume));
        SaveAudioSettings();
    }

    public void SetFXVolume(float volume)
    {
        audioMixer.SetFloat("FX", ConvertToDecibels(volume));
        SaveAudioSettings();
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("Master", ConvertToDecibels(volume));
        SaveAudioSettings();
    }

    private void LoadAudioSettings()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            musicSlider.value = musicVolume;
            SetMusicVolume(musicVolume);
        }

        if (PlayerPrefs.HasKey("FXVolume"))
        {
            float fxVolume = PlayerPrefs.GetFloat("FXVolume");
            fxSlider.value = fxVolume;
            SetFXVolume(fxVolume);
        }

        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            float masterVolume = PlayerPrefs.GetFloat("MasterVolume");
            masterSlider.value = masterVolume;
            SetMasterVolume(masterVolume);
        }
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("FXVolume", fxSlider.value);
        PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
        PlayerPrefs.Save();
    }

    private float ConvertToDecibels(float volume)
    {
        return Mathf.Log10(volume) * 20f;
    }
}
