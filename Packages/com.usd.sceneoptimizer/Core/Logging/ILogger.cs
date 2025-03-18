using System;

namespace USDOptimizer.Core.Logging
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogException(Exception ex);
    }
} 