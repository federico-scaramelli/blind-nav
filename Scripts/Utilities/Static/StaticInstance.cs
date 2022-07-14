using System;
using UnityEngine;

namespace Utilities.Static
{
    public class StaticInstance<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<T>();
                    if (!_instance)
                        throw new Exception("Static instance of type " + nameof(T) + "not found.");
                }

                return _instance;
            }
        }
        
        public virtual void Awake()
        {
            if (!_instance)
            {
                _instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}

