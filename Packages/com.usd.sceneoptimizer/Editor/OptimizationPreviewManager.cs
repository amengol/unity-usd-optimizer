using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization;
using USDOptimizer.Core.Logging;

namespace USDOptimizer.Unity.Editor
{
    public class OptimizationPreviewManager
    {
        private readonly SceneOptimizer _sceneOptimizer;
        private readonly USDOptimizer.Core.Logging.ILogger _logger;
        private USDScene _previewScene;
        private bool _isPreviewActive;
        private float _previewProgress;

        public event Action<float> OnPreviewProgressChanged;
        public event Action<USDScene> OnPreviewUpdated;
        public event Action OnPreviewCompleted;
        public event Action<Exception> OnPreviewError;

        public bool IsPreviewActive => _isPreviewActive;

        public OptimizationPreviewManager(USDOptimizer.Core.Logging.ILogger logger = null)
        {
            _sceneOptimizer = new SceneOptimizer(logger);
            _logger = logger ?? new UnityLogger();
        }

        public async Task CreateSceneCopyAsync(USDScene originalScene)
        {
            try
            {
                _isPreviewActive = true;
                _previewProgress = 0f;
                OnPreviewProgressChanged?.Invoke(_previewProgress);

                // Create a deep copy of the scene
                _previewScene = new USDScene
                {
                    FilePath = originalScene.FilePath,
                    Name = $"{originalScene.Name}_Preview",
                    ImportDate = DateTime.Now
                };

                // Copy nodes
                foreach (var node in originalScene.Nodes)
                {
                    _previewScene.Nodes.Add(CopyNode(node));
                }

                // Copy statistics
                _previewScene.Statistics = new SceneStatistics
                {
                    TotalNodes = originalScene.Statistics.TotalNodes,
                    TotalPolygons = originalScene.Statistics.TotalPolygons,
                    TotalVertices = originalScene.Statistics.TotalVertices,
                    TotalMaterials = originalScene.Statistics.TotalMaterials,
                    TotalTextures = originalScene.Statistics.TotalTextures,
                    TotalFileSize = originalScene.Statistics.TotalFileSize,
                    NodeTypeCounts = new Dictionary<string, int>(originalScene.Statistics.NodeTypeCounts)
                };

                // Simulate async work
                await Task.Delay(100);

                _previewProgress = 1f;
                OnPreviewProgressChanged?.Invoke(_previewProgress);
                OnPreviewUpdated?.Invoke(_previewScene);
                OnPreviewCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating scene copy: {ex.Message}");
                OnPreviewError?.Invoke(ex);
            }
            finally
            {
                _isPreviewActive = false;
                _previewProgress = 0f;
            }
        }

        public async Task UpdatePreviewAsync(SceneOptimizationSettings settings)
        {
            if (_previewScene == null)
            {
                throw new InvalidOperationException("No preview scene available. Call CreateSceneCopyAsync first.");
            }

            try
            {
                _isPreviewActive = true;
                _previewProgress = 0f;
                OnPreviewProgressChanged?.Invoke(_previewProgress);

                // Apply optimization settings to preview scene
                var optimizedScene = await _sceneOptimizer.OptimizeSceneAsync(_previewScene, settings);

                // Update preview scene with optimized version
                _previewScene = optimizedScene;

                _previewProgress = 1f;
                OnPreviewProgressChanged?.Invoke(_previewProgress);
                OnPreviewUpdated?.Invoke(_previewScene);
                OnPreviewCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating preview: {ex.Message}");
                OnPreviewError?.Invoke(ex);
            }
            finally
            {
                _isPreviewActive = false;
                _previewProgress = 0f;
            }
        }

        public void CancelPreview()
        {
            _isPreviewActive = false;
            _previewProgress = 0f;
            OnPreviewProgressChanged?.Invoke(_previewProgress);
        }

        private USDNode CopyNode(USDNode originalNode)
        {
            var newNode = new USDNode
            {
                Name = originalNode.Name,
                Type = originalNode.Type,
                Properties = new Dictionary<string, object>(originalNode.Properties)
            };

            foreach (var child in originalNode.Children)
            {
                newNode.Children.Add(CopyNode(child));
            }

            return newNode;
        }
    }
} 