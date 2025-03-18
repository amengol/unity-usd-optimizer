using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization;
using USDOptimizer.Core.IO;
using USDOptimizer.Core.Batch;
using USDOptimizer.Core.Logging;
using USDOptimizer.Core.Analysis.Implementations;
using USDOptimizer.Core.Analysis.Interfaces;
using USDSceneOptimizer;

namespace USDOptimizer.Unity.Editor
{
    public class USDSceneOptimizerWindow : EditorWindow
    {
        private USDSceneIO _sceneIO;
        private SceneOptimizer _sceneOptimizer;
        private SceneOptimizationSettings _settings;
        private string _selectedProfile;
        private float _progress;
        private string _currentScene;
        private string _statusMessage;
        private bool _isProcessing;
        private bool _showPreviewWindow;
        private Vector2 _previewScrollPosition;
        private BatchProcessor _batchProcessor;
        private List<string> _selectedScenes;
        private bool _showBatchProcessing;
        private Vector2 _batchScrollPosition;
        private SceneAnalyzer _sceneAnalyzer;
        private AnalysisResults _analysisResults;
        private USDScene _currentUsdScene;
        private USDScene _optimizedScene;
        private List<OptimizationResult> _optimizationResults;

        private void OnEnable()
        {
            _sceneIO = new USDSceneIO();
            _sceneOptimizer = new SceneOptimizer();
            _settings = new SceneOptimizationSettings();
            _selectedScenes = new List<string>();
            _batchProcessor = new BatchProcessor(_sceneIO, _sceneOptimizer);
            _batchProcessor.OnProgressChanged += HandleBatchProgress;
            _batchProcessor.OnSceneProcessed += HandleSceneProcessed;
            _batchProcessor.OnBatchCompleted += HandleBatchCompleted;
            _batchProcessor.OnBatchError += HandleBatchError;

            var meshAnalyzer = new MeshAnalyzer();
            var materialAnalyzer = new MaterialAnalyzer();
            var hierarchyAnalyzer = new SceneHierarchyAnalyzer();
            _sceneAnalyzer = new SceneAnalyzer(meshAnalyzer, materialAnalyzer, hierarchyAnalyzer);
        }

        private void OnDisable()
        {
            if (_batchProcessor != null)
            {
                _batchProcessor.OnProgressChanged -= HandleBatchProgress;
                _batchProcessor.OnSceneProcessed -= HandleSceneProcessed;
                _batchProcessor.OnBatchCompleted -= HandleBatchCompleted;
                _batchProcessor.OnBatchError -= HandleBatchError;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            DrawHeader();
            DrawSettings();
            DrawOptimizationButtons();
            DrawProgressBar();
            DrawStatusMessage();

            if (_showPreviewWindow)
            {
                DrawPreviewWindow();
            }

            if (_showBatchProcessing)
            {
                DrawBatchProcessingWindow();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("USD Scene Optimizer", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Optimization Settings", EditorStyles.boldLabel);
            
            // Profile selection
            string[] profileNames = OptimizationProfileManager.Instance.GetProfileNames();
            int selectedIndex = Array.IndexOf(profileNames, _selectedProfile);
            int newIndex = EditorGUILayout.Popup("Optimization Profile", selectedIndex, profileNames);
            if (newIndex != selectedIndex)
            {
                _selectedProfile = profileNames[newIndex];
                OptimizationProfileManager.Instance.LoadProfile(_selectedProfile, _settings);
            }

            EditorGUILayout.Space();
        }

        private void DrawOptimizationButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Import USD Scene"))
            {
                _ = ImportSceneAsync();
            }

            if (GUILayout.Button("Analyze Scene"))
            {
                _ = AnalyzeSceneAsync();
            }

            if (GUILayout.Button("Preview Optimization"))
            {
                _showPreviewWindow = !_showPreviewWindow;
            }

            if (GUILayout.Button("Optimize Scene"))
            {
                _ = OptimizeSceneAsync();
            }

            if (GUILayout.Button("Export USD Scene"))
            {
                _ = ExportSceneAsync();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Batch processing toggle
            _showBatchProcessing = EditorGUILayout.Toggle("Show Batch Processing", _showBatchProcessing);
        }

        private void DrawProgressBar()
        {
            if (_isProcessing)
            {
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), _progress, _currentScene ?? "Processing...");
            }
        }

        private void DrawStatusMessage()
        {
            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }
        }

