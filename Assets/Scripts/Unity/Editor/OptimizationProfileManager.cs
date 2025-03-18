using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Unity.Editor
{
    /// <summary>
    /// Manages optimization profiles, including saving, loading, and applying profiles
    /// </summary>
    public class OptimizationProfileManager
    {
        private const string PROFILES_FOLDER = "Assets/USDSceneOptimizer/Profiles";
        private static OptimizationProfileManager _instance;
        private Dictionary<string, OptimizationProfile> _profiles;

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
            LoadProfiles();
        }

        private void LoadProfiles()
        {
            _profiles.Clear();

            // Create profiles folder if it doesn't exist
            if (!Directory.Exists(PROFILES_FOLDER))
            {
                Directory.CreateDirectory(PROFILES_FOLDER);
                AssetDatabase.Refresh();
            }

            // Load all profile assets
            string[] guids = AssetDatabase.FindAssets("t:OptimizationProfile", new[] { PROFILES_FOLDER });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                OptimizationProfile profile = AssetDatabase.LoadAssetAtPath<OptimizationProfile>(path);
                if (profile != null)
                {
                    _profiles[profile.ProfileName] = profile;
                }
            }
        }

        public void SaveProfile(string name, string description, SceneOptimizationSettings settings)
        {
            // Create new profile
            OptimizationProfile profile = ScriptableObject.CreateInstance<OptimizationProfile>();
            profile.ProfileName = name;
            profile.Description = description;
            profile.LoadSettings(settings);

            // Save asset
            string path = Path.Combine(PROFILES_FOLDER, $"{name}.asset");
            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();

            // Update dictionary
            _profiles[name] = profile;
        }

        public void LoadProfile(string name, SceneOptimizationSettings targetSettings)
        {
            if (_profiles.TryGetValue(name, out OptimizationProfile profile))
            {
                profile.ApplySettings(targetSettings);
            }
            else
            {
                Debug.LogError($"Profile '{name}' not found.");
            }
        }

        public void DeleteProfile(string name)
        {
            if (_profiles.TryGetValue(name, out OptimizationProfile profile))
            {
                string path = AssetDatabase.GetAssetPath(profile);
                AssetDatabase.DeleteAsset(path);
                _profiles.Remove(name);
            }
            else
            {
                Debug.LogError($"Profile '{name}' not found.");
            }
        }

        public string[] GetProfileNames()
        {
            return new List<string>(_profiles.Keys).ToArray();
        }

        public OptimizationProfile GetProfile(string name)
        {
            return _profiles.TryGetValue(name, out OptimizationProfile profile) ? profile : null;
        }

        public void Refresh()
        {
            LoadProfiles();
        }
    }
} 