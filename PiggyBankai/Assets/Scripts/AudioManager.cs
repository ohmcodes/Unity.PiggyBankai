using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private Button AudioOnButton;
    [SerializeField] private Button AudioOffButton;

    public static AudioManager Instance;
    public AudioMixer audioMixer;
    public Slider musicSlider, fxSlider, masterSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadAudioSettings();

        int muted = PlayerPrefs.GetInt("Muted", 1); // Default to 1 (audio on)
        AudioListener.volume = muted;
        if (muted == 0)
        {
            AudioOnButton.gameObject.SetActive(false);
            AudioOffButton.gameObject.SetActive(true);
        }
        else
        {
            AudioOnButton.gameObject.SetActive(true);
            AudioOffButton.gameObject.SetActive(false);
        }
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

    public void AudioOn()
    {
        AudioListener.volume = 1f;

        PlayerPrefs.SetInt("Muted", 1);
        PlayerPrefs.Save();
    }

    public void AudioOff()
    {
        AudioListener.volume = 0f;

        PlayerPrefs.SetInt("Muted", 0);
        PlayerPrefs.Save();
    }
}
