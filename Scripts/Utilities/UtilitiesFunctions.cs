using System.Reflection;
using UnityEngine;

namespace Utilities
{
    public static class UtilitiesFunctions
    {
        public static float Map(float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static Vector2 RoundV2(Vector2 v) {
            v.Set(Mathf.Round(v.x * 10f) / 10f, Mathf.Round(v.y * 10f) / 10f);
            return v;
        }

        public static Vector3 RoundV3(Vector3 v) {
            v.Set(Mathf.Round(v.x * 10f) / 10f, Mathf.Round(v.y * 10f) / 10f, Mathf.Round(v.z * 10f) / 10f);
            return v;
        }

        public static Vector3 RoundToIntV3(Vector3 v) {
            v.Set(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
            return v;
        }

        public static bool LayerInLayerMask(int layerValue, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layerValue)) > 0;
        }

        public static float AngleOfVectorOnZAxis(Vector3 targetVector) {
            return Mathf.Atan2(targetVector.y, targetVector.x) * Mathf.Rad2Deg;
        }

        public static string FormatTimeMinSec(float time) {
            int minutes = (int)(time / 60f);
            int seconds = (int)(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        public static void ClearLog() {
            #if UNITY_EDITOR
                var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                var type = assembly.GetType("UnityEditor.LogEntries");
                var method = type.GetMethod("Clear");
                method.Invoke(new object(), null);
            #endif
        }
    }
}