        private async Task ImportSceneAsync()
        {
            try
            {
                string path = EditorUtility.OpenFilePanel("Import USD Scene", "", "usd");
                if (!string.IsNullOrEmpty(path))
                {
                    _isProcessing = true;
                    _statusMessage = "Importing scene...";
                    _currentScene = Path.GetFileName(path);
                    _progress = 0f;

                    await _sceneIO.ImportSceneAsync(path);
                    _statusMessage = "Scene imported successfully!";
                }
            }
            catch (Exception ex)
            {
                _statusMessage = $"Error importing scene: {ex.Message}";
            }
            finally
            {
                _isProcessing = false;
                _currentScene = null;
                _progress = 0f;
            }
        }

        private async Task AnalyzeSceneAsync()
        {
            try
            {
                _isProcessing = true;
                _statusMessage = "Analyzing scene...";
                _progress = 0f;

                await Task.Run(() => {
                    try {
                        _progress = 0.3f;
                        _analysisResults = _sceneAnalyzer.AnalyzeSceneAsync(_currentUsdScene).Result;
                        _progress = 0.9f;
                    }
                    catch (Exception ex) {
                        Debug.LogError($"Error during scene analysis: {ex.Message}");
                        throw;
                    }
                });

                _progress = 1f;
                _statusMessage = "Scene analysis complete!";

                _statusMessage += $"\nFound {_analysisResults.TotalPolygons} polygons, {_analysisResults.TotalMaterials} materials";
                _statusMessage += $"\n{_analysisResults.Recommendations.Count} optimization recommendations found.";

                _showPreviewWindow = true;
            }
            catch (Exception ex)
            {
                _statusMessage = $"Error analyzing scene: {ex.Message}";
            }
            finally
            {
                _isProcessing = false;
                _progress = 0f;
            }
        }

        private async Task OptimizeSceneAsync()
        {
            try
            {
                _isProcessing = true;
                _statusMessage = "Optimizing scene...";
                _progress = 0f;

                // Verify that we have a scene to optimize
                if (_currentUsdScene == null)
                {
                    // If no current scene, create one from the current analysis results
                    if (_analysisResults != null)
                    {
                        _currentUsdScene = CreateSceneFromAnalysisResults();
                    }
                    else
                    {
                        _statusMessage = "Error: No scene to optimize. Please analyze a scene first.";
                        _isProcessing = false;
                        return;
                    }
                }

                // Get optimization settings from the selected profile
                var profile = OptimizationProfileManager.Instance.GetProfile(_selectedProfile);
                profile.ApplyToSettings(_settings);

                // Perform optimization in a background task
                await Task.Run(() => {
                    try {
                        _progress = 0.3f;
                        
                        // Optimize the scene
                        var optimizeTask = _sceneOptimizer.OptimizeSceneAsync(_currentUsdScene, _settings);
                        optimizeTask.Wait();
                        _optimizedScene = optimizeTask.Result;
                        
                        // Store optimization results
                        _optimizationResults = _optimizedScene.OptimizationResults;
                        
                        _progress = 0.9f;
                    }
                    catch (Exception ex) {
                        Debug.LogError($"Error during scene optimization: {ex.Message}");
                        throw;
                    }
                });

                _progress = 1f;
                _statusMessage = "Scene optimization complete!";

                // Display some basic results in status message
                int itemsOptimized = 0;
                foreach (var result in _optimizationResults)
                {
                    itemsOptimized += result.ItemsOptimized;
                }
                
                _statusMessage += $"\nOptimized {itemsOptimized} items across {_optimizationResults.Count} optimization types.";
                
                // Compare before/after statistics
                if (_currentUsdScene.Statistics != null && _optimizedScene.Statistics != null)
                {
                    float polyReduction = 1.0f - ((float)_optimizedScene.Statistics.TotalPolygons / _currentUsdScene.Statistics.TotalPolygons);
                    float sizeReduction = 1.0f - (_optimizedScene.Statistics.TotalFileSize / _currentUsdScene.Statistics.TotalFileSize);
                    
                    _statusMessage += $"\nPolygon reduction: {polyReduction:P0}";
                    _statusMessage += $"\nEstimated file size reduction: {sizeReduction:P0}";
                }

                // Enable preview window to show detailed results
                _showPreviewWindow = true;
            }
            catch (Exception ex)
            {
                _statusMessage = $"Error optimizing scene: {ex.Message}";
            }
            finally
            {
                _isProcessing = false;
                _progress = 0f;
            }
        }

