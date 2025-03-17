namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Settings for scene optimization
    /// </summary>
    public class SceneOptimizationSettings
    {
        /// <summary>
        /// Whether to optimize instances
        /// </summary>
        public bool OptimizeInstances { get; set; } = true;

        /// <summary>
        /// Threshold for instance similarity (0-1)
        /// </summary>
        public float InstanceSimilarityThreshold { get; set; } = 0.8f;

        /// <summary>
        /// Whether to flatten hierarchy
        /// </summary>
        public bool FlattenHierarchy { get; set; } = true;

        /// <summary>
        /// Maximum depth for hierarchy flattening
        /// </summary>
        public int MaxFlattenDepth { get; set; } = 3;

        /// <summary>
        /// Whether to optimize transforms
        /// </summary>
        public bool OptimizeTransforms { get; set; } = true;

        /// <summary>
        /// Whether to optimize meshes
        /// </summary>
        public bool OptimizeMeshes { get; set; } = true;

        /// <summary>
        /// Whether to optimize materials
        /// </summary>
        public bool OptimizeMaterials { get; set; } = true;

        /// <summary>
        /// Whether to optimize textures
        /// </summary>
        public bool OptimizeTextures { get; set; } = true;

        /// <summary>
        /// Target memory usage in MB
        /// </summary>
        public float TargetMemoryUsageMB { get; set; } = 1024f;

        /// <summary>
        /// Target draw call count
        /// </summary>
        public int TargetDrawCallCount { get; set; } = 100;

        /// <summary>
        /// Whether to preserve original scene
        /// </summary>
        public bool PreserveOriginal { get; set; } = true;
    }
} 