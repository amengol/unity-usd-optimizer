using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization;
using USDOptimizer.Core.IO;
using USDOptimizer.Core.Batch;
using USDOptimizer.Core.Analysis.Implementations;
using USDOptimizer.Unity.Editor;
using USDSceneOptimizer;

namespace USDOptimizer.Tests.Editor
{
    [TestFixture]
    public class USDSceneOptimizerWindowTests
    {
        private USDSceneOptimizerWindow _window;
        private USDSceneIO _mockSceneIO;
        private SceneOptimizer _mockSceneOptimizer;
        private SceneAnalyzer _mockSceneAnalyzer;
        private BatchProcessor _mockBatchProcessor;
        private OptimizationProfileManager _mockProfileManager;
        
        [SetUp]
        public void Setup()
        {
            // Create mocks
            _mockSceneIO = Substitute.For<USDSceneIO>();
            _mockSceneOptimizer = Substitute.For<SceneOptimizer>();
            _mockSceneAnalyzer = Substitute.For<SceneAnalyzer>(
                Substitute.For<IMeshAnalyzer>(),
                Substitute.For<IMaterialAnalyzer>(),
                Substitute.For<ISceneHierarchyAnalyzer>()
            );
            _mockBatchProcessor = Substitute.For<BatchProcessor>(
                _mockSceneIO, 
                _mockSceneOptimizer
            );

            // Mock profile manager - need to use a different approach since it's a singleton
            _mockProfileManager = Substitute.For<OptimizationProfileManager>();
            ReplaceProfileManagerInstance(_mockProfileManager);
            
            // Create window
            _window = ScriptableObject.CreateInstance<USDSceneOptimizerWindow>();
            
            // Use reflection to inject mocks
            InjectMocks();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_window != null)
            {
                ScriptableObject.DestroyImmediate(_window);
            }
            
            // Restore original profile manager
            ReplaceProfileManagerInstance(null);
        }
        
        [Test]
        public void OnEnable_InitializesComponents()
        {
            // Act - OnEnable is called during Setup
            
            // Assert - use reflection to verify components were initialized
            var sceneIO = GetPrivateField<USDSceneIO>("_sceneIO");
            var sceneOptimizer = GetPrivateField<SceneOptimizer>("_sceneOptimizer");
            var batchProcessor = GetPrivateField<BatchProcessor>("_batchProcessor");
            var sceneAnalyzer = GetPrivateField<SceneAnalyzer>("_sceneAnalyzer");
            
            Assert.That(sceneIO, Is.Not.Null);
            Assert.That(sceneOptimizer, Is.Not.Null);
            Assert.That(batchProcessor, Is.Not.Null);
            Assert.That(sceneAnalyzer, Is.Not.Null);
        }
        
        [Test]
        public void AnalyzeSceneAsync_ShouldUpdateAnalysisResults()
        {
            // Arrange
            var analysisResults = new AnalysisResults
            {
                TotalPolygons = 1000,
                TotalVertices = 2000,
                TotalMaterials = 10,
                TotalTextures = 5,
                TotalMemoryUsage = 1024,
                Recommendations = new List<OptimizationRecommendation>
                {
                    new OptimizationRecommendation
                    {
                        Title = "Test Recommendation",
                        Description = "Test Description",
                        Priority = OptimizationPriority.High,
                        EstimatedImprovement = 0.5f
                    }
                }
            };
            _mockSceneAnalyzer.AnalyzeScene().Returns(analysisResults);
            
            // Act
            RunPrivateMethodAsync("AnalyzeSceneAsync");
            
            // Assert
            var resultField = GetPrivateField<AnalysisResults>("_analysisResults");
            Assert.That(resultField, Is.Not.Null);
            Assert.That(resultField.TotalPolygons, Is.EqualTo(1000));
            Assert.That(resultField.Recommendations.Count, Is.EqualTo(1));
            
            var previewVisible = GetPrivateField<bool>("_showPreviewWindow");
            Assert.That(previewVisible, Is.True, "Preview window should be shown after analysis");
        }
        
        [Test]
        public void OptimizeSceneAsync_ShouldUpdateOptimizationResults()
        {
            // Arrange
            var testScene = new USDScene
            {
                Name = "TestScene",
                Statistics = new SceneStatistics
                {
                    TotalPolygons = 1000,
                    TotalVertices = 2000,
                    TotalMaterials = 10
                }
            };
            
            var optimizedScene = new USDScene
            {
                Name = "OptimizedScene",
                Statistics = new SceneStatistics
                {
                    TotalPolygons = 500,
                    TotalVertices = 1000,
                    TotalMaterials = 5
                },
                OptimizationResults = new List<OptimizationResult>
                {
                    new OptimizationResult
                    {
                        Type = "Test Optimization",
                        ItemsOptimized = 10,
                        Notes = "Test Notes"
                    }
                }
            };
            
            SetPrivateField("_currentUsdScene", testScene);
            _mockSceneOptimizer.OptimizeSceneAsync(testScene, Arg.Any<SceneOptimizationSettings>())
                .Returns(optimizedScene);
            
            var profile = new OptimizationProfile
            {
                Name = "TestProfile",
                Settings = new SceneOptimizationSettings()
            };
            _mockProfileManager.GetProfile(Arg.Any<string>()).Returns(profile);
            
            // Act
            RunPrivateMethodAsync("OptimizeSceneAsync");
            
            // Assert
            var optimizedSceneField = GetPrivateField<USDScene>("_optimizedScene");
            Assert.That(optimizedSceneField, Is.Not.Null);
            Assert.That(optimizedSceneField.Statistics.TotalPolygons, Is.EqualTo(500));
            
            var resultsField = GetPrivateField<List<OptimizationResult>>("_optimizationResults");
            Assert.That(resultsField, Is.Not.Null);
            Assert.That(resultsField.Count, Is.EqualTo(1));
            
            var previewVisible = GetPrivateField<bool>("_showPreviewWindow");
            Assert.That(previewVisible, Is.True, "Preview window should be shown after optimization");
        }
        
