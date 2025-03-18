using UnityEngine;
using UnityEditor;
using System.IO;
using USDOptimizer.Core.Optimization;

namespace USDOptimizer.Unity.Editor
{
    public static class DefaultOptimizationProfiles
    {
        private const string PROFILES_FOLDER = "Assets/Scripts/Unity/Editor/Profiles";

        public static void CreateDefaultProfiles()
        {
            // Create profiles folder if it doesn't exist
            if (!Directory.Exists(PROFILES_FOLDER))
            {
                Directory.CreateDirectory(PROFILES_FOLDER);
                AssetDatabase.Refresh();
            }

            // Create Performance profile
            CreatePerformanceProfile();
            
            // Create Balanced profile
            CreateBalancedProfile();
            
            // Create Quality profile
            CreateQualityProfile();
        }

        private static void CreatePerformanceProfile()
        {
            var profile = ScriptableObject.CreateInstance<OptimizationProfile>();
            profile.ProfileName = "Performance";
            profile.Description = "Aggressive optimization for maximum performance";
            profile.Settings = new SceneOptimizationSettings
            {
                EnableLODGeneration = true,
                LODLevels = 3,
                EnableMeshSimplification = true,
                TargetPolygonCount = 5000,
                EnableTextureCompression = true,
                EnableMaterialBatching = true,
                EnableShaderOptimization = true,
                EnableInstanceOptimization = true,
                SimilarityThreshold = 0.8f,
                EnableHierarchyFlattening = true,
                MaxHierarchyDepth = 2,
                EnableTransformOptimization = true
            };

            SaveProfile(profile);
        }

        private static void CreateBalancedProfile()
        {
            var profile = ScriptableObject.CreateInstance<OptimizationProfile>();
            profile.ProfileName = "Balanced";
            profile.Description = "Balanced optimization between performance and quality";
            profile.Settings = new SceneOptimizationSettings
            {
                EnableLODGeneration = true,
                LODLevels = 2,
                EnableMeshSimplification = true,
                TargetPolygonCount = 8000,
                EnableTextureCompression = true,
                EnableMaterialBatching = true,
                EnableShaderOptimization = true,
                EnableInstanceOptimization = true,
                SimilarityThreshold = 0.6f,
                EnableHierarchyFlattening = true,
                MaxHierarchyDepth = 3,
                EnableTransformOptimization = true
            };

            SaveProfile(profile);
        }

        private static void CreateQualityProfile()
        {
            var profile = ScriptableObject.CreateInstance<OptimizationProfile>();
            profile.ProfileName = "Quality";
            profile.Description = "Conservative optimization preserving visual quality";
            profile.Settings = new SceneOptimizationSettings
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
                MaxHierarchyDepth = 4,
                EnableTransformOptimization = true
            };

            SaveProfile(profile);
        }

        private static void SaveProfile(OptimizationProfile profile)
        {
            string assetPath = Path.Combine(PROFILES_FOLDER, $"{profile.ProfileName}.asset");
            AssetDatabase.CreateAsset(profile, assetPath);
            AssetDatabase.SaveAssets();
        }
    }
} 