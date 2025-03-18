using UnityEngine;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Unity.Editor
{
    /// <summary>
    /// ScriptableObject for storing optimization profiles
    /// </summary>
    [CreateAssetMenu(fileName = "New Optimization Profile", menuName = "USD Scene Optimizer/Optimization Profile")]
    public class OptimizationProfile : ScriptableObject
    {
        [SerializeField]
        private string _profileName = "New Profile";

        [SerializeField]
        private string _description = "Description of this optimization profile";

        [SerializeField]
        private SceneOptimizationSettings _settings = new SceneOptimizationSettings();

        public string ProfileName
        {
            get => _profileName;
            set => _profileName = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public SceneOptimizationSettings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        public void ApplySettings(SceneOptimizationSettings targetSettings)
        {
            targetSettings.EnableLODGeneration = _settings.EnableLODGeneration;
            targetSettings.LODLevels = _settings.LODLevels;
            targetSettings.EnableMeshSimplification = _settings.EnableMeshSimplification;
            targetSettings.TargetPolygonCount = _settings.TargetPolygonCount;
            targetSettings.EnableTextureCompression = _settings.EnableTextureCompression;
            targetSettings.EnableMaterialBatching = _settings.EnableMaterialBatching;
            targetSettings.EnableShaderOptimization = _settings.EnableShaderOptimization;
            targetSettings.EnableInstanceOptimization = _settings.EnableInstanceOptimization;
            targetSettings.SimilarityThreshold = _settings.SimilarityThreshold;
            targetSettings.EnableHierarchyFlattening = _settings.EnableHierarchyFlattening;
            targetSettings.MaxHierarchyDepth = _settings.MaxHierarchyDepth;
            targetSettings.EnableTransformOptimization = _settings.EnableTransformOptimization;
        }

        public void LoadSettings(SceneOptimizationSettings sourceSettings)
        {
            _settings.EnableLODGeneration = sourceSettings.EnableLODGeneration;
            _settings.LODLevels = sourceSettings.LODLevels;
            _settings.EnableMeshSimplification = sourceSettings.EnableMeshSimplification;
            _settings.TargetPolygonCount = sourceSettings.TargetPolygonCount;
            _settings.EnableTextureCompression = sourceSettings.EnableTextureCompression;
            _settings.EnableMaterialBatching = sourceSettings.EnableMaterialBatching;
            _settings.EnableShaderOptimization = sourceSettings.EnableShaderOptimization;
            _settings.EnableInstanceOptimization = sourceSettings.EnableInstanceOptimization;
            _settings.SimilarityThreshold = sourceSettings.SimilarityThreshold;
            _settings.EnableHierarchyFlattening = sourceSettings.EnableHierarchyFlattening;
            _settings.MaxHierarchyDepth = sourceSettings.MaxHierarchyDepth;
            _settings.EnableTransformOptimization = sourceSettings.EnableTransformOptimization;
        }
    }
} 