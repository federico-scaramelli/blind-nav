using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Time;

public class StartGameVoiceLoop : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startVoiceAudioClip;
    [SerializeField] private float msLoopTime;

    private TimersHandler _timersHandler;
    private const string TIMER_NAME = "Start_Game";

    private void Awake()
    {
        _timersHandler = GameManager.Instance.timersHandler;
        _timersHandler.CreateTimer(TIMER_NAME);
    }

    private void Start()
    {
        PlayVoice();
    }

    private void PlayVoice()
    {
        audioSource.PlayOneShot(startVoiceAudioClip);
        _timersHandler.SetTimer(TIMER_NAME, startVoiceAudioClip.length * 1000 + msLoopTime, 
            PlayVoice, true);
    }

    public void Stop()
    {
        audioSource.Stop();
        _timersHandler.DeleteTimer(TIMER_NAME);
        enabled = false;
    }
}
