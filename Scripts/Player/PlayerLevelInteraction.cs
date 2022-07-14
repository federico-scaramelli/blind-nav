using System;
using CMF;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Events;
using Utilities;
using Utilities.Time;

public class PlayerLevelInteraction : MonoBehaviour
{
    #region INSPECTOR_PARAMETERS

    [SerializeField] [Range(-1f, 1f)] 
    [Tooltip("A value equal to 1 means the player forward direction is perfectly aligned with the target.")] 
    private float correctDirectionLimit;

    [SerializeField] private float backwardPosResetAmount;

    [Header("Events")]
    [SerializeField] private GameEvent safeObstacleHitVoiceEndEvent;
    [SerializeField] private GameEvent dangerousObstacleHitVoiceEndEvent;
    [SerializeField] private GameEvent start2dVoiceEvent;
    [SerializeField] private GameEvent end2dVoiceEvent;
    private readonly UnityEvent<bool> _enableWrongDirectionEvent = new UnityEvent<bool>();
    private readonly UnityEvent<bool> _enableSupportEvent = new UnityEvent<bool>();
    private readonly UnityEvent _resetAreaHitEvent = new UnityEvent();
    private readonly UnityEvent _resetBugAreaHitEvent = new UnityEvent();
    private readonly UnityEvent _supportAreaResetEvent = new UnityEvent();
    private readonly UnityEvent _nextTargetEvent = new UnityEvent();

    #endregion

    #region PRIVATE
    //Instance references
    private TimersHandler _timersHandler;
    private AudioSource _hitAudioSource;

    //Private
    private int _sToTriggerSupport;
    private int _sToTriggerReset;
    private int _sToTriggerWrongDirection;
    private Vector3 _fromPlayerToTarget;
    private Vector3 _currentTargetPos;
    private Transform _lastReachedTargetTransform;
    private Vector3 _safeObstacleHitNormal;
    private bool _wrongDirectionEnabled;
    private bool _supportEnabled;
    private int _supportTriggerCount; //To handle multiple colliders
    private bool _isResetting;

    //DON'T set the names with the word 'Level' inside!
    private const string TIMER_NAME_SUPPORT = "PlayerInteraction_Support";
    private const string TIMER_NAME_WRONG_DIR = "PlayerInteraction_WrongDirection";
    
    #endregion
    
    #region SETUP_AND_START
    
    private void Awake()
    {
        _timersHandler = GameManager.Instance.timersHandler;
        _timersHandler.CreateTimer(TIMER_NAME_SUPPORT);
        _timersHandler.CreateTimer(TIMER_NAME_WRONG_DIR);
        
        _hitAudioSource = GetComponentInChildren<AudioSource>();

        var level = GetComponentInParent<Level>();
        if (!level)
            throw new Exception("Level not found on the Player's parent.");
        SetupEvents(level);
    }

    #endregion

    #region HANDLE_INTERACTION

    public void ManualReset()
    {
        _isResetting = true;
        ResetPlayer();
    }
    
    private void SupportAreaReset()
    {
        ResetPlayer();
        _supportAreaResetEvent.Invoke();
    }

    private void ResetAreaHit()
    {
        ResetPlayer();
        _resetAreaHitEvent.Invoke();
    }

    private void ResetPlayer()
    {
        ResetPlayerPosition();

        ResetTimers();
    }

    private void ResetTimers()
    {
        if (_wrongDirectionEnabled)
            DisableWrongDirection();
        if (_supportEnabled)
            DisableSupport(); 
        
        _timersHandler.StopTimer(TIMER_NAME_SUPPORT);
        _timersHandler.StopTimer(TIMER_NAME_WRONG_DIR);
    }
    
    private void ResetPlayerPosition()
    {
        //Reset player position
        var t = transform;
        t.position = _lastReachedTargetTransform.position;
        var camera = GetComponentInChildren<CameraController>();
        camera.SetRotationAngles(0, _lastReachedTargetTransform.rotation.y);
        t.rotation = _lastReachedTargetTransform.rotation;
    }

    private void MovePlayerBackward() {
        var t = transform.GetChild(0).transform;
        // transform.position -= t.forward * backwardPosResetAmount;
        transform.position += _safeObstacleHitNormal * backwardPosResetAmount;
    }

