using System;
using System.Collections.Generic;

namespace USDOptimizer.Core.Analysis.Interfaces
{
    /// <summary>
    /// Stores analysis results for a USD scene including metrics and statistics
    /// </summary>
    [Serializable]
    public class AnalysisResults
    {
        /// <summary>
        /// Total number of nodes in the scene
        /// </summary>
        public int TotalNodes { get; set; }
        
        /// <summary>
        /// Total number of polygons in the scene's meshes
        /// </summary>
        public int TotalPolygons { get; set; }
        
        /// <summary>
        /// Total number of vertices in the scene's meshes
        /// </summary>
        public int TotalVertices { get; set; }
        
        /// <summary>
        /// Total number of materials in the scene
        /// </summary>
        public int TotalMaterials { get; set; }
        
        /// <summary>
        /// Total number of textures in the scene
        /// </summary>
        public int TotalTextures { get; set; }
        
        /// <summary>
        /// Size of the scene file in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Maximum hierarchy depth in the scene
        /// </summary>
        public int HierarchyDepth { get; set; }
        
        /// <summary>
        /// Dictionary mapping node types to their counts
        /// </summary>
        public Dictionary<string, int> NodeTypeCounts { get; set; } = new Dictionary<string, int>();
        
        /// <summary>
        /// Optimization potential score (0-100)
        /// </summary>
        public int OptimizationPotential { get; set; }
        
        /// <summary>
        /// List of optimization recommendations based on analysis
        /// </summary>
        public List<OptimizationRecommendation> Recommendations { get; set; } = new List<OptimizationRecommendation>();
        
        /// <summary>
        /// Total memory usage of the scene in bytes (meshes + textures)
        /// </summary>
        public long TotalMemoryUsage { get; set; }
        
        /// <summary>
        /// Mesh-specific metrics
        /// </summary>
        public MeshMetrics MeshMetrics { get; set; }
        
        /// <summary>
        /// Material-specific metrics
        /// </summary>
        public MaterialMetrics MaterialMetrics { get; set; }
        
        /// <summary>
        /// Hierarchy-specific metrics
        /// </summary>
        public HierarchyMetrics HierarchyMetrics { get; set; }
        
        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; } = DateTime.Now;
    }
} 