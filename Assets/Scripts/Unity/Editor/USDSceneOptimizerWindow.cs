using UnityEngine;
using UnityEditor;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization;
using USDOptimizer.Core.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace USDOptimizer.Unity.Editor
{
    public class USDSceneOptimizerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private SceneOptimizationSettings _settings;
        private ILogger _logger;
        private bool _isOptimizing;
        private string _statusMessage;
        private float _progress;
        private string _selectedProfile;
        private string _newProfileName = "";
        private string _newProfileDescription = "";
        private bool _showSaveProfileDialog;
        private bool _showDeleteProfileDialog;
        private string _profileToDelete;
        private USDSceneIO _sceneIO;
        private Scene _currentScene;
        private bool _hasLoadedScene;
        private OptimizationPreviewManager _previewManager;
        private bool _isPreviewActive;
        private bool _showPreviewWindow;
        private Vector2 _previewScrollPosition;
        private BatchProcessor _batchProcessor;
        private List<string> _selectedScenes;
        private bool _showBatchProcessing;
        private Vector2 _batchScrollPosition;

        [MenuItem("Window/USD Scene Optimizer")]
        public static void ShowWindow()
        {
            GetWindow<USDSceneOptimizerWindow>("USD Scene Optimizer");
        }

        private void OnEnable()
        {
            _settings = new SceneOptimizationSettings();
            _logger = new UnityLogger("USDSceneOptimizer");
            _sceneIO = new USDSceneIO();
            _previewManager = new OptimizationPreviewManager(new SceneOptimizer());
            _previewManager.OnPreviewProgressChanged += HandlePreviewProgressChanged;
            _previewManager.OnPreviewUpdated += HandlePreviewUpdated;
            _previewManager.OnPreviewCompleted += HandlePreviewCompleted;
            _previewManager.OnPreviewError += HandlePreviewError;
            OptimizationProfileManager.Instance.Refresh();
            _batchProcessor = new BatchProcessor(_sceneIO, new SceneOptimizer());
            _batchProcessor.OnProgressChanged += HandleBatchProgress;
            _batchProcessor.OnSceneProcessed += HandleSceneProcessed;
            _batchProcessor.OnBatchCompleted += HandleBatchCompleted;
            _batchProcessor.OnBatchError += HandleBatchError;
            _selectedScenes = new List<string>();
        }

        private void OnDisable()
        {
            if (_previewManager != null)
            {
                _previewManager.OnPreviewProgressChanged -= HandlePreviewProgressChanged;
                _previewManager.OnPreviewUpdated -= HandlePreviewUpdated;
                _previewManager.OnPreviewCompleted -= HandlePreviewCompleted;
                _previewManager.OnPreviewError -= HandlePreviewError;
            }
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
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            DrawSceneIO();
            DrawProfileManagement();
            DrawSettings();
            DrawOptimizationButtons();
            DrawProgressBar();
            DrawStatusMessage();

            EditorGUILayout.EndScrollView();

            if (_showSaveProfileDialog)
            {
                DrawSaveProfileDialog();
            }

            if (_showDeleteProfileDialog)
            {
                DrawDeleteProfileDialog();
            }

            if (_showPreviewWindow)
            {
                DrawPreviewWindow();
            }

            if (_showBatchProcessing)
            {
                DrawBatchProcessingWindow();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("USD Scene Optimizer", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Optimize your USD scenes for better real-time performance.", MessageType.Info);
            EditorGUILayout.Space(10);
        }

        private void DrawSceneIO()
        {
            EditorGUILayout.LabelField("Scene Import/Export", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Import USD Scene"))
            {
                ImportScene();
            }

            GUI.enabled = _hasLoadedScene;
            if (GUILayout.Button("Export USD Scene"))
            {
                ExportScene();
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (_hasLoadedScene)
            {
                EditorGUILayout.HelpBox("Scene loaded successfully.", MessageType.Info);
            }

            EditorGUILayout.Space(10);
        }

        private async void ImportScene()
        {
            string filePath = _sceneIO.ShowImportDialog();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            _isOptimizing = true;
            _statusMessage = "Importing USD scene...";
            _progress = 0f;

            try
            {
                _currentScene = await _sceneIO.ImportSceneAsync(filePath);
                _hasLoadedScene = true;
                _statusMessage = "USD scene imported successfully.";
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "Error importing USD scene");
                _statusMessage = "Error importing USD scene. Check console for details.";
            }
            finally
            {
                _isOptimizing = false;
                _progress = 0f;
            }
        }

        private async void ExportScene()
        {
            if (_currentScene == null)
            {
                _statusMessage = "No scene loaded to export.";
                return;
            }

            string filePath = _sceneIO.ShowExportDialog();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            _isOptimizing = true;
            _statusMessage = "Exporting USD scene...";
            _progress = 0f;

            try
            {
                await _sceneIO.ExportSceneAsync(filePath, _currentScene);
                _statusMessage = "USD scene exported successfully.";
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "Error exporting USD scene");
                _statusMessage = "Error exporting USD scene. Check console for details.";
            }
            finally
            {
                _isOptimizing = false;
                _progress = 0f;
            }
        }

        private void DrawProfileManagement()
        {
            EditorGUILayout.LabelField("Optimization Profiles", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Profile selection
            string[] profileNames = OptimizationProfileManager.Instance.GetProfileNames();
            int selectedIndex = Array.IndexOf(profileNames, _selectedProfile);
            int newIndex = EditorGUILayout.Popup("Select Profile", selectedIndex, profileNames);
            if (newIndex != selectedIndex && newIndex >= 0 && newIndex < profileNames.Length)
            {
                _selectedProfile = profileNames[newIndex];
                OptimizationProfileManager.Instance.LoadProfile(_selectedProfile, _settings);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Current Settings as Profile"))
            {
                _showSaveProfileDialog = true;
            }

            if (!string.IsNullOrEmpty(_selectedProfile) && GUILayout.Button("Delete Profile"))
            {
                _profileToDelete = _selectedProfile;
                _showDeleteProfileDialog = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        private void DrawSaveProfileDialog()
        {
            var rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Save Profile", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            _newProfileName = EditorGUILayout.TextField("Profile Name", _newProfileName);
            _newProfileDescription = EditorGUILayout.TextField("Description", _newProfileDescription);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                if (!string.IsNullOrEmpty(_newProfileName))
                {
                    OptimizationProfileManager.Instance.SaveProfile(_newProfileName, _newProfileDescription, _settings);
                    _newProfileName = "";
                    _newProfileDescription = "";
                    _showSaveProfileDialog = false;
                }
            }
            if (GUILayout.Button("Cancel"))
            {
                _showSaveProfileDialog = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawDeleteProfileDialog()
        {
            var rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Delete Profile", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField($"Are you sure you want to delete the profile '{_profileToDelete}'?");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete"))
            {
                OptimizationProfileManager.Instance.DeleteProfile(_profileToDelete);
                _selectedProfile = null;
                _showDeleteProfileDialog = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                _showDeleteProfileDialog = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Optimization Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Mesh Optimization Settings
            EditorGUILayout.LabelField("Mesh Optimization", EditorStyles.boldLabel);
            _settings.EnableLODGeneration = EditorGUILayout.Toggle("Enable LOD Generation", _settings.EnableLODGeneration);
            if (_settings.EnableLODGeneration)
            {
                EditorGUI.indentLevel++;
                _settings.LODLevels = EditorGUILayout.IntSlider("LOD Levels", _settings.LODLevels, 2, 5);
                EditorGUI.indentLevel--;
            }

            _settings.EnableMeshSimplification = EditorGUILayout.Toggle("Enable Mesh Simplification", _settings.EnableMeshSimplification);
            if (_settings.EnableMeshSimplification)
            {
                EditorGUI.indentLevel++;
                _settings.TargetPolygonCount = EditorGUILayout.IntSlider("Target Polygon Count", _settings.TargetPolygonCount, 100, 10000);
                EditorGUI.indentLevel--;
            }

            // Material Optimization Settings
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Material Optimization", EditorStyles.boldLabel);
            _settings.EnableTextureCompression = EditorGUILayout.Toggle("Enable Texture Compression", _settings.EnableTextureCompression);
            _settings.EnableMaterialBatching = EditorGUILayout.Toggle("Enable Material Batching", _settings.EnableMaterialBatching);
            _settings.EnableShaderOptimization = EditorGUILayout.Toggle("Enable Shader Optimization", _settings.EnableShaderOptimization);

            // Scene Optimization Settings
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Scene Optimization", EditorStyles.boldLabel);
            _settings.EnableInstanceOptimization = EditorGUILayout.Toggle("Enable Instance Optimization", _settings.EnableInstanceOptimization);
            if (_settings.EnableInstanceOptimization)
            {
                EditorGUI.indentLevel++;
                _settings.SimilarityThreshold = EditorGUILayout.Slider("Similarity Threshold", _settings.SimilarityThreshold, 0.1f, 1.0f);
                EditorGUI.indentLevel--;
            }

            _settings.EnableHierarchyFlattening = EditorGUILayout.Toggle("Enable Hierarchy Flattening", _settings.EnableHierarchyFlattening);
            if (_settings.EnableHierarchyFlattening)
            {
                EditorGUI.indentLevel++;
                _settings.MaxHierarchyDepth = EditorGUILayout.IntSlider("Max Hierarchy Depth", _settings.MaxHierarchyDepth, 1, 10);
                EditorGUI.indentLevel--;
            }

            _settings.EnableTransformOptimization = EditorGUILayout.Toggle("Enable Transform Optimization", _settings.EnableTransformOptimization);
        }

        private void DrawOptimizationButtons()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !_isOptimizing && !_isPreviewActive;
            if (GUILayout.Button("Analyze Scene"))
            {
                AnalyzeScene();
            }

            if (GUILayout.Button("Optimize Scene"))
            {
                OptimizeScene();
            }

            if (GUILayout.Button("Preview Optimization"))
            {
                _showPreviewWindow = true;
            }

            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private void DrawProgressBar()
        {
            if (_isOptimizing)
            {
                EditorGUILayout.Space(10);
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), _progress, "Optimizing...");
            }
        }

        private void DrawStatusMessage()
        {
            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }
        }

        private async void AnalyzeScene()
        {
            _isOptimizing = true;
            _statusMessage = "Analyzing scene...";
            _progress = 0f;

            try
            {
                // TODO: Implement scene analysis
                await System.Threading.Tasks.Task.Delay(1000); // Simulated delay
                _statusMessage = "Scene analysis complete.";
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "Error analyzing scene");
                _statusMessage = "Error analyzing scene. Check console for details.";
            }
            finally
            {
                _isOptimizing = false;
                _progress = 0f;
            }
        }

        private async void OptimizeScene()
        {
            _isOptimizing = true;
            _statusMessage = "Optimizing scene...";
            _progress = 0f;

            try
            {
                // TODO: Implement scene optimization
                await System.Threading.Tasks.Task.Delay(1000); // Simulated delay
                _statusMessage = "Scene optimization complete.";
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "Error optimizing scene");
                _statusMessage = "Error optimizing scene. Check console for details.";
            }
            finally
            {
                _isOptimizing = false;
                _progress = 0f;
            }
        }

        private void DrawPreviewWindow()
        {
            var rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Optimization Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            _previewScrollPosition = EditorGUILayout.BeginScrollView(_previewScrollPosition);

            if (_previewManager.IsPreviewActive)
            {
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), _previewManager.PreviewProgress, "Generating Preview...");
                EditorGUILayout.Space(5);

                if (GUILayout.Button("Cancel Preview"))
                {
                    _previewManager.CancelPreview();
                    _showPreviewWindow = false;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Preview shows the effects of current optimization settings.", MessageType.Info);
                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate Preview"))
                {
                    GeneratePreview();
                }

                if (GUILayout.Button("Close Preview"))
                {
                    _showPreviewWindow = false;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private async void GeneratePreview()
        {
            if (_currentScene == null)
            {
                _statusMessage = "No scene loaded to preview.";
                return;
            }

            _isPreviewActive = true;
            _statusMessage = "Generating optimization preview...";
            _showPreviewWindow = true;

            try
            {
                await _previewManager.StartPreviewAsync(_currentScene, _settings);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "Error generating preview");
                _statusMessage = "Error generating preview. Check console for details.";
            }
        }

        private void HandlePreviewProgressChanged(float progress)
        {
            Repaint();
        }

        private void HandlePreviewUpdated(Scene previewScene)
        {
            // TODO: Update preview visualization
            Repaint();
        }

        private void HandlePreviewCompleted()
        {
            _isPreviewActive = false;
            _statusMessage = "Preview generated successfully.";
            Repaint();
        }

        private void HandlePreviewError(Exception ex)
        {
            _isPreviewActive = false;
            _statusMessage = "Error generating preview. Check console for details.";
            Repaint();
        }

        private void DrawBatchProcessingWindow()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
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

            // Profile selection
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Optimization Profile:", EditorStyles.boldLabel);
            string[] profileNames = OptimizationProfileManager.Instance.GetProfileNames();
            int selectedIndex = Array.IndexOf(profileNames, _selectedProfile);
            int newIndex = EditorGUILayout.Popup(selectedIndex, profileNames);
            if (newIndex != selectedIndex)
            {
                _selectedProfile = profileNames[newIndex];
                OptimizationProfileManager.Instance.LoadProfile(_selectedProfile, _settings);
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

            GUILayout.EndVertical();
        }

        private async Task ProcessScenesAsync(OptimizationProfile profile)
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