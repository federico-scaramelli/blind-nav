using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace InputScripts.Inputs
{
    public class SoundsFeedbackInput : MonoBehaviour, PlayerInputAction.ISoundsFeedbackActionMapActions
    {
        private readonly UnityEvent _repeatSoundEvent = new UnityEvent();
        private readonly UnityEvent _confirmSoundEvent = new UnityEvent();

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var soundDispatcher = FindObjectOfType<SoundsPresentationDispatcher>();
            if (!soundDispatcher)
            {
                // Debug.Log("Dispatcher not found!");
                return;
            }
            _repeatSoundEvent.AddListener(soundDispatcher.RepeatCurrentPresentation);
            _confirmSoundEvent.AddListener(soundDispatcher.WaitForNextPresentation);
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (context.started)
                _confirmSoundEvent.Invoke();
        }

        public void OnRepeat(InputAction.CallbackContext context)
        {
            if (context.started)
                _repeatSoundEvent.Invoke();
        }

        void OnSceneUnloaded(Scene scene)
        {
            _repeatSoundEvent.RemoveAllListeners();
            _confirmSoundEvent.RemoveAllListeners();
        }
    }
}