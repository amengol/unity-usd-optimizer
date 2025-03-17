using System.Threading.Tasks;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Analysis.Interfaces
{
    /// <summary>
    /// Interface for collecting performance metrics from USD scenes
    /// </summary>
    public interface IPerformanceMetricsCollector
    {
        /// <summary>
        /// Collects memory usage metrics for the scene
        /// </summary>
        /// <param name="scene">The scene to analyze</param>
        /// <returns>Memory usage metrics</returns>
        Task<MemoryUsageMetrics> CollectMemoryUsageAsync(Scene scene);

        /// <summary>
        /// Analyzes draw calls and batching potential
        /// </summary>
        /// <param name="scene">The scene to analyze</param>
        /// <returns>Draw call analysis metrics</returns>
        Task<DrawCallMetrics> AnalyzeDrawCallsAsync(Scene scene);

        /// <summary>
        /// Collects overall performance metrics
        /// </summary>
        /// <param name="scene">The scene to analyze</param>
        /// <returns>Overall performance metrics</returns>
        Task<PerformanceMetrics> CollectPerformanceMetricsAsync(Scene scene);
    }
} 