using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Managers;

namespace CMF
{
    public class CameraInput : MonoBehaviour
    {
        private float horizontal, vertical;

        [Range(1f, 10f)] public float mouseSensitivity;
        [Range(0.1f, 1f)] public float gamePadSensitivity;

        //Invert input options;
        public bool invertHorizontalInput = false;
        public bool invertVerticalInput = false;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public float GetHorizontalCameraInput()
        {
            float _input = horizontal;
            
            if(invertHorizontalInput)
                _input *= -1f;

            return _input;
        }

        public float GetVerticalCameraInput()
        {
            float _input = -vertical;
            
            if(invertVerticalInput)
                _input *= -1f;

            return _input;
        }

        public void SetCameraRotation(float rotation)
        {
            horizontal = InputManager.Instance.isUsingMouse ? 
                rotation * mouseSensitivity : 
                rotation * gamePadSensitivity;
            horizontal = Mathf.Clamp(horizontal, -1, 1);
        }
    }
}
