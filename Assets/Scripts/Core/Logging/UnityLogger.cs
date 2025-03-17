using System;
using UnityEngine;

namespace USDOptimizer.Core.Logging
{
    /// <summary>
    /// Implementation of ILogger using Unity's logging system
    /// </summary>
    public class UnityLogger : ILogger
    {
        private readonly string _context;

        public UnityLogger(string context = null)
        {
            _context = context;
        }

        public void Debug(string message)
        {
            if (string.IsNullOrEmpty(_context))
                Debug.Log(message);
            else
                Debug.Log($"[{_context}] {message}");
        }

        public void Info(string message)
        {
            if (string.IsNullOrEmpty(_context))
                Debug.Log(message);
            else
                Debug.Log($"[{_context}] {message}");
        }

        public void Warning(string message)
        {
            if (string.IsNullOrEmpty(_context))
                Debug.LogWarning(message);
            else
                Debug.LogWarning($"[{_context}] {message}");
        }

        public void Error(string message)
        {
            if (string.IsNullOrEmpty(_context))
                Debug.LogError(message);
            else
                Debug.LogError($"[{_context}] {message}");
        }

        public void Exception(Exception ex, string message = null)
        {
            var fullMessage = message != null ? $"{message}\n{ex}" : ex.ToString();
            if (string.IsNullOrEmpty(_context))
                Debug.LogError(fullMessage);
            else
                Debug.LogError($"[{_context}] {fullMessage}");
        }

        public void Performance(string metricName, double value, string unit = null)
        {
            var unitStr = unit != null ? $" {unit}" : string.Empty;
            if (string.IsNullOrEmpty(_context))
                Debug.Log($"[Performance] {metricName}: {value}{unitStr}");
            else
                Debug.Log($"[{_context}] [Performance] {metricName}: {value}{unitStr}");
        }
    }
} 