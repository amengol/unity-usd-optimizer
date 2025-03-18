using UnityEngine;
using UnityEditor;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Unity.Editor
{
    /// <summary>
    /// Creates and manages default optimization profiles
    /// </summary>
    public static class DefaultOptimizationProfiles
    {
        private const string PROFILES_FOLDER = "Assets/USDSceneOptimizer/Profiles";

        [MenuItem("USD Scene Optimizer/Create Default Profiles")]
        public static void CreateDefaultProfiles()
        {
            // Create profiles folder if it doesn't exist
            if (!System.IO.Directory.Exists(PROFILES_FOLDER))
            {
                System.IO.Directory.CreateDirectory(PROFILES_FOLDER);
                AssetDatabase.Refresh();
            }

            CreatePerformanceProfile();
            CreateBalancedProfile();
            CreateQualityProfile();
        }

        private static void CreatePerformanceProfile()
        {
            var profile = ScriptableObject.CreateInstance<OptimizationProfile>();
            profile.ProfileName = "Performance";
            profile.Description = "Maximum performance optimization. Aggressive settings for best runtime performance.";
            
            var settings = new SceneOptimizationSettings
            {
                EnableLODGeneration = true,
                LODLevels = 4,
                EnableMeshSimplification = true,
                TargetPolygonCount = 5000,
                EnableTextureCompression = true,
                EnableMaterialBatching = true,
                EnableShaderOptimization = true,
                EnableInstanceOptimization = true,
                SimilarityThreshold = 0.8f,
                EnableHierarchyFlattening = true,
                MaxHierarchyDepth = 3,
                EnableTransformOptimization = true
            };
            
            profile.Settings = settings;
            SaveProfile(profile);
        }

        private static void CreateBalancedProfile()
        {
            var profile = ScriptableObject.CreateInstance<OptimizationProfile>();
            profile.ProfileName = "Balanced";
            profile.Description = "Balanced optimization. Good balance between performance and quality.";
            
            var settings = new SceneOptimizationSettings
            {
                EnableLODGeneration = true,
                LODLevels = 3,
                EnableMeshSimplification = true,
                TargetPolygonCount = 8000,
                EnableTextureCompression = true,
                EnableMaterialBatching = true,
                EnableShaderOptimization = true,
                EnableInstanceOptimization = true,
                SimilarityThreshold = 0.6f,
                EnableHierarchyFlattening = true,
                MaxHierarchyDepth = 5,
                EnableTransformOptimization = true
            };
            
            profile.Settings = settings;
            SaveProfile(profile);
        }

        private static void CreateQualityProfile()
        {
            var profile = ScriptableObject.CreateInstance<OptimizationProfile>();
            profile.ProfileName = "Quality";
            profile.Description = "Quality-focused optimization. Preserves visual quality while still improving performance.";
            
            var settings = new SceneOptimizationSettings
            {
                EnableLODGeneration = true,
                LODLevels = 2,
                EnableMeshSimplification = false,
                TargetPolygonCount = 10000,
                EnableTextureCompression = true,
                EnableMaterialBatching = true,
                EnableShaderOptimization = true,
                EnableInstanceOptimization = true,
                SimilarityThreshold = 0.4f,
                EnableHierarchyFlattening = false,
                MaxHierarchyDepth = 7,
                EnableTransformOptimization = true
            };
            
            profile.Settings = settings;
            SaveProfile(profile);
        }

        private static void SaveProfile(OptimizationProfile profile)
        {
            string path = System.IO.Path.Combine(PROFILES_FOLDER, $"{profile.ProfileName}.asset");
            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
        }
    }
} 