    private void HandleObstacleCollision(Collision other)
    {
        Obstacle obstacle = other.gameObject.GetComponent<Obstacle>();
        _safeObstacleHitNormal = other.contacts[0].normal;

        PlayObstacleHitAudio(other, obstacle);
        obstacle.RaiseHitEvent();
    }

    private void PlayObstacleHitAudio(Collision other, Obstacle obstacle)
    {

        _hitAudioSource.transform.position = other.contacts[0].point;
        _hitAudioSource.transform.rotation = Quaternion.Euler(other.contacts[0].normal);
        _hitAudioSource.PlayOneShot(obstacle.hitAudioClip);
    }

    #endregion
    
    #region HANDLE_SUPPORT
    
    private void StartSupportTimer()
    {
        // Debug.Log("Support entered");
        _supportTriggerCount++;
        _timersHandler.SetTimer(TIMER_NAME_SUPPORT, _sToTriggerSupport * 1000f, 
            EnableSupport, true);

        //If Wrong direction timer is running, then pause it
        if (_timersHandler.IsRunning(TIMER_NAME_WRONG_DIR))
            _timersHandler.PauseTimer(TIMER_NAME_WRONG_DIR);
    }
    
    private void Update()
    {
        CheckWrongDirection();
    }
    
    private void CheckWrongDirection()
    {
        //If the player is in the support trigger, 
        if (_timersHandler.IsRunningOrPaused(TIMER_NAME_SUPPORT) && _supportEnabled && !_wrongDirectionEnabled 
            || _currentTargetPos == Vector3.zero) //or the level is not started yet 
            return; //disable wrong direction check

        var t = transform.GetChild(0).transform;
        _fromPlayerToTarget = _currentTargetPos - t.position;
        // Debug.DrawLine(t.position, t.position + _fromPlayerToTarget, Color.blue, 100f);
        var dot = Vector3.Dot(t.forward, _fromPlayerToTarget.normalized);

        var directionIsWrong = dot < correctDirectionLimit;

        switch (directionIsWrong)
        {
            //If direction is wrong but not enabled yet and the timer is not already started
            case true when !_wrongDirectionEnabled && !_timersHandler.IsRunningOrPaused(TIMER_NAME_WRONG_DIR):
                
                // Debug.Log("Start timer wrong direction");
                _timersHandler.SetTimer(TIMER_NAME_WRONG_DIR, _sToTriggerWrongDirection * 1000f,
                    EnableWrongDirection, true);
                
                break;
            
            //If direction is not wrong anymore but it is currently enabled
            case false when _wrongDirectionEnabled:
                DisableWrongDirection(); //Disable it and stop the timer
                break;
            
            //If direction is not wrong and the timer is running (not paused or never started)
            case false when _timersHandler.IsRunning(TIMER_NAME_WRONG_DIR):
            {
                _timersHandler.StopTimer(TIMER_NAME_WRONG_DIR); //then stop the timer
                // Debug.Log("Stop wrong direction timer"); 
                break;
            }
        }
    }
    
    #endregion

