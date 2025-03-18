using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization;
using USDOptimizer.Core.IO;
using USDOptimizer.Core.Batch;
using USDOptimizer.Core.Logging;

namespace USDOptimizer.Tests.Core.Optimization
{
    [TestFixture]
    public class BatchProcessorTests
    {
        private BatchProcessor _batchProcessor;
        private USDSceneIO _mockSceneIO;
        private SceneOptimizer _mockSceneOptimizer;
        private ILogger _mockLogger;
        private List<string> _testScenePaths;
        private OptimizationProfile _testProfile;
        private USDScene _testScene;
        private USDScene _optimizedScene;

        [SetUp]
        public void Setup()
        {
            // Create mocks
            _mockSceneIO = Substitute.For<USDSceneIO>();
            _mockSceneOptimizer = Substitute.For<SceneOptimizer>();
            _mockLogger = Substitute.For<ILogger>();

            // Initialize batch processor with mocks
            _batchProcessor = new BatchProcessor(_mockSceneIO, _mockSceneOptimizer, _mockLogger);

            // Create test data
            _testScenePaths = new List<string> { "scene1.usd", "scene2.usd", "scene3.usd" };
            
            _testProfile = new OptimizationProfile
            {
                Name = "TestProfile",
                Description = "Test Profile Description",
                Settings = new SceneOptimizationSettings()
            };

            // Setup test scenes
            _testScene = new USDScene
            {
                Name = "TestScene",
                FilePath = "scene1.usd",
                Statistics = new SceneStatistics
                {
                    TotalPolygons = 1000,
                    TotalVertices = 2000,
                    TotalMaterials = 10,
                    TotalFileSize = 1024
                }
            };

            _optimizedScene = new USDScene
            {
                Name = "OptimizedScene",
                FilePath = "scene1_optimized.usd",
                Statistics = new SceneStatistics
                {
                    TotalPolygons = 500,
                    TotalVertices = 1000,
                    TotalMaterials = 5,
                    TotalFileSize = 512
                }
            };

            // Setup mock behavior
            _mockSceneIO.ImportSceneAsync(Arg.Any<string>()).Returns(Task.FromResult(_testScene));
            _mockSceneOptimizer.OptimizeSceneAsync(Arg.Any<USDScene>(), Arg.Any<SceneOptimizationSettings>())
                .Returns(Task.FromResult(_optimizedScene));
        }

        [Test]
        public async Task ProcessBatchAsync_ValidInput_ProcessesAllScenes()
        {
            // Arrange
            bool batchCompleted = false;
            _batchProcessor.OnBatchCompleted += () => batchCompleted = true;

            // Act
            await _batchProcessor.ProcessBatchAsync(_testScenePaths, _testProfile);

            // Assert
            Assert.That(batchCompleted, Is.True, "Batch completed event should be triggered");
            
            // Verify each scene was processed
            await _mockSceneIO.Received(_testScenePaths.Count)
                .ImportSceneAsync(Arg.Any<string>());
            
            await _mockSceneOptimizer.Received(_testScenePaths.Count)
                .OptimizeSceneAsync(Arg.Any<USDScene>(), Arg.Any<SceneOptimizationSettings>());
            
            await _mockSceneIO.Received(_testScenePaths.Count)
                .ExportSceneAsync(Arg.Any<string>(), Arg.Any<USDScene>());
        }

        [Test]
        public void ProcessBatchAsync_EmptySceneList_ThrowsArgumentException()
        {
            // Arrange
            var emptyList = new List<string>();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => 
                await _batchProcessor.ProcessBatchAsync(emptyList, _testProfile));
        }

        [Test]
        public void ProcessBatchAsync_NullProfile_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _batchProcessor.ProcessBatchAsync(_testScenePaths, null));
        }

        [Test]
        public async Task ProcessBatchAsync_ImportError_ContinuesWithNextScene()
        {
            // Arrange
            _mockSceneIO.ImportSceneAsync("scene1.usd")
                .Returns(Task.FromException<USDScene>(new Exception("Import error")));
            
            bool batchCompleted = false;
            _batchProcessor.OnBatchCompleted += () => batchCompleted = true;

            // Act
            await _batchProcessor.ProcessBatchAsync(_testScenePaths, _testProfile);

            // Assert
            Assert.That(batchCompleted, Is.True, "Batch should complete even with errors");
            
            // Should still process the remaining scenes
            await _mockSceneIO.Received(1).ImportSceneAsync("scene2.usd");
            await _mockSceneIO.Received(1).ImportSceneAsync("scene3.usd");
        }

        [Test]
        public async Task CancelBatch_DuringProcessing_StopsBatchProcessing()
        {
            // Arrange
            var longDelay = Task.Delay(1000);
            _mockSceneIO.ImportSceneAsync("scene2.usd").Returns(async _ => 
            {
                await longDelay;
                return _testScene;
            });

            bool batchCompleted = false;
            _batchProcessor.OnBatchCompleted += () => batchCompleted = true;

            // Act
            var processingTask = _batchProcessor.ProcessBatchAsync(_testScenePaths, _testProfile);
            _batchProcessor.CancelBatch();
            await processingTask;

            // Assert
            Assert.That(batchCompleted, Is.False, "Batch should not complete when cancelled");
            
            // Should not process scene3 after cancellation
            await _mockSceneIO.DidNotReceive().ImportSceneAsync("scene3.usd");
        }

        [Test]
        public async Task ProcessBatchAsync_ProgressEvents_ReportsCorrectProgress()
        {
            // Arrange
            List<float> progressValues = new List<float>();
            _batchProcessor.OnProgressChanged += (progress) => progressValues.Add(progress);

            // Act
            await _batchProcessor.ProcessBatchAsync(_testScenePaths, _testProfile);

            // Assert
            Assert.That(progressValues.Count, Is.GreaterThan(0), "Should report progress");
            Assert.That(progressValues[progressValues.Count - 1], Is.EqualTo(1.0f), "Final progress should be 100%");
        }
    }
} 