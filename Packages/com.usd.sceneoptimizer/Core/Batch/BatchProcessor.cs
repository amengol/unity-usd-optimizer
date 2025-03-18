using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            // TODO: Implement folder dialog using Unity's EditorUtility.OpenFolderPanel
            // This is a placeholder that returns an empty list
            return new List<string>();
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

                _logger.LogInfo($"Starting batch processing of {scenePaths.Count} scenes");

                for (int i = 0; i < scenePaths.Count; i++)
                {
                    if (_isCancelled)
                    {
                        _logger.LogWarning("Batch processing cancelled by user");
                        break;
                    }

                    string scenePath = scenePaths[i];
                    float progress = (float)i / scenePaths.Count;
                    OnProgressChanged?.Invoke(progress);

                    try
                    {
                        await ProcessSceneAsync(scenePath, profile);
                        OnSceneProcessed?.Invoke(Path.GetFileName(scenePath));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing scene {scenePath}: {ex.Message}");
                        // Continue with next scene
                        continue;
                    }
                }

                OnProgressChanged?.Invoke(1f);
                OnBatchCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during batch processing: {ex.Message}");
                OnBatchError?.Invoke(ex);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        public void CancelBatch()
        {
            _isCancelled = true;
            _logger.LogInfo("Batch processing cancellation requested");
        }

        private async Task ProcessSceneAsync(string scenePath, OptimizationProfile profile)
        {
            try
            {
                _logger.LogInfo($"Processing scene: {scenePath}");

                // Import scene
                var scene = await _sceneIO.ImportSceneAsync(scenePath);

                // Optimize scene
                var optimizedScene = await _sceneOptimizer.OptimizeSceneAsync(scene, profile.Settings);

                // Export optimized scene
                string outputPath = GetOutputPath(scenePath);
                await _sceneIO.ExportSceneAsync(outputPath, optimizedScene);

                _logger.LogInfo($"Successfully processed scene: {scenePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing scene {scenePath}: {ex.Message}");
                throw;
            }
        }

        private string GetOutputPath(string inputPath)
        {
            string directory = Path.GetDirectoryName(inputPath);
            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string extension = Path.GetExtension(inputPath);
            return Path.Combine(directory, $"{fileName}_Optimized{extension}");
        }
    }
} 