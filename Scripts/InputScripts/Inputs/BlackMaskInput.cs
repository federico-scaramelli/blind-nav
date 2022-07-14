using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace InputScripts.Inputs
{
    public class BlackMaskInput : MonoBehaviour, PlayerInputAction.IBlackMaskMapActions
    {
        private readonly UnityEvent _switchBlackMask = new UnityEvent();

        private void Awake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var maskSwitcher = FindObjectOfType<BlackMaskSwitcher>();
            if (maskSwitcher == null)
            {
                Debug.LogError("BlackMaskSwitcher not found");
                return;
            }
            _switchBlackMask.AddListener(maskSwitcher.SwitchMask);
        }
        
        public void OnBlackMask(InputAction.CallbackContext context)
        {
            if (context.performed)
                _switchBlackMask.Invoke();
        }

        void OnSceneUnloaded(Scene scene)
        {
            _switchBlackMask.RemoveAllListeners();
        }
    }
}