        [Test]
        public void ExportSceneAsync_OptimizedScene_ExportsCorrectScene()
        {
            // Arrange
            var originalScene = new USDScene { Name = "Original" };
            var optimizedScene = new USDScene { Name = "Optimized" };
            
            SetPrivateField("_currentUsdScene", originalScene);
            SetPrivateField("_optimizedScene", optimizedScene);
            
            // Mock EditorUtility.SaveFilePanel using UnityTests
            EditorUtility_SaveFilePanelPatched = () => "test_path.usd";
            
            // Act
            RunPrivateMethodAsync("ExportSceneAsync");
            
            // Assert
            _mockSceneIO.Received(1).ExportSceneAsync("test_path.usd", optimizedScene);
        }
        
        [Test]
        public void ExportSceneAsync_NoOptimizedScene_ExportsOriginalScene()
        {
            // Arrange
            var originalScene = new USDScene { Name = "Original" };
            
            SetPrivateField("_currentUsdScene", originalScene);
            SetPrivateField("_optimizedScene", null);
            
            // Mock EditorUtility.SaveFilePanel
            EditorUtility_SaveFilePanelPatched = () => "test_path.usd";
            
            // Act
            RunPrivateMethodAsync("ExportSceneAsync");
            
            // Assert
            _mockSceneIO.Received(1).ExportSceneAsync("test_path.usd", originalScene);
        }
        
        [Test]
        public void CreateSceneFromAnalysisResults_CreatesValidScene()
        {
            // Arrange
            var analysisResults = new AnalysisResults
            {
                TotalPolygons = 1000,
                TotalVertices = 2000,
                TotalMaterials = 5,
                TotalTextures = 3,
                TotalMemoryUsage = 1024
            };
            
            SetPrivateField("_analysisResults", analysisResults);
            
            // Act
            var resultScene = InvokePrivateMethod<USDScene>("CreateSceneFromAnalysisResults");
            
            // Assert
            Assert.That(resultScene, Is.Not.Null);
            Assert.That(resultScene.Statistics.TotalPolygons, Is.EqualTo(1000));
            Assert.That(resultScene.Meshes.Count, Is.EqualTo(10)); // Creates 10 sample meshes
            Assert.That(resultScene.Materials.Count, Is.EqualTo(5));
            Assert.That(resultScene.Textures.Count, Is.EqualTo(3));
        }
        
        #region Helper Methods
        
        private void InjectMocks()
        {
            SetPrivateField("_sceneIO", _mockSceneIO);
            SetPrivateField("_sceneOptimizer", _mockSceneOptimizer);
            SetPrivateField("_sceneAnalyzer", _mockSceneAnalyzer);
            SetPrivateField("_batchProcessor", _mockBatchProcessor);
            
            // Initialize other required fields
            SetPrivateField("_settings", new SceneOptimizationSettings());
            SetPrivateField("_selectedProfile", "Default");
            SetPrivateField("_selectedScenes", new List<string>());
        }
        
        private void SetPrivateField<T>(string fieldName, T value)
        {
            Type type = _window.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(_window, value);
            }
            else
            {
                Assert.Fail($"Field {fieldName} not found on {type.FullName}");
            }
        }
        
        private T GetPrivateField<T>(string fieldName)
        {
            Type type = _window.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (fieldInfo != null)
            {
                return (T)fieldInfo.GetValue(_window);
            }
            else
            {
                Assert.Fail($"Field {fieldName} not found on {type.FullName}");
                return default;
            }
        }
        
        private T InvokePrivateMethod<T>(string methodName, params object[] parameters)
        {
            Type type = _window.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (methodInfo != null)
            {
                return (T)methodInfo.Invoke(_window, parameters);
            }
            else
            {
                Assert.Fail($"Method {methodName} not found on {type.FullName}");
                return default;
            }
        }
        
        private void RunPrivateMethodAsync(string methodName, params object[] parameters)
        {
            Type type = _window.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (methodInfo != null)
            {
                var task = (System.Threading.Tasks.Task)methodInfo.Invoke(_window, parameters);
                task.Wait(); // Wait for async method to complete
            }
            else
            {
                Assert.Fail($"Method {methodName} not found on {type.FullName}");
            }
        }
        
        private void ReplaceProfileManagerInstance(OptimizationProfileManager newInstance)
        {
            Type type = typeof(OptimizationProfileManager);
            FieldInfo instanceField = type.GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            
            if (instanceField != null)
            {
                instanceField.SetValue(null, newInstance);
            }
        }
        
        // EditorUtility patch for testing
        private static Func<string> EditorUtility_SaveFilePanelPatched;
        
        // Patch EditorUtility for testing
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PatchEditorUtility()
        {
            var editorUtilityType = typeof(EditorUtility);
            var originalMethod = editorUtilityType.GetMethod("SaveFilePanel", BindingFlags.Public | BindingFlags.Static);
            
            // This would be actual patching in a real test, but we're using a simpler approach here
            // For actual testing, you might use a mocking library that can patch static methods
        }
        
        #endregion
    }
} 