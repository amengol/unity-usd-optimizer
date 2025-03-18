using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization;
using USDOptimizer.Core.Logging;

namespace USDOptimizer.Unity.Editor
{
    /// <summary>
    /// Handles batch processing of multiple USD scenes
    /// </summary>
    public class BatchProcessor
    {
        private readonly ILogger _logger;
        private readonly USDSceneIO _sceneIO;
        private readonly ISceneOptimizer _sceneOptimizer;
        private bool _isProcessing;
        private int _totalScenes;
        private int _processedScenes;
        private string _currentScene;

        public event Action<float> OnProgressChanged;
        public event Action<string> OnSceneProcessed;
        public event Action OnBatchCompleted;
        public event Action<Exception> OnBatchError;

        public bool IsProcessing => _isProcessing;

        public BatchProcessor(USDSceneIO sceneIO, ISceneOptimizer sceneOptimizer)
        {
            _logger = new UnityLogger("BatchProcessor");
            _sceneIO = sceneIO;
            _sceneOptimizer = sceneOptimizer;
        }

        /// <summary>
        /// Processes multiple USD scenes using the specified optimization profile
        /// </summary>
        public async Task ProcessBatchAsync(List<string> scenePaths, OptimizationProfile profile)
        {
            if (scenePaths == null || scenePaths.Count == 0)
            {
                throw new ArgumentException("No scenes specified for batch processing.", nameof(scenePaths));
            }

            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile), "Optimization profile cannot be null.");
            }

            try
            {
                _isProcessing = true;
                _totalScenes = scenePaths.Count;
                _processedScenes = 0;

                _logger.Info($"Starting batch processing of {_totalScenes} scenes");

                foreach (string scenePath in scenePaths)
                {
                    _currentScene = Path.GetFileName(scenePath);
                    _logger.Info($"Processing scene: {_currentScene}");

                    try
                    {
                        // Import scene
                        Scene scene = await _sceneIO.ImportSceneAsync(scenePath);
                        
                        // Apply optimization settings
                        await _sceneOptimizer.OptimizeSceneAsync(scene, profile.Settings);

                        // Export optimized scene
                        string outputPath = GetOutputPath(scenePath);
                        await _sceneIO.ExportSceneAsync(outputPath, scene);

                        _processedScenes++;
                        float progress = (float)_processedScenes / _totalScenes;
                        OnProgressChanged?.Invoke(progress);
                        OnSceneProcessed?.Invoke(_currentScene);
                    }
                    catch (Exception ex)
                    {
                        _logger.Exception(ex, $"Error processing scene: {_currentScene}");
                        // Continue with next scene
                        continue;
                    }
                }

                _logger.Info("Batch processing completed successfully");
                OnBatchCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "Error during batch processing");
                OnBatchError?.Invoke(ex);
            }
            finally
            {
                _isProcessing = false;
                _currentScene = null;
            }
        }

        /// <summary>
        /// Cancels the current batch processing operation
        /// </summary>
        public void CancelBatch()
        {
            if (_isProcessing)
            {
                _isProcessing = false;
                _logger.Info("Batch processing cancelled");
            }
        }

        /// <summary>
        /// Gets the output path for an optimized scene
        /// </summary>
        private string GetOutputPath(string inputPath)
        {
            string directory = Path.GetDirectoryName(inputPath);
            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string extension = Path.GetExtension(inputPath);
            return Path.Combine(directory, $"{fileName}_optimized{extension}");
        }

        /// <summary>
        /// Shows a folder dialog for selecting multiple USD scenes
        /// </summary>
        public List<string> ShowFolderDialog()
        {
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder with USD Scenes", "", "");
            if (string.IsNullOrEmpty(folderPath))
            {
                return new List<string>();
            }

            List<string> scenePaths = new List<string>();
            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            
            foreach (string file in files)
            {
                if (file.EndsWith(".usd", StringComparison.OrdinalIgnoreCase) ||
                    file.EndsWith(".usda", StringComparison.OrdinalIgnoreCase) ||
                    file.EndsWith(".usdc", StringComparison.OrdinalIgnoreCase))
                {
                    scenePaths.Add(file);
                }
            }

            return scenePaths;
        }
    }
} 