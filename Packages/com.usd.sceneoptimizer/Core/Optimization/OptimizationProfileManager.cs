using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using USDOptimizer.Core.Logging;

namespace USDOptimizer.Core.Optimization
{
    public class OptimizationProfileManager
    {
        private static OptimizationProfileManager _instance;
        private readonly Dictionary<string, OptimizationProfile> _profiles;
        private readonly USDOptimizer.Core.Logging.ILogger _logger;

        public static OptimizationProfileManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OptimizationProfileManager();
                }
                return _instance;
            }
        }

        private OptimizationProfileManager()
        {
            _profiles = new Dictionary<string, OptimizationProfile>();
            _logger = new UnityLogger();
            LoadProfiles();
        }

        public string[] GetProfileNames()
        {
            return _profiles.Keys.ToArray();
        }

        public OptimizationProfile GetProfile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Profile name cannot be null or empty", nameof(name));
            }

            if (!_profiles.TryGetValue(name, out OptimizationProfile profile))
            {
                throw new KeyNotFoundException($"Profile '{name}' not found");
            }

            return profile;
        }

        public void LoadProfile(string name, SceneOptimizationSettings settings)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Profile name cannot be null or empty", nameof(name));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "Settings cannot be null");
            }

            if (!_profiles.TryGetValue(name, out OptimizationProfile profile))
            {
                throw new KeyNotFoundException($"Profile '{name}' not found");
            }

            profile.ApplyToSettings(settings);
            _logger.LogInfo($"Loaded optimization profile: {name}");
        }

        public void SaveProfile(string name, SceneOptimizationSettings settings)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Profile name cannot be null or empty", nameof(name));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "Settings cannot be null");
            }

            var profile = new OptimizationProfile
            {
                Name = name,
                Description = $"Profile created on {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                Settings = new SceneOptimizationSettings()
            };

            profile.LoadFromSettings(settings);
            _profiles[name] = profile;

            SaveProfiles();
            _logger.LogInfo($"Saved optimization profile: {name}");
        }

        public void DeleteProfile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Profile name cannot be null or empty", nameof(name));
            }

            if (!_profiles.Remove(name))
            {
                throw new KeyNotFoundException($"Profile '{name}' not found");
            }

            SaveProfiles();
            _logger.LogInfo($"Deleted optimization profile: {name}");
        }

        private void LoadProfiles()
        {
            try
            {
                string profilesPath = GetProfilesPath();
                if (!Directory.Exists(profilesPath))
                {
                    Directory.CreateDirectory(profilesPath);
                    CreateDefaultProfiles();
                    return;
                }

                string[] profileFiles = Directory.GetFiles(profilesPath, "*.asset");
                foreach (string file in profileFiles)
                {
                    try
                    {
                        var profile = ScriptableObject.CreateInstance<OptimizationProfile>();
                        JsonUtility.FromJsonOverwrite(File.ReadAllText(file), profile);
                        _profiles[profile.Name] = profile;
                        _logger.LogInfo($"Loaded profile: {profile.Name}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error loading profile from {file}: {ex.Message}");
                    }
                }

                if (_profiles.Count == 0)
                {
                    CreateDefaultProfiles();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading profiles: {ex.Message}");
            }
        }

        private void SaveProfiles()
        {
            try
            {
                string profilesPath = GetProfilesPath();
                if (!Directory.Exists(profilesPath))
                {
                    Directory.CreateDirectory(profilesPath);
                }

                foreach (var profile in _profiles.Values)
                {
                    try
                    {
                        string filePath = Path.Combine(profilesPath, $"{profile.Name}.asset");
                        string json = JsonUtility.ToJson(profile, true);
                        File.WriteAllText(filePath, json);
                        _logger.LogInfo($"Saved profile: {profile.Name}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error saving profile {profile.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving profiles: {ex.Message}");
            }
        }

        private void CreateDefaultProfiles()
        {
            // Create Performance profile
            var performanceProfile = new OptimizationProfile
            {
                Name = "Performance",
                Description = "Aggressive optimization for maximum performance",
                Settings = new SceneOptimizationSettings
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
                }
            };

            // Create Balanced profile
            var balancedProfile = new OptimizationProfile
            {
                Name = "Balanced",
                Description = "Balanced optimization between performance and quality",
                Settings = new SceneOptimizationSettings
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
                }
            };

            // Create Quality profile
            var qualityProfile = new OptimizationProfile
            {
                Name = "Quality",
                Description = "Conservative optimization preserving visual quality",
                Settings = new SceneOptimizationSettings
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
                }
            };

            _profiles[performanceProfile.Name] = performanceProfile;
            _profiles[balancedProfile.Name] = balancedProfile;
            _profiles[qualityProfile.Name] = qualityProfile;

            SaveProfiles();
            _logger.LogInfo("Created default optimization profiles");
        }

        private string GetProfilesPath()
        {
            return Path.Combine(Application.dataPath, "USD Scene Optimizer", "Profiles");
        }
    }
} 