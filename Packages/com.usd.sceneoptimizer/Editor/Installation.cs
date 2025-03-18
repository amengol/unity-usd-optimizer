using UnityEngine;
using UnityEditor;
using System.IO;

namespace USDOptimizer.Unity.Editor
{
    public class Installation
    {
        [MenuItem("USD Scene Optimizer/Install")]
        public static void Install()
        {
            // Create necessary directories
            string[] directories = new string[]
            {
                "Assets/Scripts/Core",
                "Assets/Scripts/Core/Analysis",
                "Assets/Scripts/Core/Optimization",
                "Assets/Scripts/Core/Models",
                "Assets/Scripts/Core/Logging",
                "Assets/Scripts/Unity/Editor",
                "Assets/Scripts/Tests/Core",
                "Assets/Scripts/Tests/Unity/Editor",
                "Assets/Samples/TestScenes"
            };

            foreach (string dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Debug.Log($"Created directory: {dir}");
                }
            }

            // Create default optimization profiles
            DefaultOptimizationProfiles.CreateDefaultProfiles();

            // Refresh the Asset Database
            AssetDatabase.Refresh();

            Debug.Log("USD Scene Optimizer installed successfully!");
            EditorUtility.DisplayDialog("Installation Complete", 
                "USD Scene Optimizer has been installed successfully.\n\n" +
                "You can now access it from Window > USD Scene Optimizer", 
                "OK");
        }
    }
} 