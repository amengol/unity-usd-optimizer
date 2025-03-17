using System.Threading.Tasks;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Optimization.Interfaces
{
    /// <summary>
    /// Interface for scene optimization operations
    /// </summary>
    public interface ISceneOptimizer
    {
        /// <summary>
        /// Optimizes instances in the scene by merging similar instances
        /// </summary>
        /// <param name="scene">The scene to optimize</param>
        /// <param name="similarityThreshold">Threshold for instance similarity (0-1)</param>
        /// <returns>Optimized scene</returns>
        Task<Scene> OptimizeInstancesAsync(Scene scene, float similarityThreshold);

        /// <summary>
        /// Flattens the scene hierarchy by combining transforms
        /// </summary>
        /// <param name="scene">The scene to optimize</param>
        /// <param name="maxDepth">Maximum depth for flattening</param>
        /// <returns>Optimized scene</returns>
        Task<Scene> FlattenHierarchyAsync(Scene scene, int maxDepth);

        /// <summary>
        /// Optimizes transforms in the scene by combining and simplifying them
        /// </summary>
        /// <param name="scene">The scene to optimize</param>
        /// <returns>Optimized scene</returns>
        Task<Scene> OptimizeTransformsAsync(Scene scene);

        /// <summary>
        /// Optimizes the entire scene by applying all optimization techniques
        /// </summary>
        /// <param name="scene">The scene to optimize</param>
        /// <param name="settings">Optimization settings</param>
        /// <returns>Optimized scene</returns>
        Task<Scene> OptimizeSceneAsync(Scene scene, SceneOptimizationSettings settings);
    }
} 