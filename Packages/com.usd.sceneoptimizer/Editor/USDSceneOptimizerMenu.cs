using UnityEngine;
using UnityEditor;

namespace USDOptimizer.Unity.Editor
{
    public static class USDSceneOptimizerMenu
    {
        [MenuItem("Window/USD Scene Optimizer")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<USDSceneOptimizerWindow>();
            window.titleContent = new GUIContent("USD Scene Optimizer");
            window.Show();
        }
    }
} 