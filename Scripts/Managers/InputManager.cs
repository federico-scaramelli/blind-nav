using InputScripts.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Static;

namespace Managers
{
    public class InputManager : Singleton<InputManager>
    {
        public PlayerInput playerInput;
        private PlayerInputAction playerInputAction;

        [SerializeField] private CharacterInput characterInput;
        [SerializeField] private SoundsFeedbackInput soundsFeedbackInput;
        [SerializeField] private BlackMaskInput blackMaskInput;
        [SerializeField] private VolumeControlInput volumeControlInput;
        [SerializeField] private GameSessionControlInput gameSessionControlInput;
        [SerializeField] private StartInput startInput;

        [HideInInspector] public bool isUsingMouse = true;

        private bool _characterInputPaused;
        private bool _soundsFeedbackInputPaused;
        private bool _blackMaskInputPaused;
        private bool _volumeControlInputPaused;
        private bool _gameSessionControlInputPaused;

        private void OnEnable()
        {
            playerInputAction = new PlayerInputAction();
            playerInputAction.Enable();

            if (!playerInput)
                playerInput = GetComponent<PlayerInput>();
            if (!characterInput)
                characterInput = GetComponent<CharacterInput>();
            if (!soundsFeedbackInput)
                soundsFeedbackInput = GetComponent<SoundsFeedbackInput>();
            if (!blackMaskInput)
                blackMaskInput = GetComponent<BlackMaskInput>();
            if (!volumeControlInput)
                volumeControlInput = GetComponent<VolumeControlInput>();
            if (!gameSessionControlInput)
                gameSessionControlInput = GetComponent<GameSessionControlInput>();
                
            playerInputAction.SoundsFeedbackActionMap.SetCallbacks(soundsFeedbackInput);
            playerInputAction.CharacterActionMap.SetCallbacks(characterInput);
            playerInputAction.BlackMaskMap.SetCallbacks(blackMaskInput);
            playerInputAction.VolumeControlMap.SetCallbacks(volumeControlInput);
            playerInputAction.GameSessionControlMap.SetCallbacks(gameSessionControlInput);
            playerInputAction.StartMap.SetCallbacks(startInput);

            EnableStartGameInput();

            DisableInputs();
            EnableBlackMaskInput();
            EnableGameSessionInput();
        }

        public void EnableGameplayInput()
        {
            EnableVolumeControlInput();
            EnableCharacterInput();
        }

        private void DisableInputs()
        {
            DisableSoundsFeedbackInput();
            DisableCharacterInput();
            DisableVolumeControlInput();
        }
        
        public void PauseInputs()
        {
            if (playerInputAction.SoundsFeedbackActionMap.enabled)
            {
                DisableSoundsFeedbackInput();
                _soundsFeedbackInputPaused = true;
            }
            if (playerInputAction.CharacterActionMap.enabled)
            {
                DisableCharacterInput();
                _characterInputPaused = true;
            }
            if (playerInputAction.GameSessionControlMap.enabled)
            {
                DisableVolumeControlInput();
                _volumeControlInputPaused = true;
            }
        }

        public void ResumeInputs()
        {
            if (_soundsFeedbackInputPaused)
            {
                EnableSoundsFeedbackInput();
                _soundsFeedbackInputPaused = false;
            }
            if (_characterInputPaused)
            {
                EnableCharacterInput();
                _characterInputPaused = false;
            }
            if (_volumeControlInputPaused)
            {
                EnableVolumeControlInput();
                _volumeControlInputPaused = false;
            }
        }
        
        public void EnableStartGameInput()
        {
            playerInputAction.StartMap.Enable();
        }
        
        public void DisableStartGameInput()
        {
            playerInputAction.StartMap.Disable();
        }

        public void EnableGameSessionInput()
        {
            playerInputAction.GameSessionControlMap.Enable();
        }
        
        public void DisableGameSessionInput()
        {
            playerInputAction.GameSessionControlMap.Disable();
        }

        public void EnableSoundsFeedbackInput()
        {
            playerInputAction.SoundsFeedbackActionMap.Enable();
        }
        
        public void DisableSoundsFeedbackInput()
        {
            playerInputAction.SoundsFeedbackActionMap.Disable();
        }
        
        public void EnableCharacterInput()
        {
            playerInputAction.CharacterActionMap.Enable();
        }
        
        public void DisableCharacterInput()
        {
            playerInputAction.CharacterActionMap.Disable();
        }

        private void EnableBlackMaskInput()
        {
            playerInputAction.BlackMaskMap.Enable();
        }
        
        private void DisableBlackMaskInput()
        {
            playerInputAction.BlackMaskMap.Disable();
        }
        
        public void EnableVolumeControlInput()
        {
            playerInputAction.VolumeControlMap.Enable();
        }
        
        public void DisableVolumeControlInput()
        {
            playerInputAction.VolumeControlMap.Disable();
        }
    }
}