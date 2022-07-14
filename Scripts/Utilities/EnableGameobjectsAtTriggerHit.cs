using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utilities.CustomAttributes;

namespace Utilities
{
    [RequireComponent(typeof(Collider))] [ExecuteInEditMode]
    public class EnableGameobjectsAtTriggerHit : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField]  private List<Tag> tags;
    
        [SerializeField] private GameObject[] gameObjectsToEnable;
   
    #if UNITY_EDITOR

        private void Awake()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }
    
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnSceneGUI(SceneView obj)
        {
            tags = tags ??= new List<Tag>();
            if (tags.Count > 0) return;
        
            foreach (var t in UnityEditorInternal.InternalEditorUtility.tags)
            {
                tags.Add(new Tag(t, false));
            }
        }
    
        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        
    #endif

        private void OnTriggerEnter(Collider other)
        {
            /*if (!UtilitiesFunctions.LayerInLayerMask(other.gameObject.layer, layerMask) &&
                !other.gameObject.CompareTag(tag)) return;*/
            
            if (!other.gameObject.CompareTag("Player")) return;
        
            foreach (var go in gameObjectsToEnable)
                go.SetActive(true);
            
            gameObject.SetActive(false);
        }
    }

    [Serializable]
    public struct Tag
    {
        [ReadOnly] public string tag;
        public bool checkCollision;

        public Tag(string t, bool b)
        {
            tag = t;
            checkCollision = b;
        }
    }
}