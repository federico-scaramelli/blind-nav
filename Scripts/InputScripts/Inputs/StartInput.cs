using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StartInput : MonoBehaviour, PlayerInputAction.IStartMapActions 
{
    private UnityEvent _startEvent = new UnityEvent();
    
    private void Awake()
    {
        var dispatcher = FindObjectOfType<SoundsPresentationDispatcher>();
        if (dispatcher == null)
        {
            Debug.LogError("Dispatcher not found");
            return;
        }
        
        var startVoiceLoop = FindObjectOfType<StartGameVoiceLoop>();
        if (startVoiceLoop == null)
        {
            Debug.LogError("Start game voice loop not found");
            return;
        }
        _startEvent.AddListener(dispatcher.StartPresentations);
        _startEvent.AddListener(startVoiceLoop.Stop);
        
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    
    public void OnStartGame(InputAction.CallbackContext context)
    {
        if(context.started)
            _startEvent.Invoke();
    }
    
    void OnSceneUnloaded(Scene scene)
    {
        _startEvent.RemoveAllListeners();
        InputManager.Instance.DisableStartGameInput();
    }
}
