using ScriptableObjectArchitecture;
using UnityEngine;

public class ObstacleWithSound : Obstacle
{  
    [SerializeField] private AudioClip obstacleLoopSoundAudioClip; //Sound emitted by the obstacle continuously
    [SerializeField] [Range(0, 2000)] private int msSoundLoopDelay;
    [SerializeField] [Range(0, 1000)] private int msRandomFactorLoopDelay;

    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();
        
        _audioSource = GetComponentInChildren<AudioSource>();
        if (!_audioSource) 
            Debug.LogError("Obstacle with sound " + gameObject.name + " has not an audio source.");
    }

    protected override void Start()
    {
        base.Start();

        if (msSoundLoopDelay == 0)
        {
            _audioSource.loop = true;
            _audioSource.clip = obstacleLoopSoundAudioClip;
        }

        PlayObstacleSound();
    }

    public void PlayObstacleSound()
    {
        if (msSoundLoopDelay == 0)
            _audioSource.Play();
        else
        {
            _audioSource.PlayOneShot(obstacleLoopSoundAudioClip);
            _timersHandler.SetTimer(TIMER_NAME, obstacleLoopSoundAudioClip.length * 1000 + msSoundLoopDelay 
                                    + Random.Range(-msRandomFactorLoopDelay, msRandomFactorLoopDelay + 1), 
                                    PlayObstacleSound, true);
        }
    }

    public void StopObstacleSound()
    {
        _audioSource.Stop();
        
        _timersHandler.StopTimer(TIMER_NAME);
    }
    
    public override void RaiseHitEvent()
    {
        ((BoolGameEvent)hitEvent).Raise(false);
    }
}
