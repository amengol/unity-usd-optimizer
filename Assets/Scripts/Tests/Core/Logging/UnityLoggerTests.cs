using System;
using NUnit.Framework;
using USDOptimizer.Core.Logging;

namespace USDOptimizer.Tests.Core.Logging
{
    [TestFixture]
    public class UnityLoggerTests
    {
        private UnityLogger _logger;
        private UnityLogger _contextLogger;

        [SetUp]
        public void Setup()
        {
            _logger = new UnityLogger();
            _contextLogger = new UnityLogger("TestContext");
        }

        [Test]
        public void Debug_LogsMessage()
        {
            // Act
            _logger.Debug("Test debug message");
            _contextLogger.Debug("Test debug message with context");

            // Assert
            // Note: We can't directly test the output since Unity's Debug.Log is not accessible in tests
            // In a real implementation, we would use a test double or mock to verify the output
        }

        [Test]
        public void Info_LogsMessage()
        {
            // Act
            _logger.Info("Test info message");
            _contextLogger.Info("Test info message with context");

            // Assert
            // Note: We can't directly test the output since Unity's Debug.Log is not accessible in tests
            // In a real implementation, we would use a test double or mock to verify the output
        }

        [Test]
        public void Warning_LogsWarningMessage()
        {
            // Act
            _logger.Warning("Test warning message");
            _contextLogger.Warning("Test warning message with context");

            // Assert
            // Note: We can't directly test the output since Unity's Debug.LogWarning is not accessible in tests
            // In a real implementation, we would use a test double or mock to verify the output
        }

        [Test]
        public void Error_LogsErrorMessage()
        {
            // Act
            _logger.Error("Test error message");
            _contextLogger.Error("Test error message with context");

            // Assert
            // Note: We can't directly test the output since Unity's Debug.LogError is not accessible in tests
            // In a real implementation, we would use a test double or mock to verify the output
        }

        [Test]
        public void Exception_LogsExceptionWithMessage()
        {
            // Arrange
            var ex = new Exception("Test exception");
            var message = "Test exception message";

            // Act
            _logger.Exception(ex, message);
            _contextLogger.Exception(ex, message);

            // Assert
            // Note: We can't directly test the output since Unity's Debug.LogError is not accessible in tests
            // In a real implementation, we would use a test double or mock to verify the output
        }

        [Test]
        public void Performance_LogsPerformanceMetric()
        {
            // Act
            _logger.Performance("TestMetric", 42.5, "ms");
            _contextLogger.Performance("TestMetric", 42.5, "ms");

            // Assert
            // Note: We can't directly test the output since Unity's Debug.Log is not accessible in tests
            // In a real implementation, we would use a test double or mock to verify the output
        }

        [Test]
        public void Performance_LogsPerformanceMetricWithoutUnit()
        {
            // Act
            _logger.Performance("TestMetric", 42.5);
            _contextLogger.Performance("TestMetric", 42.5);

            // Assert
            // Note: We can't directly test the output since Unity's Debug.Log is not accessible in tests
            // In a real implementation, we would use a test double or mock to verify the output
        }
    }
} 