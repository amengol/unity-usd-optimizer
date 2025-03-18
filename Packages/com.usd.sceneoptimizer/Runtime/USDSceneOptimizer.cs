using UnityEngine;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization;

namespace USDSceneOptimizer
{
    /// <summary>
    /// Main entry point for the USD Scene Optimizer
    /// </summary>
    public class USDSceneOptimizer : MonoBehaviour
    {
        private SceneOptimizer _sceneOptimizer;
        private SceneOptimizationSettings _settings;
        
        /// <summary>
        /// Initialize the optimizer
        /// </summary>
        public void Initialize()
        {
            _sceneOptimizer = new SceneOptimizer();
            _settings = new SceneOptimizationSettings();
            Debug.Log("USD Scene Optimizer initialized");
        }
        
        /// <summary>
        /// Optimize a USD scene with the current settings
        /// </summary>
        /// <param name="scene">The scene to optimize</param>
        /// <returns>The optimized scene</returns>
        public USDScene OptimizeScene(USDScene scene)
        {
            return _sceneOptimizer.OptimizeSceneAsync(scene, _settings).Result;
        }
        
        /// <summary>
        /// Set the optimization settings
        /// </summary>
        /// <param name="settings">The settings to apply</param>
        public void SetSettings(SceneOptimizationSettings settings)
        {
            _settings = settings;
        }
    }
} 