using UnityEngine;
using USDOptimizer.Core.Optimization;

namespace USDOptimizer.Unity.Editor
{
    /// <summary>
    /// ScriptableObject that stores optimization settings for USD scenes
    /// </summary>
    public class OptimizationProfile : ScriptableObject
    {
        [SerializeField]
        private string _profileName = "New Profile";

        [SerializeField]
        private string _description = "Profile description";

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

        /// <summary>
        /// Applies the profile settings to the target settings object
        /// </summary>
        public void ApplySettings(SceneOptimizationSettings target)
        {
            target.EnableLODGeneration = _settings.EnableLODGeneration;
            target.LODLevels = _settings.LODLevels;
            target.EnableMeshSimplification = _settings.EnableMeshSimplification;
            target.TargetPolygonCount = _settings.TargetPolygonCount;
            target.EnableTextureCompression = _settings.EnableTextureCompression;
            target.EnableMaterialBatching = _settings.EnableMaterialBatching;
            target.EnableShaderOptimization = _settings.EnableShaderOptimization;
            target.EnableInstanceOptimization = _settings.EnableInstanceOptimization;
            target.SimilarityThreshold = _settings.SimilarityThreshold;
            target.EnableHierarchyFlattening = _settings.EnableHierarchyFlattening;
            target.MaxHierarchyDepth = _settings.MaxHierarchyDepth;
            target.EnableTransformOptimization = _settings.EnableTransformOptimization;
        }

        /// <summary>
        /// Loads settings from the source settings object into this profile
        /// </summary>
        public void LoadSettings(SceneOptimizationSettings source)
        {
            _settings.EnableLODGeneration = source.EnableLODGeneration;
            _settings.LODLevels = source.LODLevels;
            _settings.EnableMeshSimplification = source.EnableMeshSimplification;
            _settings.TargetPolygonCount = source.TargetPolygonCount;
            _settings.EnableTextureCompression = source.EnableTextureCompression;
            _settings.EnableMaterialBatching = source.EnableMaterialBatching;
            _settings.EnableShaderOptimization = source.EnableShaderOptimization;
            _settings.EnableInstanceOptimization = source.EnableInstanceOptimization;
            _settings.SimilarityThreshold = source.SimilarityThreshold;
            _settings.EnableHierarchyFlattening = source.EnableHierarchyFlattening;
            _settings.MaxHierarchyDepth = source.MaxHierarchyDepth;
            _settings.EnableTransformOptimization = source.EnableTransformOptimization;
        }
    }
} 