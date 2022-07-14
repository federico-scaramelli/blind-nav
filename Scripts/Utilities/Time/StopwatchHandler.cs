using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities.Time
{
    public class StopwatchHandler : MonoBehaviour
    {
        private Dictionary<string, Stopwatch> _stopwatches = new Dictionary<string, Stopwatch>();

        public void CreateStopwatch(string key)
        {
            if (_stopwatches.ContainsKey(key))
            {
                Debug.LogError("Stopwatch already exists.");
                return;
            }
            
            _stopwatches.Add(key, new Stopwatch());
        }
    
        public void DeleteStopwatch(string key)
        {
            if (_stopwatches.ContainsKey(key))
            {
                _stopwatches.Remove(key);
            }
            else
                Debug.LogWarning("Stopwatch " + key + " does not exist.");
        }
    
    
        public void ResumeTimer(string key)
        {
            if (_stopwatches.ContainsKey(key))
                _stopwatches[key].started = true;
            else
                Debug.LogWarning("Stopwatch " + key + " does not exist.");
        }

        public void StopTime(string key)
        {
            if (_stopwatches.ContainsKey(key))
                _stopwatches[key].started = false;
            else
                Debug.LogWarning("Stopwatch " + key + " does not exist.");
        }

        public float GetTime(string key)
        {
            if (_stopwatches.ContainsKey(key))
                return _stopwatches[key].time;
        
            Debug.LogWarning("Stopwatch " + key + " does not exist.");
            return float.PositiveInfinity;
        }

        public void ResetTime(string key)
        {
            if (_stopwatches.ContainsKey(key))
            {
                _stopwatches[key].started = false;
                _stopwatches[key].time = 0.0f;
            }
            else
                Debug.LogWarning("Stopwatch " + key + " does not exist.");
        }

        private void Update()
        {
            foreach (var stopwatch in _stopwatches
                .Select(t => t.Value)
                .Where(stopwatch => stopwatch.started))
            {
                stopwatch.time += UnityEngine.Time.deltaTime * 1000;
            }
        }
    
        public void ResetHandler()
        {
            _stopwatches.Clear();
        }
    }

    class Stopwatch
    {
        public float time;
        public bool started = false;
    }
}