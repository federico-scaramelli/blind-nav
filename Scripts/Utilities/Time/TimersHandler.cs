using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Utilities.Time
{
    public class TimersHandler : MonoBehaviour
    {
        private Dictionary<string, Timer> _timers = new Dictionary<string, Timer>();
        public delegate void DelegateMethod();

        private DelegateMethod _currentDelegate;

        #region CREATION_AND_DELETION
        
        public bool CreateTimer(string key)
        {
            if (_timers.ContainsKey(key))
            {
                Debug.LogWarning("Timer " + key + " already exists.");
                return false;
            }
            
            _timers.Add(key, new Timer());
            return true;
        }

        //Used to generate unique timer keys and return them to the caller
        public string CreateTimerWithRandomID(string key)
        {
            int n = Random.Range(0, 10000);
            if (CreateTimer(key + n))
            {
                return key + n;
            }
            return CreateTimerWithRandomID(key);
        }
        
        public void DeleteTimer(string key)
        {
            if (_timers.ContainsKey(key))
            {
                _timers.Remove(key);
            }
            else
                Debug.LogWarning("Timer " + key + " does not exist.");
        }
        
        #endregion

        #region SET

        public void SetTimer(string key, float msTimerValue, UnityEvent eventToTrigger, bool startNow)
        {
            if (_timers.ContainsKey(key))
            {
                _timers[key].timerValue = msTimerValue;
                _timers[key].currentTimeLeftover = msTimerValue;
                _timers[key].methodToCall = null;
                _timers[key].eventToTrigger = eventToTrigger;
                if (startNow) _timers[key].isRunning = true;
            }
            else
                Debug.LogWarning("Timer " + key + " is not found.");
        }
        
        public void SetTimer(string key, float msTimerValue, DelegateMethod methodToCall, bool startNow)
        {
            if (_timers.ContainsKey(key))
            {
                _timers[key].timerValue = msTimerValue;
                _timers[key].currentTimeLeftover = msTimerValue;
                _timers[key].methodToCall = methodToCall;
                _timers[key].eventToTrigger = null;
                if (startNow) _timers[key].isRunning = true;
                if(_timers[key].debug) Debug.Log("Timer " + key + " set to " + msTimerValue + " ms.");
            }
            else
                Debug.LogWarning("Timer " + key + " is not found.");
        }

        public void SetOneShotTimer(string key, float msTimerValue, DelegateMethod methodToCall)
        {
            if (!TimerExists(key))
                CreateTimer(key);
            SetTimer(key, msTimerValue, methodToCall, true);
            _timers[key].isOneShot = true;
        }
        
        public void SetOneShotTimer(string key, float msTimerValue, UnityEvent eventToTrigger)
        {
            if (!TimerExists(key))
                CreateTimer(key);
            _timers[key].isOneShot = true;
            SetTimer(key, msTimerValue, eventToTrigger, true);
        }

        #endregion

        #region START_AND_RESUME

        public void StartTimer(string key)
        {
            if (_timers.ContainsKey(key))
            {
                if (_timers[key].timerValue == 0)
                    return;
                
                _timers[key].currentTimeLeftover = _timers[key].timerValue;
                _timers[key].isRunning = true;
            }
            else
                Debug.LogWarning("Timer " + key + " is not found.");
        }

        public void ResumeTimer(string key)
        {
            if (_timers.ContainsKey(key))
            {
                if (!IsPaused(key))
                    return;
                _timers[key].isRunning = true;
            }
            else
                Debug.LogWarning("Timer " + key + " is not found.");
        }
        
        private void RestartTimer(string key)
        {
            if (_timers.ContainsKey(key))
            {
                StopTimer(key);
                StartTimer(key);
            }
            else
                Debug.LogWarning("Timer " + key + " is not found.");
        }

        #endregion

        #region PAUSE_AND_STOP

        public void PauseTimer(string key)
        {
            if (_timers.ContainsKey(key))
            {
                if (IsRunning(key))
                    _timers[key].isRunning = false;
            }
            else
                Debug.LogWarning("Timer " + key + " is not found.");
        }

        //Stop the timer and set its leftover value to 0
        public void StopTimer(string key)
        {
            if (_timers.ContainsKey(key))
            {
                _timers[key].isRunning = false;
                _timers[key].currentTimeLeftover = 0;
            }
            else
                Debug.LogWarning("Timer " + key + " is not found.");
        }

        #endregion

        #region RESET

        public void TickAndRestartTimer(string key)
        {
            if (_timers.ContainsKey(key))
            {
                TickTimer(key); //It includes the stop
                StartTimer(key);
            }
            else
                Debug.LogWarning("Timer " + key + " is not found.");
        }
        
        //Keep the timer in the dictionary but totally reset it on an unusable state
        private void ResetTimer(string key)
        {
            if (_timers.ContainsKey(key))
            {
                _timers[key].methodToCall = null;
                _timers[key].eventToTrigger = null;
                _timers[key].timerValue = 0;
                _timers[key].currentTimeLeftover = 0;
                _timers[key].isRunning = false;   
            }
            else
                Debug.LogWarning("Timer " + key + " is not found.");
        }

        #endregion

        #region GETTERS

        public bool IsRunning(string key)
        {
            if (_timers.ContainsKey(key))
                return _timers[key].isRunning;
            
            Debug.LogWarning("Timer " + key + " is not found.");
            return false;
        }

        public bool IsPaused(string key)
        {
            if (_timers.ContainsKey(key))
            {
                if (_timers[key].debug)
                    Debug.Log(key + " -- isRunning: " + _timers[key].isRunning + "; " 
                              + _timers[key].currentTimeLeftover + "/"+ _timers[key].timerValue + ";");
                
                return !_timers[key].isRunning && //Not running
                _timers[key].currentTimeLeftover > 0; //Not stopped}
                // _timers[key].currentTimeLeftover < _timers[key].timerValue && //Already started
            }

            Debug.LogWarning("Timer " + key + " is not found.");
            return false;
        }

        public bool IsRunningOrPaused(string key)
        {
            if (_timers.ContainsKey(key))
                return IsRunning(key) || IsPaused(key);
            
            Debug.LogWarning("Timer " + key + " is not found.");
            return false;
        }

        public float GetLeftover(string key)
        {
            if (_timers.ContainsKey(key))
                return _timers[key].currentTimeLeftover;

            Debug.LogWarning("Timer " + key + " is not found.");
            return float.PositiveInfinity;
        }
        
        public bool TimerExists(string key)
        {
            return _timers.ContainsKey(key);
        }

        #endregion

        #region UPDATE_AND_TICK

        void Update()
        {
            foreach (var timer in _timers
                .Select(t => new {t.Value, t.Key})
                .Where(timer => timer.Value.isRunning))
            {
                if(timer.Value.currentTimeLeftover > 0)
                {
                    timer.Value.currentTimeLeftover -= UnityEngine.Time.deltaTime * 1000;
                    if (timer.Value.debug)
                        Debug.Log("Timer " + timer.Key + " current leftover: " + timer.Value.currentTimeLeftover);
                }
                else
                {
                    TickTimer(timer.Key);
                    return;
                }
            }
        }

        private void TickTimer(string key)
        {
            if (_timers.ContainsKey(key))
            {
                StopTimer(key);
                var isOneShot = _timers[key].isOneShot;
                
                if (_timers[key].methodToCall != null)
                {
                    _currentDelegate = _timers[key].methodToCall;
                    _currentDelegate.Invoke();
                }
                else if (_timers[key].eventToTrigger != null)
                {
                    UnityEvent eventToTrigger = _timers[key].eventToTrigger;
                    eventToTrigger.Invoke();
                }
                
                //Check needed because the timer could be deleted during the invoked methods
                if (isOneShot && TimerExists(key)) 
                    DeleteTimer(key);
            }
            else
                Debug.LogWarning("Timer " + key + " is not found.");
        }

        #endregion

        #region HANDLER

        public void ResetHandler()
        {
            _timers.Clear();
        }

        #endregion

        #region QUERY_ACTIONS
        
        public void StopAllTimersContainingString(string key)
        {
            foreach (var timer in _timers
                .Select(t => new {t.Value, t.Key})
                .Where(timer => timer.Key.Contains(key)))
            {
                StopTimer(timer.Key);
            }
        }

        public void PauseAllTimersContainingString(string key)
        {
            foreach (var timer in _timers
                .Select(t => new {t.Value, t.Key})
                .Where(timer => timer.Key.Contains(key)))
            {
                if(IsRunning(timer.Key))
                    PauseTimer(timer.Key);
            }
        }
        
        public void ResumeAllTimersContainingString(string key)
        {
            foreach (var timer in _timers
                .Select(t => new {t.Value, t.Key})
                .Where(timer => timer.Key.Contains(key)))
            {
                if(IsPaused(timer.Key))
                {
                    ResumeTimer(timer.Key);
                }
            }
        }

        #endregion

        #region DEBUG

        public void DebugTimer(string key)
        {
            if (_timers.ContainsKey(key))
                _timers[key].debug = true;
        }

        #endregion
    }

    public class Timer
    {
        public UnityEvent eventToTrigger;
        public TimersHandler.DelegateMethod methodToCall;
        
        public float timerValue;
        public float currentTimeLeftover;
        
        public bool isRunning = false;
        
        public bool isOneShot = false;
        
        public bool debug = false;
    }
}
