using UnityEngine;

namespace USDOptimizer.Core.Logging
{
    public class UnityLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Debug.Log($"[USD Scene Optimizer] {message}");
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning($"[USD Scene Optimizer] {message}");
        }

        public void LogError(string message)
        {
            Debug.LogError($"[USD Scene Optimizer] {message}");
        }

        public void LogDebug(string message)
        {
            #if UNITY_EDITOR
            Debug.Log($"[USD Scene Optimizer Debug] {message}");
            #endif
        }
    }
} 