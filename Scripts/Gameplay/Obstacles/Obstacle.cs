using ScriptableObjectArchitecture;
using UnityEngine;
using Utilities;
using Utilities.Time;

public class Obstacle : MonoBehaviour
{
    [SerializeField] protected GameEventBase hitEvent;
    
    [Header("Obstacle Audio Clips")]
    [SerializeField] public AudioClip hitAudioClip;
    
    //Private
    protected TimersHandler _timersHandler;

    protected string TIMER_NAME;

    protected virtual void Awake()
    {
        _timersHandler = GameManager.Instance.timersHandler;
        SetupTimers();
        TIMER_NAME = _timersHandler.CreateTimerWithRandomID(TIMER_NAME);
    }

    protected virtual void SetupTimers()
    {
        TIMER_NAME = "Obstacle_" + gameObject.name;
    }

    protected virtual void Start()
    {
        
    }

    public virtual void RaiseHitEvent()
    {
        ((GameEvent)hitEvent).Raise();
    }
    
}