    #region COLLISIONS_DETECTION

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "SupportVolume" when _supportTriggerCount == 0:
                StartSupportTimer();
                break;
            case "SupportVolume":
                _supportTriggerCount++;
                break;
            case "ResetVolume":
                ResetAreaHit();
                break;
            case "ResetBugVolume":
                ResetAreaHit();
                break;
            case "TargetVolume":
                _nextTargetEvent.Invoke();
                ResetTimers();
                break;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.tag.Equals("SupportVolume")) return;
        
        if (--_supportTriggerCount >= 1)
            return;
        
        //If it's due to a reset, do nothing as ResetPlayer take care of everything.
        if (_isResetting ||  //isResetting could be set true by ResetPlayer
            _supportEnabled && _timersHandler.GetLeftover(TIMER_NAME_SUPPORT) == 0)
        {
            _isResetting = false;
            return;
        }

        if (_wrongDirectionEnabled) //If the wrong direction was enabled
            EnableWrongDirection(); //Enable it again
        else if (_timersHandler.IsRunningOrPaused(TIMER_NAME_WRONG_DIR)) //If the wrong direction timer was paused
            _timersHandler.ResumeTimer(TIMER_NAME_WRONG_DIR); //resume it
        
        if (_supportEnabled) //If support is enabled
            DisableSupport(); //disable support
            
        _timersHandler.StopTimer(TIMER_NAME_SUPPORT); //Anyway stop the support timer
        // Debug.Log("Support exit");
    }
    
    private void OnCollisionEnter(Collision other)
        {
            switch (other.transform.tag)
            {
                case "Obstacle":
                    HandleObstacleCollision(other);
                    break;
            }
        }

    #endregion
    
    #region SUPPORT_METHODS

    private void EnableWrongDirection()
    {
        // Debug.Log("Start wrong direction");
        _timersHandler.StopTimer(TIMER_NAME_WRONG_DIR);
        _wrongDirectionEnabled = true;
        
        _enableWrongDirectionEvent.Invoke(true);
    }
    
    private void DisableWrongDirection()    
    {
        // Debug.Log("Stop wrong direction");
        _wrongDirectionEnabled = false;

        _enableWrongDirectionEvent.Invoke(false);
    }
    
    private void EnableSupport()
    {
        // Debug.Log("support enabled");
        
        _timersHandler.StopTimer(TIMER_NAME_SUPPORT);
        _timersHandler.SetTimer(TIMER_NAME_SUPPORT, _sToTriggerReset * 1000f, SupportAreaReset, true);

        _supportEnabled = true;
        
        _enableSupportEvent.Invoke(true);
    }

    private void DisableSupport()
    {
        // Debug.Log("support disabled");

        _supportEnabled = false;

        _enableSupportEvent.Invoke(false);
    }

    public void ReceiveCurrentTargetPosition(Vector3 currentPos, Transform lastReachedTransform)
    {
        _currentTargetPos = currentPos;
        _lastReachedTargetTransform = lastReachedTransform;
    }

    public void ReceiveTimingParameters(int[] timingParameters)
    {
        _sToTriggerSupport = timingParameters[0];
        _sToTriggerReset = timingParameters[1];
        _sToTriggerWrongDirection = timingParameters[2];
    }

    private void PauseTimers()
    {
        _timersHandler.PauseTimer(TIMER_NAME_SUPPORT);
        _timersHandler.PauseTimer(TIMER_NAME_WRONG_DIR);
    }

    private void ResumeTimers()
    {
        if (_timersHandler.IsPaused(TIMER_NAME_SUPPORT))
            _timersHandler.ResumeTimer(TIMER_NAME_SUPPORT);
        if (_timersHandler.IsPaused(TIMER_NAME_SUPPORT))
            _timersHandler.ResumeTimer(TIMER_NAME_WRONG_DIR);
    }

    #endregion

    #region EVENTS

    private void SetupEvents(Level level)
    {
        _enableWrongDirectionEvent.AddListener(level.EnableWrongDirection);
        _enableSupportEvent.AddListener(level.EnableSupport);
        _resetAreaHitEvent.AddListener(level.ResetAreaHit);
        _resetBugAreaHitEvent.AddListener(level.ResetBugAreaHit);
        _supportAreaResetEvent.AddListener(level.SupportAreaReset);
        _nextTargetEvent.AddListener(level.TargetReached);
        
        safeObstacleHitVoiceEndEvent.AddListener(MovePlayerBackward);
        dangerousObstacleHitVoiceEndEvent.AddListener(ResetPlayer);
        start2dVoiceEvent.AddListener(PauseTimers);
        end2dVoiceEvent.AddListener(ResumeTimers);
    }
    
    private void OnDestroy()
    {
        _enableWrongDirectionEvent.RemoveAllListeners();
        _enableSupportEvent.RemoveAllListeners();
        _resetAreaHitEvent.RemoveAllListeners();
        _resetBugAreaHitEvent.RemoveAllListeners();
        _supportAreaResetEvent.RemoveAllListeners();
        _nextTargetEvent.RemoveAllListeners();
        
        safeObstacleHitVoiceEndEvent.RemoveListener(MovePlayerBackward);
        dangerousObstacleHitVoiceEndEvent.RemoveListener(ResetPlayer);
        start2dVoiceEvent.RemoveListener(PauseTimers);
        end2dVoiceEvent.RemoveListener(ResumeTimers);
    }

    #endregion
}
