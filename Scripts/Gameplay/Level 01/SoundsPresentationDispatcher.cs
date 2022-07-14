using System;
using Managers;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Events;
using Utilities;
using Utilities.Time;

public class SoundsPresentationDispatcher : MonoBehaviour
{
    #region INSPECTOR_PARAMETERS

    [SerializeField] private string tableName;
    
    [Header("Presentation")]
    [SerializeField] private AudioClip supportAudioClip;
    [SerializeField] private AudioClip requestInputAudioClip;
    [SerializeField] private AudioClip tutorialSkipAudioClip;


    [SerializeField] [Range(2000, 5000)] private int repeatRequestTimer = 3000;
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] private AudioSource environmentAudioSource;
    [SerializeField] [Range(2, 15)] 
    [Tooltip("The amount of time the player can request to repeat the same " +
             "presentation before the voice helper starts to play the support audio")]
    private int attemptsLimit = 2;
    [SerializeField] private SingleSoundPresentation[] presentations; 

    [Header("Support development")] 
    [SerializeField] private bool doNotWaitInputRequest;
    [SerializeField] private int startingPresentationIdx;
    
    [Header("Event")]
    private readonly UnityEvent _startLevelEvent = new UnityEvent();
    
    public AudioClip TutorialSkipAudioClip => tutorialSkipAudioClip;

    #endregion
    
    #region PRIVATE
    
    //Instance references
    private StopwatchHandler _stopwatchHandler;
    private TimersHandler _timersHandler;
    
    //Support values
    private SingleSoundPresentation _currentPresentation;
    private int _currentPresentationIdx;
    private bool _inputFeedbackRequested;

    //Save data value
    private int _currentAttempts; //Attempts number for the current feedback

    private const string TIMER_NAME = "SoundDispatcher";
    
    #endregion

    #region DISPATCHER
    
    private void Awake()
    {
        if (doNotWaitInputRequest)
            _inputFeedbackRequested = true;

        _timersHandler = GameManager.Instance.timersHandler;
        _timersHandler.CreateTimer(TIMER_NAME);

        _stopwatchHandler = GameManager.Instance.stopwatchHandler;
        _stopwatchHandler.CreateStopwatch(TIMER_NAME);
        
        SetupEvents();
    }

    private void SetupEvents()
    {
        var level = FindObjectOfType<Level>();
        if (!level)
        {
            Debug.LogError("Level not found!");
            return;
        }

        _startLevelEvent.AddListener(level.PlayIntroAudioClip);
    }

    private void OnDestroy()
    {
        _startLevelEvent.RemoveAllListeners();
    }

    // private void Start()
    // {
    //     StartPresentations();
    // }

    //Start the dispatcher
    public void StartPresentations()
    {
        InputManager.Instance.EnableVolumeControlInput();
        InputManager.Instance.DisableStartGameInput();

        _currentPresentationIdx = startingPresentationIdx != 0 ? startingPresentationIdx : 0;
        _currentPresentation = presentations[_currentPresentationIdx];
        _currentPresentation.Dispatcher = this;

        SetupCurrentPresentation();
    }

    private void SetupCurrentPresentation()
    {
        //Create the timer
        _currentPresentation.SetupTimer("SoundPresentation_" + _currentPresentation.Name);
        _timersHandler.CreateTimer(_currentPresentation.TimerName);

        if (IsMovingPresentation())
            _timersHandler.CreateTimer(_currentPresentation.TimerName + "_Moving");

        StartCurrentPresentation();
    }

    //Start the current presentation
    public void StartCurrentPresentation()
    {
        _timersHandler.StopTimer(TIMER_NAME); //Timer is not used yet
        
        //Add an attempts; If it's the first one, it's set from 0 to 1;
        _currentAttempts++;

        //Active the object if it's a moving presentation
        if (IsMovingPresentation())
        {
            ActiveMovingPresentationGameObject(true);
            _currentPresentation.Sound3DAudioSource = 
                _currentPresentation.MovingTransform.GetComponentInChildren<AudioSource>();
        }

        //Set audio source(s) and play the voice sound
        if (_currentPresentation.HasEnvironmentAudioClip()) 
            _currentPresentation.SetAudioSources(voiceAudioSource, environmentAudioSource);
        else
            _currentPresentation.SetAudioSources(voiceAudioSource);
        _currentPresentation.PlayVoice();
    }
    
    public void WaitForNextPresentation()
    {
        _timersHandler.StopTimer(TIMER_NAME); //Stop the timer 
        
        EndCurrentPresentation();
        
        //Reset variables
        //inputFeedbackRequested = false; 
        _currentAttempts = 0;
        // _inputRequestAmount = 0;
        
        _timersHandler.SetTimer(TIMER_NAME, _currentPresentation.MSDelayToNextPresentation, 
                                            NextPresentation, true);
    }

    private void NextPresentation()
    {
        //Check if there is a presentation to dispatch
        if (++_currentPresentationIdx >= presentations.Length)
        {
            EndPresentationDispatch();
            return;
        }
        
        _currentPresentation = presentations[_currentPresentationIdx];
        _currentPresentation.Dispatcher = this;
        
        SetupCurrentPresentation();
    }

    private void EndPresentationDispatch()
    {
        _timersHandler.DeleteTimer(TIMER_NAME);
        _stopwatchHandler.DeleteStopwatch(TIMER_NAME);

        // Debug.Log("All sounds presented!");
        _startLevelEvent.Invoke();
        transform.parent.gameObject.SetActive(false);
    }

    private void EndCurrentPresentation()
    {
        StopCurrentPresentation();
        
        if (IsMovingPresentation())
            ActiveMovingPresentationGameObject(false);
        
        _timersHandler.DeleteTimer(_currentPresentation.TimerName);
        if (IsMovingPresentation())
            _timersHandler.DeleteTimer(_currentPresentation.TimerName + "_Moving");

        if (_currentPresentation.Sound3DAudioClip == null  || !_currentPresentation.RequireInput)
            return;

        SaveTotalAttemptsAmount();
    }

    [ContextMenu("Skip Presentation")]
    private void SkipCurrentPresentation()
    {
        EndCurrentPresentation();
        NextPresentation();
    }

    public void SkipPresentationDispatch()
    {
        EndCurrentPresentation();
        EndPresentationDispatch();
    }

    private void StopCurrentPresentation()
    {
        InputManager.Instance.DisableSoundsFeedbackInput(); //Input disabled

        //Stop voice if it's playing
        if (voiceAudioSource.isPlaying)
            voiceAudioSource.Stop();
    }

    public void RepeatCurrentPresentation()
    {
        StopCurrentPresentation();

        if (MaxAttemptsReached())
            return;
        
        if (IsMovingPresentation())
        {
            ResetMovingPresentation();
            return;
        }
        
        StartCurrentPresentation(); //Restart the same presentation
    }


    private void PlaySupportVoice()
    {
        InputManager.Instance.DisableSoundsFeedbackInput(); //Input disabled

        voiceAudioSource.clip = supportAudioClip;
        voiceAudioSource.Play();
        
        //If the presentation is Moving, it starts after its reset
        if (_currentPresentation.MovingTransform) 
            _timersHandler.SetTimer(TIMER_NAME, 500 + supportAudioClip.length * 1000, 
                                                ResetMovingPresentation, true);
        else //otherwise, restart the Fixed presentation
            _timersHandler.SetTimer(TIMER_NAME, 500 + supportAudioClip.length * 1000, 
                                            StartCurrentPresentation, true);
    }
    
    public void PauseDispatcher()
    {
        voiceAudioSource.Pause();
        environmentAudioSource.Pause();
        _currentPresentation?.PausePresentation();
        _timersHandler.PauseTimer(TIMER_NAME);
    }

    public void ResumeDispatcher()
    {
        voiceAudioSource.UnPause();
        environmentAudioSource.UnPause();
        _currentPresentation?.ResumePresentation();
        _timersHandler.ResumeTimer(TIMER_NAME);
    }
    
    #endregion
    
    #region INPUT
    
    public void RequestInput()
    {
        if(!_currentPresentation.RequireInput)
        {
            WaitForNextPresentation();
            return;
        }
        
        //Request the input with voice
        voiceAudioSource.clip = requestInputAudioClip;
        voiceAudioSource.Play();
        
        //Add a repetition of the input request to the counter;
        // _inputRequestAmount++;

        //If it's not the first input request
        if (_inputFeedbackRequested)
        {
            //then set a timer to repeat the request
            _timersHandler.SetTimer(TIMER_NAME, repeatRequestTimer + requestInputAudioClip.length * 1000, 
                                            RequestInput, true);
            // _timersHandler.DebugTimer(TIMER_NAME);
            WaitForInput(); //and enable input
            return;
        }
        
        //otherwise enable input after this timer
        _timersHandler.SetTimer(TIMER_NAME, requestInputAudioClip.length * 1000, 
                                            WaitForInput, true);
    }

    private void WaitForInput()
    {
        //Enable feedback input
        InputManager.Instance.EnableSoundsFeedbackInput();

        //Start the stopwatch to count the time needed to make a decision
        _stopwatchHandler.ResumeTimer(TIMER_NAME);
        
        //If the input is already been requested one time exit
        if (_inputFeedbackRequested) return;
        
        //otherwise set a timer to request the input for the first time
        _timersHandler.SetTimer(TIMER_NAME, repeatRequestTimer, RequestInput, true);
        _inputFeedbackRequested = true;
    }
    
    #endregion
    
    #region SUPPORT_METHODS
    private bool MaxAttemptsReached()
    {
        if (_currentAttempts % attemptsLimit > 0) return false;
        
        PlaySupportVoice();
        return true;
    }
    
    public void StartDelay()
    {
        _currentPresentation.WaitFor3DSound();
    }

    public void Start3DSound()
    {
        if (!_currentPresentation.Sound3DAudioClip)
        {
            RequestInput();
            return;
        }
        _currentPresentation.PlaySound();
    }

    #endregion
    
    #region MOVING_PRESENTATION
    private bool IsMovingPresentation()
    {
        return _currentPresentation.MovingTransform;
    }

    private void ResetMovingPresentation()
    {
        _currentPresentation.MovingTransform.Reset();
    }

    private void ActiveMovingPresentationGameObject(bool active)
    {
        _currentPresentation.MovingTransform.gameObject.SetActive(active);
    }
    #endregion
    
    #region SAVE_DATA

    //Save on the json file the total attempts amount to the next presentation
    private void SaveTotalAttemptsAmount()
    {
        SoundPresentationData soundPresentationData = 
            new SoundPresentationData(_currentPresentation.Name, _currentAttempts);
        GameManager.Instance.saveDataManager.AddPresentation(soundPresentationData);
    }
    
    #endregion
}