        private USDScene CreateSceneFromAnalysisResults()
        {
            var scene = new USDScene
            {
                Name = "AnalyzedScene",
                ImportDate = DateTime.Now,
                Meshes = new List<Mesh>(),
                Materials = new List<Material>(),
                Textures = new List<Texture>()
            };

            // Create statistics
            scene.Statistics = new SceneStatistics
            {
                TotalPolygons = _analysisResults.TotalPolygons,
                TotalVertices = _analysisResults.TotalVertices,
                TotalMaterials = _analysisResults.TotalMaterials,
                TotalTextures = _analysisResults.TotalTextures,
                TotalFileSize = _analysisResults.TotalMemoryUsage,
                TotalNodes = 0, // Not available in analysis results
                NodeTypeCounts = new Dictionary<string, int>()
            };

            // Create dummy mesh data based on analysis
            for (int i = 0; i < 10; i++) // Create some sample meshes
            {
                scene.Meshes.Add(new USDOptimizer.Core.Models.Mesh
                {
                    Name = $"Mesh_{i}",
                    PolygonCount = _analysisResults.TotalPolygons / 10,
                    VertexCount = _analysisResults.TotalVertices / 10
                });
            }

            // Create dummy material data
            for (int i = 0; i < _analysisResults.TotalMaterials; i++)
            {
                scene.Materials.Add(new USDOptimizer.Core.Models.Material
                {
                    Name = $"Material_{i}"
                });
            }

            // Create dummy texture data
            for (int i = 0; i < _analysisResults.TotalTextures; i++)
            {
                scene.Textures.Add(new USDOptimizer.Core.Models.Texture
                {
                    Name = $"Texture_{i}",
                    Width = 1024,
                    Height = 1024,
                    Size = (long)(_analysisResults.TotalMemoryUsage / _analysisResults.TotalTextures)
                });
            }

            return scene;
        }

        private void DrawPreviewWindow()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Optimization Preview", EditorStyles.boldLabel);
            
            _previewScrollPosition = EditorGUILayout.BeginScrollView(_previewScrollPosition);
            
