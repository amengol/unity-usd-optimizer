using UnityEngine;
using System.Collections.Generic;

namespace USDSceneOptimizer
{
    /// <summary>
    /// Interface for scene analysis functionality.
    /// </summary>
    public interface ISceneAnalyzer
    {
        /// <summary>
        /// Analyzes the current scene and returns analysis results.
        /// </summary>
        /// <returns>Analysis results containing metrics and recommendations.</returns>
        AnalysisResults AnalyzeScene();

        /// <summary>
        /// Analyzes a specific GameObject in the scene.
        /// </summary>
        /// <param name="gameObject">The GameObject to analyze.</param>
        /// <returns>Analysis results for the specific GameObject.</returns>
        GameObjectAnalysisResults AnalyzeGameObject(GameObject gameObject);
    }

    /// <summary>
    /// Contains the results of a scene analysis.
    /// </summary>
    public class AnalysisResults
    {
        public int TotalPolygons { get; set; }
        public int TotalVertices { get; set; }
        public int TotalMaterials { get; set; }
        public int TotalTextures { get; set; }
        public float TotalMemoryUsage { get; set; }
        public List<OptimizationRecommendation> Recommendations { get; set; }
    }

    /// <summary>
    /// Contains the results of analyzing a specific GameObject.
    /// </summary>
    public class GameObjectAnalysisResults
    {
        public string GameObjectName { get; set; }
        public int PolygonCount { get; set; }
        public int VertexCount { get; set; }
        public int MaterialCount { get; set; }
        public float MemoryUsage { get; set; }
        public List<OptimizationRecommendation> Recommendations { get; set; }
    }

    /// <summary>
    /// Represents a single optimization recommendation.
    /// </summary>
    public class OptimizationRecommendation
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public OptimizationPriority Priority { get; set; }
        public float EstimatedImprovement { get; set; }
    }

    /// <summary>
    /// Defines the priority level of an optimization recommendation.
    /// </summary>
    public enum OptimizationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
} 