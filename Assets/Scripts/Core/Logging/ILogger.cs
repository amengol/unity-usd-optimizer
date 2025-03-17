using System;

namespace USDOptimizer.Core.Logging
{
    /// <summary>
    /// Interface for logging operations
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log a debug message
        /// </summary>
        void Debug(string message);

        /// <summary>
        /// Log an info message
        /// </summary>
        void Info(string message);

        /// <summary>
        /// Log a warning message
        /// </summary>
        void Warning(string message);

        /// <summary>
        /// Log an error message
        /// </summary>
        void Error(string message);

        /// <summary>
        /// Log an exception
        /// </summary>
        void Exception(Exception ex, string message = null);

        /// <summary>
        /// Log a performance metric
        /// </summary>
        void Performance(string metricName, double value, string unit = null);
    }
} 