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

                // Simulate analysis delay
                await Task.Delay(1000);
                _progress = 0.5f;

                // TODO: Implement actual scene analysis
                await Task.Delay(1000);
                _progress = 1f;

                _statusMessage = "Scene analysis complete!";
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

                // Simulate optimization delay
                await Task.Delay(1000);
                _progress = 0.5f;

                // TODO: Implement actual scene optimization
                await Task.Delay(1000);
                _progress = 1f;

                _statusMessage = "Scene optimization complete!";
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

                    await _sceneIO.ExportSceneAsync(path, null); // TODO: Pass actual scene
                    _statusMessage = "Scene exported successfully!";
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

        private void DrawPreviewWindow()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Optimization Preview", EditorStyles.boldLabel);
            
            _previewScrollPosition = EditorGUILayout.BeginScrollView(_previewScrollPosition);
            
            // TODO: Implement preview UI
            EditorGUILayout.HelpBox("Preview functionality coming soon...", MessageType.Info);
            
            EditorGUILayout.EndScrollView();
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
    }
} 