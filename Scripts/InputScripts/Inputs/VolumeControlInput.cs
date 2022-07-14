using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class VolumeControlInput : MonoBehaviour, PlayerInputAction.IVolumeControlMapActions
{
    //Volume control
    private readonly UnityEvent _nextVolume = new UnityEvent();
    private readonly UnityEvent _previousVolume = new UnityEvent();
    private readonly UnityEvent _volumeUp = new UnityEvent();
    private readonly UnityEvent _volumeDown = new UnityEvent();
    
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        SetupEvents();
    }

    private void SetupEvents()
    {
        var volumeController = GetComponentInChildren<VolumeController>();
        _nextVolume.AddListener(volumeController.NextVolume);
        _previousVolume.AddListener(volumeController.PreviousVolume);
        _volumeUp.AddListener(volumeController.VolumeUp);
        _volumeDown.AddListener(volumeController.VolumeDown);
    }

    public void OnVolumeUp(InputAction.CallbackContext context)
    {
        if (context.started)
            _volumeUp.Invoke();
    }
        
    public void OnVolumeDown(InputAction.CallbackContext context)
    {
        if (context.started)
            _volumeDown.Invoke();
    }
        
    public void OnNextVolume(InputAction.CallbackContext context)
    {
        if (context.started)
            _nextVolume.Invoke();
    }

    public void OnPreviousVolume(InputAction.CallbackContext context)
    {
        if (context.started)
            _previousVolume.Invoke();
    }
    
    void OnSceneUnloaded(Scene scene)
    {
        _nextVolume.RemoveAllListeners();
        _previousVolume.RemoveAllListeners();
        _volumeUp.RemoveAllListeners();
        _volumeDown.RemoveAllListeners();
    }
}
