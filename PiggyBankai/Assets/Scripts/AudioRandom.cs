using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioRandom : MonoBehaviour
{
    [System.Serializable]
    public class AudioEntry
    {
        public AudioClip clip;
        public bool useCustomVolume = false;
        [Range(0f, 1f)] public float customVolume = 1f;
    }

    [Header("Audio Settings")]
    [SerializeField] private AudioEntry[] audioClips;
    [SerializeField] private AudioSource audioSource;

    public enum TriggerMode { Manual, Automatic }
    [Header("Trigger Settings")]
    [SerializeField] private TriggerMode triggerMode = TriggerMode.Manual;
    [SerializeField] private float autoPlayInterval = 2f;

    public enum PlaybackMode { Sequential, Shuffle, Random }
    [Header("Playback Settings")]
    [SerializeField] private PlaybackMode playbackMode = PlaybackMode.Random;

    [Header("Volume & Pitch Randomization")]
    [SerializeField] private Vector2 volumeRange = new Vector2(0.5f, 1f);
    [SerializeField] private Vector2 pitchRange = new Vector2(0.8f, 1.2f);

    [Header("Debug Controls")]
    [SerializeField] private bool playOnStart = false;

    // Inspector button for testing
    [ContextMenu("Play Audio (Test)")]
    private void PlayAudioTest()
    {
        PlayAudio();
    }

    // Private variables
    private int currentIndex = 0;
    private List<int> shuffleList = new List<int>();
    private float nextAutoPlayTime = 0f;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        InitializeShuffleList();

        if (playOnStart)
        {
            PlayAudio();
        }

        if (triggerMode == TriggerMode.Automatic)
        {
            nextAutoPlayTime = Time.time + autoPlayInterval;
        }
    }

    void Update()
    {
        if (triggerMode == TriggerMode.Automatic && Time.time >= nextAutoPlayTime && audioClips.Length > 0)
        {
            PlayAudio();
            nextAutoPlayTime = Time.time + autoPlayInterval;
        }
    }

    public void PlayAudio()
    {
        if (audioClips.Length == 0) return;

        int selectedIndex = GetNextAudioIndex();
        PlayAudioAtIndex(selectedIndex);
    }

    public void PlayAudioAtIndex(int index)
    {
        if (index < 0 || index >= audioClips.Length || audioClips[index].clip == null) return;

        // Set audio clip
        audioSource.clip = audioClips[index].clip;

        // Set volume (use custom if enabled, otherwise randomize)
        float volume = audioClips[index].useCustomVolume 
            ? audioClips[index].customVolume 
            : Random.Range(volumeRange.x, volumeRange.y);
        audioSource.volume = volume;

        // Set pitch
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);

        // Play the audio
        audioSource.Play();

        Debug.Log($"Playing audio: {audioClips[index].clip.name} (Volume: {volume:F2}, Pitch: {audioSource.pitch:F2})");
    }

    private int GetNextAudioIndex()
    {
        switch (playbackMode)
        {
            case PlaybackMode.Sequential:
                currentIndex = (currentIndex + 1) % audioClips.Length;
                return currentIndex;

            case PlaybackMode.Shuffle:
                if (shuffleList.Count == 0)
                {
                    InitializeShuffleList();
                }
                int index = shuffleList[0];
                shuffleList.RemoveAt(0);
                return index;

            case PlaybackMode.Random:
            default:
                return Random.Range(0, audioClips.Length);
        }
    }

    private void InitializeShuffleList()
    {
        shuffleList.Clear();
        for (int i = 0; i < audioClips.Length; i++)
        {
            shuffleList.Add(i);
        }
        
        // Fisher-Yates shuffle
        for (int i = shuffleList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = shuffleList[i];
            shuffleList[i] = shuffleList[randomIndex];
            shuffleList[randomIndex] = temp;
        }
    }

    // Public methods for external control
    public void SetTriggerMode(TriggerMode mode)
    {
        triggerMode = mode;
        if (mode == TriggerMode.Automatic)
        {
            nextAutoPlayTime = Time.time + autoPlayInterval;
        }
    }

    public void SetPlaybackMode(PlaybackMode mode)
    {
        playbackMode = mode;
        if (mode == PlaybackMode.Shuffle)
        {
            InitializeShuffleList();
        }
    }

    public void SetVolumeRange(float min, float max)
    {
        volumeRange = new Vector2(min, max);
    }

    public void SetPitchRange(float min, float max)
    {
        pitchRange = new Vector2(min, max);
    }

    public void SetAutoPlayInterval(float interval)
    {
        autoPlayInterval = interval;
    }
}
