using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utilities;
using Utilities.Time;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private VolumeParameter[] volumeParameters;
    [SerializeField] private float singleIncrement = 1f;
    private int _currentParameterIndex = 0;

    [SerializeField] private AudioClip volumeControlFeedbackClip;
    [SerializeField] private AudioClip maxMinVolumeFeedbackClip;
    
    private readonly UnityEvent<AudioClip, float, TimersHandler.DelegateMethod> _play2DVoiceEvent = 
        new UnityEvent<AudioClip, float, TimersHandler.DelegateMethod>();

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var level = FindObjectOfType<Level>();
        if (!level)
            throw new Exception("Level not found!");
        _play2DVoiceEvent.AddListener(level.Play2DVoice); 
    }

    public void NextVolume()
    {
        _currentParameterIndex = (_currentParameterIndex + 1) % 3;
        _play2DVoiceEvent.Invoke(volumeParameters[_currentParameterIndex].volumeControlChoice, 500f, null);
    }
    
    public void PreviousVolume()
    {
        _currentParameterIndex = (_currentParameterIndex - 1) % 3;
        if (_currentParameterIndex < 0)
            _currentParameterIndex = 2;
        _play2DVoiceEvent.Invoke(volumeParameters[_currentParameterIndex].volumeControlChoice, 500f, null);
    }
    
    public void VolumeUp()
    {
        mixer.GetFloat(volumeParameters[_currentParameterIndex].name, out var currentVolume);
        if (currentVolume + singleIncrement > volumeParameters[_currentParameterIndex].maxVolume)
        {
            _play2DVoiceEvent.Invoke(maxMinVolumeFeedbackClip, 250f, null);
            return;
        }
        _play2DVoiceEvent.Invoke(volumeControlFeedbackClip, 250f, null);
        mixer.SetFloat(volumeParameters[_currentParameterIndex].name, currentVolume + singleIncrement);
    }
    
    public void VolumeDown()
    {
        mixer.GetFloat(volumeParameters[_currentParameterIndex].name, out var currentVolume);
        if (currentVolume - singleIncrement < volumeParameters[_currentParameterIndex].minVolume)
        {
            _play2DVoiceEvent.Invoke(maxMinVolumeFeedbackClip, 250f, null);
            return;
        }
        _play2DVoiceEvent.Invoke(volumeControlFeedbackClip, 250f, null);
        mixer.SetFloat(volumeParameters[_currentParameterIndex].name, currentVolume - singleIncrement);
    }

    void OnSceneUnloaded(Scene scene)
    {
        _play2DVoiceEvent.RemoveAllListeners();
    }
}

[Serializable]
struct VolumeParameter
{
    public string name;
    public int maxVolume;
    public int minVolume;
    public AudioClip volumeControlChoice;
}
