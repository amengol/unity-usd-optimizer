using System;
using UnityEngine;

namespace USDOptimizer.Core.Logging
{
    public class UnityLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Debug.Log($"[USD Optimizer] {message}");
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning($"[USD Optimizer] {message}");
        }

        public void LogError(string message)
        {
            Debug.LogError($"[USD Optimizer] {message}");
        }

        public void LogException(Exception ex)
        {
            Debug.LogException(ex);
        }

        public void LogDebug(string message)
        {
            #if UNITY_EDITOR
            Debug.Log($"[USD Scene Optimizer Debug] {message}");
            #endif
        }
    }
} 