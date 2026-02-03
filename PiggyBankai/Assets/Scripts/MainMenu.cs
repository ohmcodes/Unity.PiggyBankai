using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button AudioOnButton;
    [SerializeField] private Button AudioOffButton;
    void Start()
    {
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

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
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
