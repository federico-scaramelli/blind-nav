using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Utilities;
using Utilities.Time;

public class MovingSoundPresentation : MonoBehaviour
{
    [Header("Presentation")]
    [SerializeField] private Transform transformToMove;
    [SerializeField] private Transform targetPoint;
    [SerializeField] [Range(0.5f, 10)] private float metersPerSecond;
    [SerializeField] [Range(1, 5)] private int resetSpeedMultiplier;
    [SerializeField] private bool loopSound;
    [SerializeField] [Range(0, 1500)]private int msLoopDelay;

    [Header("Events")] 
    [SerializeField] private UnityEvent notifyResetToDispatcher;
    
    //Instances Reference
    private SingleSoundPresentation _soundPresentation;
    private Vector3 _initialPosition;
    private TimersHandler _timersHandler;

    private string _timerName;
    private bool _isPaused;

    #region SETUP

    private void Awake()
    {
        _initialPosition = transformToMove.position;
        _timersHandler = GameManager.Instance.timersHandler;
    }

    private void Start()
    {   

        try
        {
            if (notifyResetToDispatcher.GetPersistentTarget(0) == null) return;
        }
        catch (Exception)
        {
            Debug.LogError("MovingSoundPresentation '"+ gameObject.name +"' event has not any listener.");
        }
    }

    private void OnEnable()
    {
        transformToMove.position = _initialPosition;
    }
    
    #endregion
    
    public void StartMoving(SingleSoundPresentation mySoundPresentation)
    {
        _soundPresentation = mySoundPresentation;
        _timerName = _soundPresentation.TimerName + "_Moving";
        
        StartCoroutine(MoveToTarget(targetPoint.position, metersPerSecond));
        
        PlaySound();       
    }

    IEnumerator MoveToTarget(Vector3 targetPos, float speed)
    {
        var t = 0f;
        var startingPos = transformToMove.position;
        var moveTime = (targetPos - startingPos).magnitude / speed;
        while (t <= 1f)
        {
            if (_isPaused)
                yield return null;
            else
            {
                t += Time.deltaTime / moveTime;
                transformToMove.position = Vector3.Lerp(startingPos, targetPos, t);
                yield return null;
            }
        }

        transformToMove.position = targetPos;
        if (targetPos == _initialPosition)
            NotifyResetToDispatcher();
        else
            End();
    }

    private void PlaySound()
    {
        _soundPresentation.Play3DSoundOneShot();
        
        if (!loopSound) return;
        
        _timersHandler.SetTimer(_timerName, 
                    _soundPresentation.Sound3DAudioClip.length * 1000 + msLoopDelay, PlaySound, true);
    }
    
    #region RESET_PAUSE_END
    
    private void End()
    {
        _timersHandler.StopTimer(_timerName);
        _soundPresentation.EndPresentation();
    }

    public void Reset()
    {
        StartCoroutine(MoveToTarget(_initialPosition, resetSpeedMultiplier * metersPerSecond));
    }

    public void Pause()
    {
        if (_timerName == null) return;
        _isPaused = true;
        _timersHandler.PauseTimer(_timerName);
    }

    public void Resume()
    {
        //Do not resume if the object is already at target (reset point or target point)
        if (Vector3.Distance(targetPoint.position, transformToMove.position) < 0.001f
            || Vector3.Distance(targetPoint.position, _initialPosition) < 0.001f
            ||  _timerName == null) 
            return; 
        _isPaused = false;
        _timersHandler.ResumeTimer(_timerName);
    }
    
    private void NotifyResetToDispatcher()
    {
        notifyResetToDispatcher.Invoke();
    }
    
    #endregion
}
