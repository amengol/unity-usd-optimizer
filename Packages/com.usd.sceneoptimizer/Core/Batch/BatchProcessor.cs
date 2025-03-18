using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization;
using USDOptimizer.Core.IO;
using USDOptimizer.Core.Logging;

namespace USDOptimizer.Core.Batch
{
    public class BatchProcessor
    {
        private readonly USDSceneIO _sceneIO;
        private readonly SceneOptimizer _sceneOptimizer;
        private readonly ILogger _logger;
        private bool _isProcessing;
        private bool _isCancelled;

        public event Action<float> OnProgressChanged;
        public event Action<string> OnSceneProcessed;
        public event Action OnBatchCompleted;
        public event Action<Exception> OnBatchError;

        public bool IsProcessing => _isProcessing;

        public BatchProcessor(USDSceneIO sceneIO, SceneOptimizer sceneOptimizer, ILogger logger = null)
        {
            _sceneIO = sceneIO ?? throw new ArgumentNullException(nameof(sceneIO));
            _sceneOptimizer = sceneOptimizer ?? throw new ArgumentNullException(nameof(sceneOptimizer));
            _logger = logger ?? new UnityLogger();
        }

        public List<string> ShowFolderDialog()
        {
            string folderPath = EditorUtility.OpenFolderPanel("Select USD Scenes Folder", "", "");
            var scenePaths = new List<string>();
            
            if (!string.IsNullOrEmpty(folderPath))
            {
                try
                {
                    // Find all USD files in the selected folder
                    string[] usdFiles = Directory.GetFiles(folderPath, "*.usd", SearchOption.AllDirectories);
                    string[] usdaFiles = Directory.GetFiles(folderPath, "*.usda", SearchOption.AllDirectories);
                    string[] usdcFiles = Directory.GetFiles(folderPath, "*.usdc", SearchOption.AllDirectories);
                    
                    // Combine results
                    scenePaths.AddRange(usdFiles);
                    scenePaths.AddRange(usdaFiles);
                    scenePaths.AddRange(usdcFiles);
                    
                    _logger.LogInfo($"Found {scenePaths.Count} USD files in {folderPath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error listing USD files: {ex.Message}");
                    EditorUtility.DisplayDialog("Error", $"Failed to list USD files: {ex.Message}", "OK");
                }
            }
            
            return scenePaths;
        }

        public async Task ProcessBatchAsync(List<string> scenePaths, OptimizationProfile profile)
        {
            try
            {
                _isProcessing = true;
                _isCancelled = false;

                if (scenePaths == null || !scenePaths.Any())
                {
                    throw new ArgumentException("No scenes selected for batch processing");
                }

                if (profile == null)
                {
                    throw new ArgumentNullException(nameof(profile), "Optimization profile cannot be null");
                }

                // Apply profile settings
                var settings = new SceneOptimizationSettings();
                profile.ApplyToSettings(settings);

                // Process each scene
                int totalScenes = scenePaths.Count;
                int processedScenes = 0;

                foreach (string scenePath in scenePaths)
                {
                    try
                    {
                        if (_isCancelled)
                        {
                            _logger.LogInfo("Batch processing cancelled");
                            break;
                        }

                        string sceneName = Path.GetFileName(scenePath);
                        OnSceneProcessed?.Invoke(sceneName);
                        _logger.LogInfo($"Processing scene: {sceneName}");

                        // Update progress
                        float progress = (float)processedScenes / totalScenes;
                        OnProgressChanged?.Invoke(progress);

                        // Import scene
                        var scene = await _sceneIO.ImportSceneAsync(scenePath);

                        // Optimize scene
                        var optimizedScene = await _sceneOptimizer.OptimizeSceneAsync(scene, settings);

                        // Generate output path
                        string outputDir = Path.Combine(Path.GetDirectoryName(scenePath), "Optimized");
                        Directory.CreateDirectory(outputDir);
                        string outputPath = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(scenePath)}_optimized{Path.GetExtension(scenePath)}");

                        // Export optimized scene
                        await _sceneIO.ExportSceneAsync(outputPath, optimizedScene);

                        // Log results
                        float polyReduction = 1.0f - ((float)optimizedScene.Statistics.TotalPolygons / scene.Statistics.TotalPolygons);
                        float sizeReduction = 1.0f - (optimizedScene.Statistics.TotalFileSize / scene.Statistics.TotalFileSize);
                        _logger.LogInfo($"Optimized {sceneName}: Polygon reduction: {polyReduction:P0}, Size reduction: {sizeReduction:P0}");

                        // Update processed count
                        processedScenes++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing scene {Path.GetFileName(scenePath)}: {ex.Message}");
                        // Continue with next scene instead of stopping the batch
                    }
                }

                // Update final progress
                OnProgressChanged?.Invoke(1.0f);

                if (!_isCancelled)
                {
                    _logger.LogInfo($"Batch processing completed. Processed {processedScenes} of {totalScenes} scenes.");
                    OnBatchCompleted?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during batch processing: {ex.Message}");
                OnBatchError?.Invoke(ex);
                throw;
            }
            finally
            {
                _isProcessing = false;
            }
        }

        public void CancelBatch()
        {
            if (_isProcessing)
            {
                _isCancelled = true;
                _logger.LogInfo("Batch processing cancellation requested");
            }
        }
    }
} 