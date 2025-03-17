using UnityEngine;
using System.Collections.Generic;

namespace USDSceneOptimizer
{
    /// <summary>
    /// Interface for scene optimization functionality.
    /// </summary>
    public interface ISceneOptimizer
    {
        /// <summary>
        /// Applies optimizations to the current scene based on analysis results.
        /// </summary>
        /// <param name="analysisResults">The analysis results to base optimizations on.</param>
        /// <returns>Results of the optimization process.</returns>
        OptimizationResults OptimizeScene(AnalysisResults analysisResults);

        /// <summary>
        /// Applies optimizations to a specific GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to optimize.</param>
        /// <param name="analysisResults">The analysis results for the GameObject.</param>
        /// <returns>Results of the optimization process.</returns>
        GameObjectOptimizationResults OptimizeGameObject(GameObject gameObject, GameObjectAnalysisResults analysisResults);
    }

    /// <summary>
    /// Contains the results of a scene optimization process.
    /// </summary>
    public class OptimizationResults
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public float MemoryReduction { get; set; }
        public int PolygonReduction { get; set; }
        public int VertexReduction { get; set; }
        public List<OptimizationAction> AppliedActions { get; set; }
    }

    /// <summary>
    /// Contains the results of optimizing a specific GameObject.
    /// </summary>
    public class GameObjectOptimizationResults
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public float MemoryReduction { get; set; }
        public int PolygonReduction { get; set; }
        public int VertexReduction { get; set; }
        public List<OptimizationAction> AppliedActions { get; set; }
    }

    /// <summary>
    /// Represents a single optimization action that was applied.
    /// </summary>
    public class OptimizationAction
    {
        public string ActionType { get; set; }
        public string Description { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
} 