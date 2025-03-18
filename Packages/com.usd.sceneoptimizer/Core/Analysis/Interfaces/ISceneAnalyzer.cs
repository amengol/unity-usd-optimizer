using System.Threading.Tasks;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Analysis.Interfaces
{
    /// <summary>
    /// Interface for analyzing USD scenes and generating scene statistics
    /// </summary>
    public interface ISceneAnalyzer
    {
        /// <summary>
        /// Analyzes a USD scene to collect statistics and metrics
        /// </summary>
        /// <param name="scene">The scene to analyze</param>
        /// <returns>Analysis results containing scene metrics</returns>
        Task<AnalysisResults> AnalyzeSceneAsync(USDScene scene);
        
        /// <summary>
        /// Calculate optimization potential for a scene based on analysis results
        /// </summary>
        /// <param name="results">The analysis results to evaluate</param>
        /// <returns>Score indicating optimization potential (0-100)</returns>
        int CalculateOptimizationPotential(AnalysisResults results);
    }
} 