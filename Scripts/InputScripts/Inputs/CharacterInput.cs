using System;
using CMF;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

namespace InputScripts.Inputs
{
    public class CharacterInput : MonoBehaviour, PlayerInputAction.ICharacterActionMapActions
    {
        private readonly UnityEvent<Vector2> _moveEvent = new UnityEvent<Vector2>();
        private readonly UnityEvent<float> _rotateEvent = new UnityEvent<float>();
        private readonly UnityEvent _resetToTarget = new UnityEvent();

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            InputUser.onChange += OnInputDeviceChange;
        }
        
        private void OnInputDeviceChange(InputUser user, InputUserChange change, InputDevice device) {
            if (change == InputUserChange.ControlSchemeChanged)
                InputManager.Instance.isUsingMouse = user.controlScheme.Value.name.Equals("M&K");
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var movementController = FindObjectOfType<CharacterMovementInput>();
            var rotationController = FindObjectOfType<CameraInput>();
            if (movementController == null)
            {
                Debug.LogError("Character movement input handler not found");
                return;
            }
            if (rotationController == null)
            {
                Debug.LogError("Character rotation input handler not found");
                return;
            }
            
            _moveEvent.AddListener(movementController.SetMovementInput);
            _rotateEvent.AddListener(rotationController.SetCameraRotation);
            
            var player = FindObjectOfType<PlayerLevelInteraction>();
            var level = FindObjectOfType<Level>();
            _resetToTarget.AddListener(player.ManualReset);
            _resetToTarget.AddListener(level.ManualTargetReset);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 movement = context.ReadValue<Vector2>();
            _moveEvent.Invoke(movement);
        }

        public void OnRotate(InputAction.CallbackContext context)
        {
            Vector2 rotation = context.ReadValue<Vector2>();
            var x = InputManager.Instance.isUsingMouse ? rotation.x * Time.smoothDeltaTime : rotation.x;
            _rotateEvent.Invoke(x);
        }
        
        public void OnResetToTarget(InputAction.CallbackContext context)
        {
            if (context.performed)
                _resetToTarget.Invoke();
        }

        void OnSceneUnloaded(Scene scene)
        {
            _moveEvent.RemoveAllListeners();
            _rotateEvent.RemoveAllListeners();
            _resetToTarget.RemoveAllListeners();
        }
    }
}