using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepsAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] audioRandom;
    [SerializeField] private AudioSource audioSource;

    void Awake()
    {
        // Find AudioSource component
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            // If no AudioSource found, add one
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("AudioSource component added to " + gameObject.name);
            }
        }
    }

    // Method to play random footstep sound
    public void PlayRandomFootstep()
    {
        if (audioRandom.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, audioRandom.Length);
            audioSource.PlayOneShot(audioRandom[randomIndex]);
        }
    }

    public void EnableFootsteps()
    {
        audioSource.enabled = true;
        int randomIndex = Random.Range(0, audioRandom.Length);
        audioSource.clip = audioRandom[randomIndex];
        audioSource.Play();
    }

    public void DisableFootsteps()
    {
        audioSource.enabled = false;
    }
}
