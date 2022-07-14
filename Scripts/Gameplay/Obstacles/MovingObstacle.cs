using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ScriptableObjectArchitecture;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MovingObstacle : ObstacleWithSound
{
    [Header("Movement")] [SerializeField] private List<Transform> waypoints;

    [SerializeField] [Range(0.05f, 30f)] private float metersPerSecond;
    [SerializeField] [Range(10, 270)] private int anglesPerSecond;
    [SerializeField] private bool loop;
    [SerializeField] private bool pingpong;
    [SerializeField] private bool disableRotation;
    [SerializeField] [Range(0, 5000)] private int loopTime;

    private Vector3 _currentTarget;
    private int _currentWaypoint = 0;
    private bool _isPaused;

    private string TIMER_NAME_MOVING = "MovingObstacle_";

    #region SETUP
    
    protected override void SetupTimers()
    {
        TIMER_NAME = "Obstacle_" + transform.parent.gameObject.name + gameObject.name;
        TIMER_NAME_MOVING += transform.parent.gameObject.name + gameObject.name;
        TIMER_NAME_MOVING = _timersHandler.CreateTimerWithRandomID(TIMER_NAME_MOVING);
    }

    protected override void Start()
    {
        base.Start();

        ResetPositionAndRotation();

        if (pingpong)
            SetupPingpongWaypoints();

        NextTarget();
    }

    private void SetupPingpongWaypoints()
    {
        if (waypoints.Count < 2) return;
        
        for (var i = waypoints.Count - 2; i >= 0; i--)
            waypoints.Add(waypoints[i]);
    }
    
    #endregion

    #region MOVE

     private void StartMoving()
    {
        StartCoroutine(MoveToTarget());
    }

    IEnumerator MoveToTarget()
    {
        var initialPos = transform.position;
        var t = 0f;
        var totalTime = (_currentTarget - transform.position).magnitude / metersPerSecond;
        while (t <= 1f)
        {
            if(_isPaused)
                yield return null;
            else
            {
                t += Time.deltaTime / totalTime;
                transform.position = Vector3.Lerp(initialPos, _currentTarget, t);
                yield return null;
            }
        }
        transform.position = _currentTarget;
        NextTarget();
    }

    #endregion

    #region ROTATION

    private void RotateToTarget()
    {
        if (disableRotation)
        {
            StartMoving();
            return;
        }
        
        var direction = (_currentTarget - transform.position).normalized;
        var rotation = Quaternion.LookRotation(direction, transform.up);
        StartCoroutine(RotateTowards(rotation));
    }

    IEnumerator RotateTowards(Quaternion rotation)
    {
        var startRotation = transform.rotation;
        var t = 0f;
        var _rotationTime = Quaternion.Angle(transform.rotation, rotation) / anglesPerSecond;
        while (t <= 1f)
        {
            if(_isPaused)
                    yield return null;
            else
            {
                t += Time.deltaTime / _rotationTime;
                transform.rotation = Quaternion.Lerp(startRotation, rotation, t);
                yield return null;
            }
        }

        transform.rotation = rotation;
        StartMoving();
    }
    
    #endregion
    
    #region HANDLE_TARGET_SEQUENCE

    private void NextTarget()
    {
        _currentWaypoint += 1;
        _currentWaypoint %= waypoints.Count;
        _currentTarget = waypoints[_currentWaypoint].position;
        if (IsInTheLastWaypoint())
        {
            HandleLoop();
            return;
        }

        RotateToTarget();
    }

    private bool IsInTheLastWaypoint()
    {
        return _currentWaypoint == 0;
    }

    private void HandleLoop()
    {
        if (loop)
        {
            if (pingpong)
            {
                _currentWaypoint++;
                _currentTarget = waypoints[_currentWaypoint].position;
            }
            _timersHandler.SetTimer(TIMER_NAME_MOVING, loopTime, RotateToTarget, true);
        }
        else
            _timersHandler.SetTimer(TIMER_NAME_MOVING, loopTime, Reset, true);
    }
    
    #endregion

    #region RESET_PAUSE

    private void ResetPositionAndRotation()
    {
        _currentTarget = waypoints[0].position;
        transform.position = _currentTarget;
        transform.rotation = waypoints[0].rotation;
    }

    public void Pause()
    {
        _isPaused = true;
        _timersHandler.PauseTimer(TIMER_NAME_MOVING);
    }

    public void Resume()
    {
        _isPaused = false;
        if (_timersHandler.IsRunningOrPaused(TIMER_NAME_MOVING))
            _timersHandler.StartTimer(TIMER_NAME_MOVING);
    }

    public void Reset()
    {
        _currentWaypoint = 0;
        ResetPositionAndRotation();
        StopObstacleSound();
        PlayObstacleSound();
        NextTarget();
    }

    #endregion
    
    public override void RaiseHitEvent()
    {
        ((BoolGameEvent)hitEvent).Raise(true);
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var w in waypoints)
        {
            Gizmos.DrawCube(w.position, 0.2f * Vector3.one);
        }
    }
    #endif
}