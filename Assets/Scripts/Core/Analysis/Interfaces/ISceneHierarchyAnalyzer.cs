using System.Threading.Tasks;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Analysis.Interfaces
{
    /// <summary>
    /// Interface for analyzing scene hierarchy in USD scenes
    /// </summary>
    public interface ISceneHierarchyAnalyzer
    {
        /// <summary>
        /// Analyzes the scene hierarchy and returns metrics
        /// </summary>
        /// <param name="scene">The scene to analyze</param>
        /// <returns>Scene hierarchy analysis metrics</returns>
        Task<SceneHierarchyMetrics> AnalyzeHierarchyAsync(Scene scene);

        /// <summary>
        /// Analyzes transform data in the scene hierarchy
        /// </summary>
        /// <param name="scene">The scene to analyze</param>
        /// <returns>Transform analysis metrics</returns>
        Task<TransformAnalysisMetrics> AnalyzeTransformsAsync(Scene scene);

        /// <summary>
        /// Detects instances in the scene hierarchy
        /// </summary>
        /// <param name="scene">The scene to analyze</param>
        /// <returns>Instance analysis metrics</returns>
        Task<InstanceAnalysisMetrics> DetectInstancesAsync(Scene scene);

        /// <summary>
        /// Analyzes the depth and complexity of the scene hierarchy
        /// </summary>
        /// <param name="scene">The scene to analyze</param>
        /// <returns>Hierarchy complexity metrics</returns>
        Task<HierarchyComplexityMetrics> AnalyzeHierarchyComplexityAsync(Scene scene);
    }
} 