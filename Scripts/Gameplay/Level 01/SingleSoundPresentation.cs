using System;
using UnityEngine;
using Utilities;
using Utilities.Time;

[Serializable]
public class SingleSoundPresentation
{
    [Header("Sound Presentation")]
    [SerializeField] private string name;
    [SerializeField] private SoundPresentationParametersSO parametersSo;
    
    [Header("Scene References")]
    [SerializeField] private AudioSource sound3DAudioSource;

    [Space]
    
    [Header("Optional Scene References")]
    [SerializeField] private AudioSource alternativeVoiceAudioSource;
    [SerializeField] private MovingSoundPresentation movingTransform;
    
    //Private not in inspector
    private AudioSource _standardVoiceAudioSource;
    private AudioSource _currentVoiceAudioSource;
    private AudioSource _environmentAudioSource;

    private TimersHandler _timersHandler;

    private SoundsPresentationDispatcher _dispatcher;
    
    #region Getters & Setters

    public AudioSource Sound3DAudioSource
    {
        set => sound3DAudioSource = value;
    }

    public SoundsPresentationDispatcher Dispatcher
    {
        get => _dispatcher;
        set => _dispatcher = value;
    }
    
    //Getters
    public string Name => name;
    public AudioClip Sound3DAudioClip => parametersSo.Sound3DAudioClip;
    public bool RequireInput => parametersSo.RequestForInput;
    public MovingSoundPresentation MovingTransform => movingTransform;
    public int MSDelayToNextPresentation => parametersSo.MSDelayToNextPresentation;

    public string TimerName { get; private set; }

    #endregion

    public void SetupTimer(string timerName)
    {
        TimerName = timerName;
        _timersHandler = GameManager.Instance.timersHandler;
    }
    
    public void SetAudioSources(AudioSource standardVoiceAudioSource)
    {
        _standardVoiceAudioSource = standardVoiceAudioSource;
        _currentVoiceAudioSource = standardVoiceAudioSource;
    }
    
    public void SetAudioSources(AudioSource standardVoiceAudioSource, AudioSource environmentAudioSource)
    {
        SetAudioSources(standardVoiceAudioSource);
        _environmentAudioSource = environmentAudioSource;
    }

    public void PlayVoice()
    {
        _timersHandler.SetTimer(TimerName, parametersSo.VoiceAudioClip.length * 1000, 
                                        _dispatcher.StartDelay, true);
        _currentVoiceAudioSource = alternativeVoiceAudioSource ? alternativeVoiceAudioSource : _standardVoiceAudioSource;
        _currentVoiceAudioSource.clip = parametersSo.VoiceAudioClip;
        _currentVoiceAudioSource.Play();
    }

    public void WaitFor3DSound()
    {
        _timersHandler.SetTimer(TimerName, parametersSo.MSDelayBetweenVoiceAnd3DSound, 
                                        _dispatcher.Start3DSound, true);
    }

    public void PlaySound()
    {
        if (_environmentAudioSource)
            PlayEnvironmentSound();
        
        if (movingTransform)
        {
            movingTransform.StartMoving(this);
            return;
        }

        //Fixed presentation
        _timersHandler.SetTimer(TimerName, parametersSo.Sound3DAudioClip.length * 1000, 
                                        EndPresentation, true);
        Play3DSoundOneShot();
    }
    
    public void Play3DSoundOneShot()
    {
        sound3DAudioSource.clip = parametersSo.Sound3DAudioClip;
        sound3DAudioSource.Play();
    }

    private void PlayEnvironmentSound()
    {
        if(!parametersSo.EnvironmentAudioClip)
        {
            Debug.LogError("Environment clip is not set up!");
            return;
        }
        
        _environmentAudioSource.clip = parametersSo.EnvironmentAudioClip;
        _environmentAudioSource.loop = true;
        _environmentAudioSource.Play();
    }

    public void PausePresentation()
    {
        if(_environmentAudioSource) _environmentAudioSource.Pause();
        if (sound3DAudioSource) sound3DAudioSource.Pause();
        _timersHandler.PauseTimer(TimerName);
    }

    public void ResumePresentation()
    {
        if(_environmentAudioSource) _environmentAudioSource.UnPause();
        if (sound3DAudioSource) sound3DAudioSource.UnPause();
        _timersHandler.ResumeTimer(TimerName);
    }

    public void EndPresentation()
    {
        Stop3DSound();

        if (parametersSo.EnvironmentAudioClip)
            StopEnvironmentSound();
        _timersHandler.SetTimer(TimerName, parametersSo.MSDelayToRequestInput, 
                                        RequestInput, true);
    }

    private void RequestInput()
    {
        _timersHandler.StopTimer(TimerName);
        _dispatcher.RequestInput();
    }

    private void Stop3DSound()
    {
        _timersHandler.StopTimer(TimerName);
        sound3DAudioSource.clip = null;
        sound3DAudioSource.Stop();
    }

    private void StopEnvironmentSound()
    {
        _environmentAudioSource.clip = null;
        _environmentAudioSource.loop = false;
        _environmentAudioSource.Stop();
    }

    public bool HasEnvironmentAudioClip()
    {
        return parametersSo.EnvironmentAudioClip;
    }
}