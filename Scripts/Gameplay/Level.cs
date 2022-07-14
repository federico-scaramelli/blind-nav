using System;
using System.ComponentModel;
using Managers;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Time;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Level : MonoBehaviour
{
    #region =========INSPECTOR_PARAMETERS=========

    [SerializeField] private string levelName;
    [SerializeField] private bool automaticallyStart;

    [Header("References")]
    [SerializeField] private AudioSource voice2DAudioSource;
    [SerializeField] private AudioSource voice3DAudioSource;
    [SerializeField] private AudioSource environment2DAudioSource;

    [Header("Target")] 
    [SerializeField] private AudioClip introAudioClip;
    [SerializeField] private Transform startingPoint;
    [SerializeField] public Target[] targets;
    [SerializeField] private AudioClip[] audioClipsToFollow;
    [SerializeField] private int msLoopTime;
    [SerializeField] private int msLoopRandomizeFactor;

    [Header("Support - Timing")] 
    [SerializeField] private int sToTriggerSupport;
    [Tooltip("Start when support is activated.")]
    [SerializeField] private int sToTriggerReset; 
    [SerializeField] private int sToTriggerWrongDirection;
    [SerializeField] private int sToTriggerManualResetVoice;
    [Tooltip("The actual time that should elapse after the audio being played ends.")]
    [SerializeField] private int sToRepeatManualResetVoice;
    
    [Header("Support - Audio Clips")]
    [SerializeField] private AudioClip[] audioClipsToSupport;
    [SerializeField] private AudioClip[] audioClipsWrongDirection;
    [SerializeField] private AudioClip audioClipQuitGame;
    [SerializeField] private AudioClip audioClipReset;
    [SerializeField] private AudioClip audioClipResetTriggerHit;
    [SerializeField] private AudioClip audioClipSupportAreaReset;
    [SerializeField] private AudioClip audioClipManualReset;
    [SerializeField] private AudioClip audioClipDangerousObstacleHit;
    [SerializeField] private AudioClip audioClipSafeObstacleHit;
    [SerializeField] private AudioClip audioClipEnvironmentObstacleHit;
    [SerializeField] private AudioClip audioClipEnvironmentObstacleHitFirstTime;

    [Header("Parameters")] 
    [SerializeField] private int maxEnvironmentObstacleHitNotification;
    
    [Header("Events")] 
    [SerializeField] private GameEvent voice2dStartEvent;
    [SerializeField] private GameEvent voice2dEndEvent;
    [SerializeField] private GameEvent environmentObstacleVoiceHitEndEvent;
    [SerializeField] private GameEvent environmentObstacleVoiceHitEvent;
    [SerializeField] private GameEvent safeObstacleVoiceHitEndEvent;
    [SerializeField] private BoolGameEvent safeObstacleVoiceHitEvent;
    [SerializeField] private GameEvent dangerousObstacleVoiceHitEndEvent;
    [SerializeField] private BoolGameEvent dangerousObstacleVoiceHitEvent;
    
    private readonly UnityEvent<Vector3, Transform> _sendLastTargetsPosEvent = new UnityEvent<Vector3, Transform>();
    private readonly UnityEvent<int[]> _sendTimingParametersEvent = new UnityEvent<int[]>();
    private readonly UnityEvent _nextLevel = new UnityEvent();
    
    [Header("Debug")] 
    [SerializeField] private bool skipIntroVoice;
    [SerializeField] private bool useDebugSounds;
    [SerializeField] private AudioClip debugSound;

    #endregion
    
    #region =========PRIVATE_VARIABLES=========
   
    //Private data
    private Target _currentTarget;
    private Transform _lastReachedTargetTransform;
    // [HideInInspector] public Transform lastReachedTargetTransform;
    private int _currentTargetIdx;
    private AudioClip _audioClipToPlay;
    private AudioClip _audioClipOverride2d;
    private AudioClip[] _currentAudioClipArray;
    private float _currentLoopTime;
    private bool _wrongDirectionEnabled;
    private bool _supportEnabled;
    private float _playerAudioListenerHeight;

    //Instance references
    private StopwatchHandler _stopwatchHandler;
    private TimersHandler _timersHandler;
    private Transform _playerAudioSourceTransform;

    private const string TIMER_NAME = "Level"; 
    private const string TIMER_NAME_MANUAL_RESET = TIMER_NAME + "_ManualReset";
    private const string TIMER_NAME_VOICE_2D = TIMER_NAME + "_Voice2D";

    #endregion

    #region =========SAVE_DATA_VARIABLES=========

    private int _supportsAmount;
    private int _wrongDirectionsAmount;
    private int _dangerousObstaclesHitAmount;
    private int _dangerousMovingObstaclesHitAmount;
    private int _safeObstaclesHitAmount;
    private int _safeMovingObstaclesHitAmount;
    private int _environmentObstacleHitAmount;
    private int _resetAreaHitAmount;
    private int _supportAreaResetAmount;
    private int _manualResetAmount;

    private float _totalTimeToFinishLevel;
    private int _totalSupportsAmount;
    private int _totalWrongDirectionsAmount;
    private int _totalDangerousObstaclesHitAmount;
    private int _totalDangerousMovingObstaclesHitAmount;
    private int _totalSafeObstaclesHitAmount;
    private int _totalSafeMovingObstaclesHitAmount;
    private int _totalEnvironmentObstacleHitAmount;
    private int _totalResetAreaHitAmount;
    private int _totalSupportAreaResetAmount;
    private int _totalManualResetAmount;

    #endregion
    
    #region ==========SETUP_AND_START==========
    
    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        _timersHandler = GameManager.Instance.timersHandler;
        _timersHandler.CreateTimer(TIMER_NAME);
        _timersHandler.CreateTimer(TIMER_NAME_MANUAL_RESET);
        
        _stopwatchHandler = GameManager.Instance.stopwatchHandler;
        _stopwatchHandler.CreateStopwatch(TIMER_NAME);
        
        if (!CheckReferences()) return;

        var player = GetComponentInChildren(typeof(PlayerLevelInteraction), true) as PlayerLevelInteraction;
        if (!player)
            throw new Exception("Player not found on the Level's children.");

        SetupEvents(player);

        _playerAudioSourceTransform = player.GetComponentInChildren<AudioListener>().transform;
        _playerAudioListenerHeight = _playerAudioSourceTransform.position.y;

        if (useDebugSounds)
            EnableDebug();
    }

    private void Start()
    {
        if (automaticallyStart)
            PlayIntroAudioClip();
    }

    public void PlayIntroAudioClip()
    {
        if (skipIntroVoice || introAudioClip == null)
        {
            StartLevel();
            return;
        }

        Play2DVoice(introAudioClip, 500, StartLevel);
    }
    
    private void StartLevel()
    {
        InputManager.Instance.EnableGameplayInput();
        
        int[] timingParameters = {sToTriggerSupport, sToTriggerReset, sToTriggerWrongDirection};
        _sendTimingParametersEvent.Invoke(timingParameters); //Send timing to the player
        _currentAudioClipArray = audioClipsToFollow;

        foreach (var target in targets)
            target.DisableColliders();

        _lastReachedTargetTransform = startingPoint;
        StartCurrentTarget();
    }
    
    #endregion

    #region ==========HANDLE_LEVEL==========

    public void TargetReached()
    {
        StopCurrentTarget();
        
        if (++_currentTargetIdx < targets.Length)
            PlayTargetWinVoice();
        else
            PlayLevelWinVoice();
    }

    private void StartCurrentTarget()
    {
        SetupCurrentTarget();
        Play3DVoiceInLoop();
    }

    private void NextLevel()
    {
        SaveLevelData();
        
        _timersHandler.DeleteTimer(TIMER_NAME);
        _stopwatchHandler.DeleteStopwatch(TIMER_NAME);
        
        _nextLevel.Invoke();
    }

    public void QuitLevel()
    {
        Play2DVoice(audioClipQuitGame, 500, GameManager.Instance.EndGame);
    }

    #endregion

    #region ==========HANDLE_TARGETS==========

    private void SetupCurrentTarget()
    {
        _currentTarget = targets[_currentTargetIdx]; //Set current target reference
        _currentTarget.EnableColliders(); //Enable its colliders

        //Move the audio source on the new target position
        var targetPos = _currentTarget.targetCollider.transform.position;
        var sourcePos = targetPos;
        sourcePos.y = _playerAudioListenerHeight;
        voice3DAudioSource.transform.position = sourcePos;
        
        PlayEnvironmentClip();

        _stopwatchHandler.ResumeTimer(TIMER_NAME); //Start the stopwatch
        
        //Start the timer related to the manual reset 2D voice
        _timersHandler.SetTimer(TIMER_NAME_MANUAL_RESET, sToTriggerManualResetVoice * 1000f, 
                                    EnqueueManualResetVoice, true);

        //Send the last and current target positions to the player
        _sendLastTargetsPosEvent.Invoke(targetPos, _lastReachedTargetTransform);
    }

    private void StopCurrentTarget()
    {
        Stop3DVoiceLoop(); //Stop voice

        _stopwatchHandler.StopTime(TIMER_NAME); //Stop stopwatch
        _timersHandler.StopTimer(TIMER_NAME_MANUAL_RESET); //Stop the timer related to manual reset
        if (_audioClipOverride2d) _audioClipOverride2d = null; //Reset the override 2D voice, if present

        SaveCurrentTargetData(); //Save data of the target right passed
        
        _currentTarget.DisableColliders(); //Disable its colliders
        
        _lastReachedTargetTransform = targets[_currentTargetIdx].targetCollider.transform; //Update last target position

        ResetCurrentTargetData(); //Reset the save data
    }

    private void ResetCurrentTarget() //Reset to the last reached target
    {
        Stop3DVoiceLoop();
        _audioClipOverride2d = null;
        
        //The manual reset option will be enabled with the initial timer and not the repeat timer
        _timersHandler.SetTimer(TIMER_NAME_MANUAL_RESET, sToTriggerManualResetVoice * 1000f, 
            EnqueueManualResetVoice, true);

        Play2DVoice(audioClipReset, 500);
    }

    public void ManualTargetReset()
    {
        _manualResetAmount++;
        ResetCurrentTarget();
    }
    
    public void ResetAreaHit()
    {
        _resetAreaHitAmount++;
        
        Play2DVoice(audioClipResetTriggerHit, 200, ResetCurrentTarget);
    }
    
    public void ResetBugAreaHit()
    {
        Play2DVoice(audioClipResetTriggerHit, 200, ResetCurrentTarget);
    }
    
    public void SupportAreaReset()
    {
        _supportAreaResetAmount++;
        
        Play2DVoice(audioClipSupportAreaReset, 200, ResetCurrentTarget);
    }

    #endregion
    
    #region =========HANDLE_3D_VOICES=========
    
    private void Play3DVoiceInLoop()
    { 
        voice3DAudioSource.Stop(); //Stop the voice in the case is playing
        //Compute the current randomized loop time
        _currentLoopTime = msLoopTime + Random.Range(-msLoopRandomizeFactor, msLoopRandomizeFactor);

        if (_audioClipOverride2d) //If there is an enqueued 2d audio clip to override the voice
        {
            Play2DVoice(_audioClipOverride2d, _currentLoopTime); //then play it
            _audioClipOverride2d = null; //and delete the override
            return;
        } 
        
        //Randomly select the next audio clip to play
        _audioClipToPlay = _currentAudioClipArray[Random.Range(0, _currentAudioClipArray.Length)];
        
        voice3DAudioSource.PlayOneShot(_audioClipToPlay); //and play it
        
        //Loop the voice
        _timersHandler.SetTimer(TIMER_NAME, _audioClipToPlay.length * 1000 + _currentLoopTime,
            Play3DVoiceInLoop, true);
    }
    
    private void Stop3DVoiceLoop()
    {
        _timersHandler.StopTimer(TIMER_NAME);
        voice3DAudioSource.Stop();
    }
    
    private void Resume3DVoiceAndInput()
    {
        Play3DVoiceInLoop();
    }
    
    private void PlayEnvironmentClip()
    {
        if (environment2DAudioSource.clip == _currentTarget.environmentClip) return;
        
        environment2DAudioSource.clip = _currentTarget.environmentClip;
        environment2DAudioSource.Play();
    }

    private void Update()
    {
        var direction = _playerAudioSourceTransform.position - voice3DAudioSource.transform.position;
        voice3DAudioSource.transform.rotation = Quaternion.LookRotation(direction);
    }

    #endregion
    
    #region ==========HANDLE_2D_VOICES==========
    
    public void Play2DVoice(AudioClip clip, float delayAtClipEnd, TimersHandler.DelegateMethod methodToCall = null)
    {
        voice2dStartEvent.Raise(); //Raise the event to pause all the obstacles 
        
        if(environment2DAudioSource.isPlaying)
            environment2DAudioSource.Pause(); //Pause the environment sound source
        
        Stop3DVoiceLoop(); //Stop the voice loop
        PauseAllLevelTimers(); //Pause all the timers related to the current level

        InputManager.Instance.PauseInputs(); //Disable the input
        
        voice2DAudioSource.PlayOneShot(clip); //Play the 2D voice
        
        //Call the method to handle the 2D voice end
        _timersHandler.SetOneShotTimer(TIMER_NAME + clip.name,
            clip.length * 1000f + delayAtClipEnd - 10f,
            End2DVoiceEvent);

        if(methodToCall != null)
        {
            //Call the specified method at the end of the clip
            _timersHandler.SetOneShotTimer(TIMER_NAME_VOICE_2D, clip.length * 1000f + delayAtClipEnd,
                methodToCall);
        }
    }

    private void End2DVoiceEvent()
    {
        voice2dEndEvent.Raise(); //Raise the event to resume all the obstacles
        InputManager.Instance.ResumeInputs();

        //If it's a voice played after the reaching of the last target,
        //or before the level has been started, don't resume the level
        if (_currentTargetIdx == targets.Length || _currentTarget == null) return; 
        
        Resume3DVoiceAndInput(); //Resume the voice loop and enable the character input
        ResumeAllLevelTimers(); //Resume all the timers related to the current level
        if (environment2DAudioSource.clip)
            environment2DAudioSource.Play(); //Resume the environment sound source
    }
    
    private void PlayTargetWinVoice()
    {
        var clip = useDebugSounds ? debugSound : _currentTarget.audioClipWin; 
        Play2DVoice(clip, 1000, StartCurrentTarget);
    }

    private void PlayLevelWinVoice()
    {
        Play2DVoice(targets[_currentTargetIdx-1].audioClipWin, 1000, NextLevel);
    }
    
    private void EnqueueManualResetVoice()
    {
        //The timer is set but not start, it will be started from the ResumeAllTimers method when the 2D voice ends
        _timersHandler.SetTimer(TIMER_NAME_MANUAL_RESET, 
            sToRepeatManualResetVoice * 1000f +
            _timersHandler.GetLeftover(TIMER_NAME),
            EnqueueManualResetVoice, true);
        
        Debug.Log("Enqueued");
        _audioClipOverride2d = audioClipManualReset;
    }
    
    #endregion
    
    #region =========HANDLE_OBSTACLES=========

    private void DangerousObstaclesHit(bool isMoving)
    {
        if (!isMoving)
            _dangerousObstaclesHitAmount++;
        else
            _dangerousMovingObstaclesHitAmount++;
        
        Stop3DVoiceLoop();
        
        Play2DVoice(audioClipDangerousObstacleHit, 100, DangerousObstacleVoiceEnd);
    }

    private void SafeObstaclesHit(bool isMoving)
    {
        if (!isMoving)
            _safeObstaclesHitAmount++;
        else
            _safeMovingObstaclesHitAmount++;
        
        Stop3DVoiceLoop();
        
        Play2DVoice(audioClipSafeObstacleHit, 100, SafeObstacleVoiceHitEnd);
    }

    private void EnvironmentObstacleHit()
    {
        ++_environmentObstacleHitAmount;
        ++_totalEnvironmentObstacleHitAmount;
        if (maxEnvironmentObstacleHitNotification < _totalEnvironmentObstacleHitAmount)
            return;
        Stop3DVoiceLoop();
        
        var audioClip = _totalEnvironmentObstacleHitAmount > 1
            ? audioClipEnvironmentObstacleHit
            : audioClipEnvironmentObstacleHitFirstTime;
        
        Play2DVoice(audioClip, 100, EnvironmentObstacleVoiceHitEnd);
    }

    private void EnvironmentObstacleVoiceHitEnd()
    {
        environmentObstacleVoiceHitEndEvent.Raise();
    }

    private void SafeObstacleVoiceHitEnd()
    {
        safeObstacleVoiceHitEndEvent.Raise();
    }

    private void DangerousObstacleVoiceEnd()
    {
        dangerousObstacleVoiceHitEndEvent.Raise();
        ResetCurrentTarget();
    }

    #endregion

    #region =========SUPPORT_METHODS=========
    
    private void PauseAllLevelTimers()
    {
        _timersHandler.PauseAllTimersContainingString(TIMER_NAME);
    }

    private void ResumeAllLevelTimers()
    {
        _timersHandler.ResumeAllTimersContainingString(TIMER_NAME);
    }

    public void EnableSupport(bool on)
    {
        if (on)
        {
            _currentAudioClipArray = audioClipsToSupport;
            _supportsAmount++;
        }
        else
            _currentAudioClipArray = _wrongDirectionEnabled ? audioClipsWrongDirection : audioClipsToFollow;
        
        _supportEnabled = on;

    }

    public void EnableWrongDirection(bool on)
    {
        if (on)
        {
            _currentAudioClipArray = audioClipsWrongDirection;
            _wrongDirectionsAmount++;
        }
        else
            _currentAudioClipArray = _supportEnabled ? audioClipsToSupport : audioClipsToFollow;

        _wrongDirectionEnabled = on;
    }

    #endregion
    
    #region =========SAVE_DATA=========
    
    private void ResetCurrentTargetData()
    {
        _stopwatchHandler.ResetTime(TIMER_NAME);
        _resetAreaHitAmount = 0;
        _supportAreaResetAmount = 0;
        _manualResetAmount = 0;
        _supportsAmount = 0;
        _wrongDirectionsAmount = 0;
        _dangerousObstaclesHitAmount = 0;
        _dangerousMovingObstaclesHitAmount = 0;
        _safeObstaclesHitAmount = 0;
        _safeMovingObstaclesHitAmount = 0;
        _environmentObstacleHitAmount = 0;
    }

    private void SaveCurrentTargetData()
    {
        var timeToReachTarget = (float)Math.Truncate(_stopwatchHandler.GetTime(TIMER_NAME) / 1000f * 100) / 100;
        
        TargetData targetData = new TargetData();
        targetData.levelName = levelName;
        targetData.name = _currentTarget.name;
        targetData.timeToReach = timeToReachTarget;
        targetData.supportsAmount = _supportsAmount;
        targetData.wrongDirectionsAmount = _wrongDirectionsAmount;
        targetData.dangerousObstaclesHitAmount = _dangerousObstaclesHitAmount;
        targetData.dangerousMovingObstaclesHitAmount = _dangerousMovingObstaclesHitAmount;
        targetData.safeObstaclesHitAmount = _safeObstaclesHitAmount;
        targetData.safeMovingObstaclesHitAmount = _safeMovingObstaclesHitAmount;
        targetData.environmentObstacleHitAmount = _environmentObstacleHitAmount;
        targetData.resetAreaHitAmount = _resetAreaHitAmount;
        targetData.supportAreaResetAmount = _supportAreaResetAmount;
        targetData.manualResetAmount = _manualResetAmount;
        GameManager.Instance.saveDataManager.AddTarget(targetData);
        
        UpdateTotalAmountsData(timeToReachTarget);
    }

    private void UpdateTotalAmountsData(float timeToReach)
    {
        // _totalTimeToFinishLevel += _stopwatchHandler.GetTime(TIMER_NAME);
        _totalTimeToFinishLevel += timeToReach;
        _totalResetAreaHitAmount += _resetAreaHitAmount;
        _totalSupportAreaResetAmount += _supportAreaResetAmount;
        _totalManualResetAmount += _manualResetAmount;
        _totalSupportsAmount += _supportsAmount;
        _totalWrongDirectionsAmount += _wrongDirectionsAmount;
        _totalDangerousObstaclesHitAmount += _dangerousObstaclesHitAmount;
        _totalDangerousMovingObstaclesHitAmount += _dangerousMovingObstaclesHitAmount;
        _totalSafeObstaclesHitAmount += _safeObstaclesHitAmount;
        _totalSafeMovingObstaclesHitAmount += _safeMovingObstaclesHitAmount;
        
        //Real-time update applied in EnvironmentObstacleHit()
        // _totalEnvironmentObstacleHitAmount += _environmentObstacleHitAmount; 
    }

    private void SaveLevelData()
    {
        OverallLevelData overallLevelData = new OverallLevelData();
        overallLevelData.levelName = levelName;
        overallLevelData.totalTimeToFinishLevel = _totalTimeToFinishLevel;
        overallLevelData.totalSupportsAmount = _totalSupportsAmount;
        overallLevelData.totalWrongDirectionsAmount = _totalWrongDirectionsAmount;
        overallLevelData.totalDangerousObstaclesHitAmount = _totalDangerousObstaclesHitAmount;
        overallLevelData.totalDangerousMovingObstaclesHitAmount = _totalDangerousMovingObstaclesHitAmount;
        overallLevelData.totalSafeObstaclesHitAmount = _totalSafeObstaclesHitAmount;
        overallLevelData.totalSafeMovingObstaclesHitAmount = _totalSafeMovingObstaclesHitAmount;
        overallLevelData.totalEnvironmentObstacleHitAmount = _totalEnvironmentObstacleHitAmount;
        overallLevelData.totalResetAreaHitAmount = _totalResetAreaHitAmount;
        overallLevelData.totalSupportAreaResetAmount = _totalSupportAreaResetAmount;
        overallLevelData.totalManualResetAmount = _totalManualResetAmount;
        GameManager.Instance.saveDataManager.SaveLevelOverallData(overallLevelData);
    }

    #endregion

    #region EVENTS

    private void SetupEvents(PlayerLevelInteraction player)
    {
        _sendLastTargetsPosEvent.AddListener(player.ReceiveCurrentTargetPosition);
        _sendTimingParametersEvent.AddListener(player.ReceiveTimingParameters);
        _nextLevel.AddListener(GameManager.Instance.NextLevel);
        _nextLevel.AddListener(() => player.enabled = false);

        environmentObstacleVoiceHitEvent.AddListener(EnvironmentObstacleHit);
        safeObstacleVoiceHitEvent.AddListener(SafeObstaclesHit);
        dangerousObstacleVoiceHitEvent.AddListener(DangerousObstaclesHit);
    }
    
    private void OnDestroy()
    {
        _sendLastTargetsPosEvent.RemoveAllListeners();
        _sendTimingParametersEvent.RemoveAllListeners();
        _nextLevel.RemoveAllListeners();
        environmentObstacleVoiceHitEvent.RemoveListener(EnvironmentObstacleHit);
        safeObstacleVoiceHitEvent.RemoveListener(SafeObstaclesHit);
        dangerousObstacleVoiceHitEvent.RemoveListener(DangerousObstaclesHit);
    }

    #endregion

    #region DEBUG_AND_ERRORCHECK

    private void EnableDebug()
    {
        audioClipQuitGame = debugSound;
        audioClipReset = debugSound;
        audioClipResetTriggerHit = debugSound;
        audioClipSupportAreaReset = debugSound;
        audioClipManualReset = debugSound;
        audioClipEnvironmentObstacleHit = debugSound;
        audioClipEnvironmentObstacleHitFirstTime = debugSound;
        audioClipDangerousObstacleHit = debugSound;
        audioClipSafeObstacleHit = debugSound;
    }

    private bool CheckReferences()
    {
        if (environment2DAudioSource)
            environment2DAudioSource.loop = true;
        else
            Debug.LogError("Environment audio source is missing!");

        if (targets.Length > 0) return true;
        Debug.LogError("Targets array is empty!");
        return false;

    }

    #endregion
    
}

[Serializable]
public class Target
{
    [Header("Target specific parameters")]
    public string name;
    public AudioClip audioClipWin;
    public AudioClip environmentClip;

    [Header("Spatial parameters")]
    public GameObject targetCollider; //to trigger the next intermediate voice source
    public GameObject resetCollider; //to trigger the reset
    public GameObject supportCollider; //to receive support from the voice 

    public void EnableColliders()
    {
        targetCollider.SetActive(true);
        if (resetCollider)
            resetCollider.SetActive(true);
        if (supportCollider)
            supportCollider.SetActive(true);
    }

    public void DisableColliders()
    {
        if (!targetCollider)
            return;
        
        targetCollider.SetActive(false);
        if (resetCollider)
            resetCollider.SetActive(false);
        if (supportCollider)
            supportCollider.SetActive(false);
    }
}
