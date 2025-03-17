using System.Collections.Generic;

namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Overall performance metrics for a USD scene
    /// </summary>
    public class PerformanceMetrics
    {
        /// <summary>
        /// Memory usage metrics
        /// </summary>
        public MemoryUsageMetrics MemoryUsage { get; set; }

        /// <summary>
        /// Draw call metrics
        /// </summary>
        public DrawCallMetrics DrawCalls { get; set; }

        /// <summary>
        /// Estimated frame time in milliseconds
        /// </summary>
        public float EstimatedFrameTime { get; set; }

        /// <summary>
        /// Estimated memory bandwidth usage in MB/s
        /// </summary>
        public float EstimatedMemoryBandwidth { get; set; }

        /// <summary>
        /// List of performance optimization opportunities
        /// </summary>
        public List<PerformanceOptimizationOpportunity> OptimizationOpportunities { get; set; } = new List<PerformanceOptimizationOpportunity>();
    }

    /// <summary>
    /// Memory usage metrics for a USD scene
    /// </summary>
    public class MemoryUsageMetrics
    {
        /// <summary>
        /// Total memory usage in MB
        /// </summary>
        public float TotalMemoryUsageMB { get; set; }

        /// <summary>
        /// Memory usage by component type
        /// </summary>
        public Dictionary<string, float> MemoryUsageByComponent { get; set; } = new Dictionary<string, float>();

        /// <summary>
        /// Memory usage by mesh
        /// </summary>
        public Dictionary<string, float> MemoryUsageByMesh { get; set; } = new Dictionary<string, float>();

        /// <summary>
        /// Memory usage by texture
        /// </summary>
        public Dictionary<string, float> MemoryUsageByTexture { get; set; } = new Dictionary<string, float>();

        /// <summary>
        /// List of memory optimization opportunities
        /// </summary>
        public List<MemoryOptimizationOpportunity> OptimizationOpportunities { get; set; } = new List<MemoryOptimizationOpportunity>();
    }

    /// <summary>
    /// Draw call metrics for a USD scene
    /// </summary>
    public class DrawCallMetrics
    {
        /// <summary>
        /// Total number of draw calls
        /// </summary>
        public int TotalDrawCalls { get; set; }

        /// <summary>
        /// Number of draw calls by material
        /// </summary>
        public Dictionary<string, int> DrawCallsByMaterial { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Number of draw calls by mesh
        /// </summary>
        public Dictionary<string, int> DrawCallsByMesh { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Number of potential batches
        /// </summary>
        public int PotentialBatches { get; set; }

        /// <summary>
        /// Number of current batches
        /// </summary>
        public int CurrentBatches { get; set; }

        /// <summary>
        /// List of batching optimization opportunities
        /// </summary>
        public List<BatchingOptimizationOpportunity> OptimizationOpportunities { get; set; } = new List<BatchingOptimizationOpportunity>();
    }

    /// <summary>
    /// Represents a performance optimization opportunity
    /// </summary>
    public class PerformanceOptimizationOpportunity
    {
        /// <summary>
        /// Type of optimization opportunity
        /// </summary>
        public PerformanceOptimizationType Type { get; set; }

        /// <summary>
        /// Description of the optimization opportunity
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Estimated performance impact (0-1)
        /// </summary>
        public float EstimatedImpact { get; set; }

        /// <summary>
        /// Estimated memory savings in MB
        /// </summary>
        public float EstimatedMemorySavingsMB { get; set; }

        /// <summary>
        /// Estimated frame time improvement in ms
        /// </summary>
        public float EstimatedFrameTimeImprovementMS { get; set; }
    }

    /// <summary>
    /// Represents a memory optimization opportunity
    /// </summary>
    public class MemoryOptimizationOpportunity : PerformanceOptimizationOpportunity
    {
        /// <summary>
        /// Current memory usage in MB
        /// </summary>
        public float CurrentMemoryUsageMB { get; set; }

        /// <summary>
        /// Target memory usage in MB
        /// </summary>
        public float TargetMemoryUsageMB { get; set; }

        /// <summary>
        /// Component or asset that can be optimized
        /// </summary>
        public string TargetComponent { get; set; }
    }

    /// <summary>
    /// Represents a batching optimization opportunity
    /// </summary>
    public class BatchingOptimizationOpportunity : PerformanceOptimizationOpportunity
    {
        /// <summary>
        /// Current number of draw calls
        /// </summary>
        public int CurrentDrawCalls { get; set; }

        /// <summary>
        /// Potential number of draw calls after batching
        /// </summary>
        public int PotentialDrawCalls { get; set; }

        /// <summary>
        /// List of materials that can be batched
        /// </summary>
        public List<string> BatchableMaterials { get; set; } = new List<string>();
    }

    /// <summary>
    /// Types of performance optimization opportunities
    /// </summary>
    public enum PerformanceOptimizationType
    {
        /// <summary>
        /// Memory optimization opportunity
        /// </summary>
        MemoryOptimization,

        /// <summary>
        /// Draw call optimization opportunity
        /// </summary>
        DrawCallOptimization,

        /// <summary>
        /// Batching optimization opportunity
        /// </summary>
        BatchingOptimization,

        /// <summary>
        /// Texture optimization opportunity
        /// </summary>
        TextureOptimization,

        /// <summary>
        /// Mesh optimization opportunity
        /// </summary>
        MeshOptimization,

        /// <summary>
        /// Material optimization opportunity
        /// </summary>
        MaterialOptimization
    }
} 