            // Show Analysis Results
            if (_analysisResults != null)
            {
                EditorGUILayout.LabelField("Scene Analysis", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Scene Metrics", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Total Polygons: {_analysisResults.TotalPolygons:N0}");
                EditorGUILayout.LabelField($"Total Vertices: {_analysisResults.TotalVertices:N0}");
                EditorGUILayout.LabelField($"Total Materials: {_analysisResults.TotalMaterials:N0}");
                EditorGUILayout.LabelField($"Total Textures: {_analysisResults.TotalTextures:N0}");
                EditorGUILayout.LabelField($"Estimated Memory Usage: {FormatMemorySize(_analysisResults.TotalMemoryUsage)}");
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Optimization Recommendations", EditorStyles.boldLabel);
                
                if (_analysisResults.Recommendations.Count == 0)
                {
                    EditorGUILayout.HelpBox("No optimization recommendations found.", MessageType.Info);
                }
                else
                {
                    foreach (var recommendation in _analysisResults.Recommendations)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        
                        EditorGUILayout.BeginHorizontal();
                        MessageType messageType = GetMessageTypeForPriority(recommendation.Priority);
                        EditorGUILayout.HelpBox(recommendation.Title, messageType);
                        EditorGUILayout.EndHorizontal();
                        
                        EditorGUILayout.LabelField(recommendation.Description);
                        EditorGUILayout.LabelField($"Priority: {recommendation.Priority}");
                        EditorGUILayout.LabelField($"Estimated Improvement: {recommendation.EstimatedImprovement:P0}");
                        
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            
            // Show Optimization Results
            if (_optimizationResults != null && _optimizationResults.Count > 0)
            {
                EditorGUILayout.LabelField("Optimization Results", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                // Show statistics comparison if available
                if (_currentUsdScene != null && _optimizedScene != null && 
                    _currentUsdScene.Statistics != null && _optimizedScene.Statistics != null)
                {
                    EditorGUILayout.LabelField("Before / After Comparison", EditorStyles.boldLabel);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Metric", GUILayout.Width(150));
                    EditorGUILayout.LabelField("Before", GUILayout.Width(100));
                    EditorGUILayout.LabelField("After", GUILayout.Width(100));
                    EditorGUILayout.LabelField("Change", GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                    
                    DrawStatComparison("Polygons", 
                        _currentUsdScene.Statistics.TotalPolygons, 
                        _optimizedScene.Statistics.TotalPolygons);
                    
                    DrawStatComparison("Vertices", 
                        _currentUsdScene.Statistics.TotalVertices, 
                        _optimizedScene.Statistics.TotalVertices);
                    
                    DrawStatComparison("Materials", 
                        _currentUsdScene.Statistics.TotalMaterials, 
                        _optimizedScene.Statistics.TotalMaterials);
                    
                    DrawStatComparison("Textures", 
                        _currentUsdScene.Statistics.TotalTextures, 
                        _optimizedScene.Statistics.TotalTextures);
                    
                    DrawStatComparison("File Size", 
                        _currentUsdScene.Statistics.TotalFileSize, 
                        _optimizedScene.Statistics.TotalFileSize,
                        true);
                    
                    EditorGUILayout.Space();
                }
                
                // Show detailed optimization results
                EditorGUILayout.LabelField("Applied Optimizations", EditorStyles.boldLabel);
                
                foreach (var result in _optimizationResults)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(result.Type, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Items Optimized: {result.ItemsOptimized}");
                    EditorGUILayout.LabelField($"Notes: {result.Notes}");
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                
                EditorGUI.indentLevel--;
            }
            else if (_optimizedScene != null)
            {
                EditorGUILayout.LabelField("Optimization Results", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Scene was processed but no optimizations were applied.", MessageType.Info);
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawStatComparison(string label, float before, float after, bool isFileSize = false)
        {
            float change = before > 0 ? (after - before) / before : 0;
            string beforeStr = isFileSize ? FormatMemorySize(before) : $"{before:N0}";
            string afterStr = isFileSize ? FormatMemorySize(after) : $"{after:N0}";
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(150));
            EditorGUILayout.LabelField(beforeStr, GUILayout.Width(100));
            EditorGUILayout.LabelField(afterStr, GUILayout.Width(100));
            
            GUIStyle changeStyle = new GUIStyle(EditorStyles.label);
            if (change < 0)
            {
                changeStyle.normal.textColor = Color.green; // Reduction is good
            }
            else if (change > 0)
            {
                changeStyle.normal.textColor = Color.red; // Increase is bad
            }
            
            EditorGUILayout.LabelField($"{change:P1}", changeStyle, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
        }

        private MessageType GetMessageTypeForPriority(OptimizationPriority priority)
        {
            switch (priority)
            {
                case OptimizationPriority.Critical:
                    return MessageType.Error;
                case OptimizationPriority.High:
                    return MessageType.Warning;
                case OptimizationPriority.Medium:
                    return MessageType.Warning;
                case OptimizationPriority.Low:
                default:
                    return MessageType.Info;
            }
        }

        private string FormatMemorySize(float bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            float size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }

        private void DrawBatchProcessingWindow()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Batch Processing", EditorStyles.boldLabel);

            _batchScrollPosition = EditorGUILayout.BeginScrollView(_batchScrollPosition);

            // Selected scenes list
            EditorGUILayout.LabelField("Selected Scenes:", EditorStyles.boldLabel);
            if (_selectedScenes.Count == 0)
            {
                EditorGUILayout.HelpBox("No scenes selected. Click 'Select Scenes' to choose USD scenes for batch processing.", MessageType.Info);
            }
            else
            {
                foreach (string scene in _selectedScenes)
                {
                    EditorGUILayout.LabelField(Path.GetFileName(scene));
                }
            }

            EditorGUILayout.EndScrollView();

            // Buttons
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Select Scenes"))
            {
                _selectedScenes = _batchProcessor.ShowFolderDialog();
            }

            if (_selectedScenes.Count > 0)
            {
                if (_batchProcessor.IsProcessing)
                {
                    if (GUILayout.Button("Cancel"))
                    {
                        _batchProcessor.CancelBatch();
                    }
                }
                else
                {
                    if (GUILayout.Button("Process Scenes"))
                    {
                        var profile = OptimizationProfileManager.Instance.GetProfile(_selectedProfile);
                        _ = ProcessScenesAsync(profile);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            // Progress
            if (_batchProcessor.IsProcessing)
            {
                EditorGUILayout.Space();
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), _progress, $"Processing: {_currentScene}");
            }
        }

        private async Task ProcessScenesAsync(USDOptimizer.Core.Optimization.OptimizationProfile profile)
        {
            try
            {
                await _batchProcessor.ProcessBatchAsync(_selectedScenes, profile);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Error processing scenes: {ex.Message}", "OK");
            }
        }

        private void HandleBatchProgress(float progress)
        {
            _progress = progress;
            Repaint();
        }

        private void HandleSceneProcessed(string sceneName)
        {
            _currentScene = sceneName;
            Repaint();
        }

        private void HandleBatchCompleted()
        {
            EditorUtility.DisplayDialog("Success", "Batch processing completed successfully!", "OK");
            _progress = 0f;
            _currentScene = null;
            Repaint();
        }

        private void HandleBatchError(Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"Error during batch processing: {ex.Message}", "OK");
            _progress = 0f;
            _currentScene = null;
            Repaint();
        }

        private async Task ExportSceneAsync()
        {
            try
            {
                string path = EditorUtility.SaveFilePanel("Export USD Scene", "", "optimized_scene", "usd");
                if (!string.IsNullOrEmpty(path))
                {
                    _isProcessing = true;
                    _statusMessage = "Exporting scene...";
                    _currentScene = Path.GetFileName(path);
                    _progress = 0f;

                    // Use the optimized scene if available, otherwise use the current scene
                    USDScene sceneToExport = _optimizedScene ?? _currentUsdScene;
                    
                    if (sceneToExport != null)
                    {
                        await _sceneIO.ExportSceneAsync(path, sceneToExport);
                        _statusMessage = "Scene exported successfully!";
                    }
                    else
                    {
                        _statusMessage = "Error: No scene to export. Please analyze or optimize a scene first.";
                    }
                }
            }
            catch (Exception ex)
            {
                _statusMessage = $"Error exporting scene: {ex.Message}";
            }
            finally
            {
                _isProcessing = false;
                _currentScene = null;
                _progress = 0f;
            }
        }

        private void AnalyzeScene()
        {
            _statusMessage = "Analyzing scene...";
            _progress = 0.5f;
            Repaint();
            
            try
            {
                // For a real implementation, use AnalyzeSceneAsync, but for this example:
                _analysisResults = _sceneAnalyzer.AnalyzeSceneAsync(_currentUsdScene).Result;
                _statusMessage = $"Analysis complete. Found {_analysisResults.TotalNodes} nodes, {_analysisResults.TotalPolygons} polygons, {_analysisResults.TotalMaterials} materials.";
                
                if (_analysisResults.Recommendations.Count > 0)
                {
                    _statusMessage += $"\n{_analysisResults.Recommendations.Count} optimization recommendations found.";
                }
            }
            catch (Exception ex)
            {
                _statusMessage = $"Error analyzing scene: {ex.Message}";
                Debug.LogError($"Analysis error: {ex}");
            }
            
            _progress = 1.0f;
            Repaint();
        }

        private void CreateSampleScene()
        {
            _currentUsdScene = new USDScene
            {
                Name = "Sample Scene",
                FilePath = "sample.usda",
                ImportDate = DateTime.Now,
                Nodes = new List<USDNode>(),
                Meshes = new List<USDOptimizer.Core.Models.Mesh>(),
                Materials = new List<USDOptimizer.Core.Models.Material>(),
                Textures = new List<USDOptimizer.Core.Models.Texture>(),
                OptimizationResults = new List<USDOptimizer.Core.Models.OptimizationResult>()
            };

            // ... existing code continuing from here ...
        }

        private void DisplayMemoryUsagePieChart(Rect rect)
        {
            if (_analysisResults == null) return;
            
            GUILayout.BeginArea(rect);
            GUILayout.Label("Memory Usage Breakdown", EditorStyles.boldLabel);
            
            float meshMemory = _analysisResults.MeshMetrics?.MemoryUsage ?? 0;
            float textureMemory = _analysisResults.MaterialMetrics?.TextureMemoryUsage ?? 0;
            
            // Convert to MB for display
            float meshMemoryMB = meshMemory / (1024 * 1024);
            float textureMemoryMB = textureMemory / (1024 * 1024);
            float totalMemoryMB = (meshMemory + textureMemory) / (1024 * 1024);
            
            // Display values
            EditorGUILayout.LabelField($"Total Memory: {totalMemoryMB:F2} MB");
            EditorGUILayout.LabelField($"Mesh Data: {meshMemoryMB:F2} MB ({meshMemoryMB / totalMemoryMB * 100:F1}%)");
            EditorGUILayout.LabelField($"Texture Data: {textureMemoryMB:F2} MB ({textureMemoryMB / totalMemoryMB * 100:F1}%)");
            
            GUILayout.EndArea();
        }

        private void ShowOptimizationOptions()
        {
            if (_analysisResults == null)
            {
                EditorGUILayout.HelpBox("Run analysis first to see optimization options.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Optimization Statistics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Polygon Count: {_analysisResults.TotalPolygons:N0}");
            EditorGUILayout.LabelField($"Texture Count: {_analysisResults.TotalTextures:N0}");
            EditorGUILayout.LabelField($"Material Count: {_analysisResults.TotalMaterials:N0}");
            EditorGUILayout.LabelField($"Estimated Memory Usage: {FormatMemorySize(_analysisResults.TotalMemoryUsage)}");
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Optimization Recommendations", EditorStyles.boldLabel);
            
            if (_analysisResults.Recommendations.Count == 0)
            {
                EditorGUILayout.HelpBox("No optimization recommendations found.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Found {_analysisResults.Recommendations.Count} optimization recommendations.", MessageType.Info);
                
                foreach (var recommendation in _analysisResults.Recommendations)
                {
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                    
                    // Display priority icon
                    Texture2D priorityIcon = GetPriorityIcon(recommendation.Priority);
                    if (priorityIcon != null)
                    {
                        GUILayout.Label(new GUIContent(priorityIcon), GUILayout.Width(20), GUILayout.Height(20));
                    }
                    
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(recommendation.Title, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(recommendation.Description, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
            }
        }

        private Texture2D GetPriorityIcon(OptimizationPriorityLevel priority)
        {
            switch (priority)
            {
                case OptimizationPriorityLevel.Critical:
                    return EditorGUIUtility.FindTexture("console.erroricon");
                case OptimizationPriorityLevel.High:
                    return EditorGUIUtility.FindTexture("console.warnicon");
                case OptimizationPriorityLevel.Medium:
                    return EditorGUIUtility.FindTexture("console.infoicon");
                case OptimizationPriorityLevel.Low:
                    return EditorGUIUtility.FindTexture("console.infoicon.sml");
                default:
                    return null;
            }
        }

        private OptimizationPriority ConvertPriorityLevel(OptimizationPriorityLevel level)
        {
            switch (level)
            {
                case OptimizationPriorityLevel.Critical:
                    return OptimizationPriority.Critical;
                case OptimizationPriorityLevel.High:
                    return OptimizationPriority.High;
                case OptimizationPriorityLevel.Medium:
                    return OptimizationPriority.Medium;
                case OptimizationPriorityLevel.Low:
                default:
                    return OptimizationPriority.Low;
            }
        }
    }
} 