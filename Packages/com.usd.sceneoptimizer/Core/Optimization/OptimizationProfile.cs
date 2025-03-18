using UnityEngine;

namespace USDOptimizer.Core.Optimization
{
    public class OptimizationProfile : ScriptableObject
    {
        public string Name;
        public string Description;
        public SceneOptimizationSettings Settings;

        public void ApplyToSettings(SceneOptimizationSettings targetSettings)
        {
            if (targetSettings == null)
            {
                targetSettings = new SceneOptimizationSettings();
            }

            // Mesh optimization settings
            targetSettings.EnableLODGeneration = Settings.EnableLODGeneration;
            targetSettings.LODLevels = Settings.LODLevels;
            targetSettings.EnableMeshSimplification = Settings.EnableMeshSimplification;
            targetSettings.TargetPolygonCount = Settings.TargetPolygonCount;

            // Material optimization settings
            targetSettings.EnableTextureCompression = Settings.EnableTextureCompression;
            targetSettings.EnableMaterialBatching = Settings.EnableMaterialBatching;
            targetSettings.EnableShaderOptimization = Settings.EnableShaderOptimization;

            // Scene optimization settings
            targetSettings.EnableInstanceOptimization = Settings.EnableInstanceOptimization;
            targetSettings.SimilarityThreshold = Settings.SimilarityThreshold;
            targetSettings.EnableHierarchyFlattening = Settings.EnableHierarchyFlattening;
            targetSettings.MaxHierarchyDepth = Settings.MaxHierarchyDepth;
            targetSettings.EnableTransformOptimization = Settings.EnableTransformOptimization;
        }

        public void LoadFromSettings(SceneOptimizationSettings sourceSettings)
        {
            if (sourceSettings == null)
            {
                sourceSettings = new SceneOptimizationSettings();
            }

            // Mesh optimization settings
            Settings.EnableLODGeneration = sourceSettings.EnableLODGeneration;
            Settings.LODLevels = sourceSettings.LODLevels;
            Settings.EnableMeshSimplification = sourceSettings.EnableMeshSimplification;
            Settings.TargetPolygonCount = sourceSettings.TargetPolygonCount;

            // Material optimization settings
            Settings.EnableTextureCompression = sourceSettings.EnableTextureCompression;
            Settings.EnableMaterialBatching = sourceSettings.EnableMaterialBatching;
            Settings.EnableShaderOptimization = sourceSettings.EnableShaderOptimization;

            // Scene optimization settings
            Settings.EnableInstanceOptimization = sourceSettings.EnableInstanceOptimization;
            Settings.SimilarityThreshold = sourceSettings.SimilarityThreshold;
            Settings.EnableHierarchyFlattening = sourceSettings.EnableHierarchyFlattening;
            Settings.MaxHierarchyDepth = sourceSettings.MaxHierarchyDepth;
            Settings.EnableTransformOptimization = sourceSettings.EnableTransformOptimization;
        }
    }
} 