using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utilities.Time;

namespace InputScripts.Inputs
{
    public class GameSessionControlInput : MonoBehaviour, PlayerInputAction.IGameSessionControlMapActions
    {
        //Session control
        private readonly UnityEvent _quitGame = new UnityEvent();
        private readonly UnityEvent _skipTutorial = new UnityEvent();
        private readonly UnityEvent<AudioClip, float, TimersHandler.DelegateMethod> _skipTutorialVoice = 
            new UnityEvent<AudioClip, float, TimersHandler.DelegateMethod>();


        SoundsPresentationDispatcher tutorial;

        private void Awake()
        {
            tutorial = FindObjectOfType<SoundsPresentationDispatcher>();
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetupEvents();
        }

        public void OnQuit(InputAction.CallbackContext context)
        {
            if (context.performed)
                _quitGame.Invoke();
        }

        public void OnSkipTutorial(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _skipTutorial.Invoke();
                _skipTutorialVoice.Invoke(tutorial.TutorialSkipAudioClip, 500, null);
                
                _skipTutorial.RemoveAllListeners();
                _skipTutorialVoice.RemoveAllListeners();
            }
        }
        
        private void SetupEvents()
        {
            var level = FindObjectOfType<Level>();
            _quitGame.AddListener(level.QuitLevel);

            if (tutorial)
            {
                _skipTutorial.AddListener(tutorial.SkipPresentationDispatch);
                _skipTutorialVoice.AddListener(level.Play2DVoice);
            }
        }

        void OnSceneUnloaded(Scene scene)
        {
            _quitGame.RemoveAllListeners();
            _skipTutorial.RemoveAllListeners();
        }
    }
}
