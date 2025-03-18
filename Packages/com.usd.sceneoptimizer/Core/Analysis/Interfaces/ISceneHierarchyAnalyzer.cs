using System.Collections.Generic;
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
        /// Analyzes scene hierarchy to collect metrics
        /// </summary>
        /// <param name="scene">The scene with hierarchy to analyze</param>
        /// <returns>Hierarchy metrics collected from the scene</returns>
        Task<HierarchyMetrics> AnalyzeHierarchyAsync(USDScene scene);
    }
    
    /// <summary>
    /// Stores metrics related to the scene hierarchy
    /// </summary>
    public class HierarchyMetrics
    {
        /// <summary>
        /// Total number of nodes in the scene
        /// </summary>
        public int TotalNodes { get; set; }
        
        /// <summary>
        /// Maximum depth of the scene hierarchy
        /// </summary>
        public int MaxHierarchyDepth { get; set; }
        
        /// <summary>
        /// Average number of children per node
        /// </summary>
        public float AverageChildrenPerNode { get; set; }
        
        /// <summary>
        /// Maximum number of children for any node
        /// </summary>
        public int MaxChildrenPerNode { get; set; }
        
        /// <summary>
        /// Number of empty nodes (no mesh, no material)
        /// </summary>
        public int EmptyNodeCount { get; set; }
        
        /// <summary>
        /// Number of deeply nested nodes (above a certain depth threshold)
        /// </summary>
        public int DeeplyNestedNodeCount { get; set; }
        
        /// <summary>
        /// Dictionary mapping node types to their counts
        /// </summary>
        public Dictionary<string, int> NodeTypeCounts { get; set; } = new Dictionary<string, int>();
    }
} 