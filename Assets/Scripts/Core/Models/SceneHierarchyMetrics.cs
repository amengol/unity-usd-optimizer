using System.Collections.Generic;

namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Metrics for scene hierarchy analysis
    /// </summary>
    public class SceneHierarchyMetrics
    {
        /// <summary>
        /// Total number of nodes in the hierarchy
        /// </summary>
        public int TotalNodeCount { get; set; }

        /// <summary>
        /// Number of leaf nodes (nodes without children)
        /// </summary>
        public int LeafNodeCount { get; set; }

        /// <summary>
        /// Number of intermediate nodes (nodes with children)
        /// </summary>
        public int IntermediateNodeCount { get; set; }

        /// <summary>
        /// Mapping of node names to their child counts
        /// </summary>
        public Dictionary<string, int> NodeChildCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Mapping of node names to their depth in the hierarchy
        /// </summary>
        public Dictionary<string, int> NodeDepths { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// List of nodes with potential optimization opportunities
        /// </summary>
        public List<HierarchyOptimizationOpportunity> OptimizationOpportunities { get; set; } = new List<HierarchyOptimizationOpportunity>();
    }

    /// <summary>
    /// Metrics for transform analysis
    /// </summary>
    public class TransformAnalysisMetrics
    {
        /// <summary>
        /// Number of nodes with non-identity transforms
        /// </summary>
        public int NonIdentityTransformCount { get; set; }

        /// <summary>
        /// Number of nodes with non-uniform scales
        /// </summary>
        public int NonUniformScaleCount { get; set; }

        /// <summary>
        /// Number of nodes with non-zero rotations
        /// </summary>
        public int NonZeroRotationCount { get; set; }

        /// <summary>
        /// List of nodes with complex transforms that could be simplified
        /// </summary>
        public List<TransformOptimizationOpportunity> TransformOptimizationOpportunities { get; set; } = new List<TransformOptimizationOpportunity>();
    }

    /// <summary>
    /// Metrics for instance analysis
    /// </summary>
    public class InstanceAnalysisMetrics
    {
        /// <summary>
        /// Total number of instances found
        /// </summary>
        public int TotalInstanceCount { get; set; }

        /// <summary>
        /// Number of unique prototype objects
        /// </summary>
        public int UniquePrototypeCount { get; set; }

        /// <summary>
        /// Mapping of prototype names to their instance counts
        /// </summary>
        public Dictionary<string, int> PrototypeInstanceCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// List of potential instance optimization opportunities
        /// </summary>
        public List<InstanceOptimizationOpportunity> InstanceOptimizationOpportunities { get; set; } = new List<InstanceOptimizationOpportunity>();
    }

    /// <summary>
    /// Metrics for hierarchy complexity analysis
    /// </summary>
    public class HierarchyComplexityMetrics
    {
        /// <summary>
        /// Maximum depth of the hierarchy
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Average depth of leaf nodes
        /// </summary>
        public float AverageLeafDepth { get; set; }

        /// <summary>
        /// Number of nodes with high child counts
        /// </summary>
        public int HighChildCountNodeCount { get; set; }

        /// <summary>
        /// List of nodes with high complexity that could be simplified
        /// </summary>
        public List<ComplexityOptimizationOpportunity> ComplexityOptimizationOpportunities { get; set; } = new List<ComplexityOptimizationOpportunity>();
    }

    /// <summary>
    /// Represents a potential optimization opportunity in the hierarchy
    /// </summary>
    public class HierarchyOptimizationOpportunity
    {
        /// <summary>
        /// Name of the node with optimization potential
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// Type of optimization opportunity
        /// </summary>
        public OptimizationType Type { get; set; }

        /// <summary>
        /// Description of the optimization opportunity
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Estimated impact of the optimization
        /// </summary>
        public float EstimatedImpact { get; set; }
    }

    /// <summary>
    /// Represents a potential transform optimization opportunity
    /// </summary>
    public class TransformOptimizationOpportunity : HierarchyOptimizationOpportunity
    {
        /// <summary>
        /// Current transform matrix
        /// </summary>
        public Matrix4x4 CurrentTransform { get; set; }

        /// <summary>
        /// Suggested simplified transform
        /// </summary>
        public Matrix4x4 SuggestedTransform { get; set; }
    }

    /// <summary>
    /// Represents a potential instance optimization opportunity
    /// </summary>
    public class InstanceOptimizationOpportunity : HierarchyOptimizationOpportunity
    {
        /// <summary>
        /// Name of the prototype object
        /// </summary>
        public string PrototypeName { get; set; }

        /// <summary>
        /// Number of instances of this prototype
        /// </summary>
        public int InstanceCount { get; set; }
    }

    /// <summary>
    /// Represents a potential complexity optimization opportunity
    /// </summary>
    public class ComplexityOptimizationOpportunity : HierarchyOptimizationOpportunity
    {
        /// <summary>
        /// Current child count of the node
        /// </summary>
        public int CurrentChildCount { get; set; }

        /// <summary>
        /// Suggested child count after optimization
        /// </summary>
        public int SuggestedChildCount { get; set; }
    }

    /// <summary>
    /// Types of optimization opportunities
    /// </summary>
    public enum OptimizationType
    {
        /// <summary>
        /// Node can be removed
        /// </summary>
        RemovableNode,

        /// <summary>
        /// Node can be merged with parent
        /// </summary>
        MergeableNode,

        /// <summary>
        /// Node can be instanced
        /// </summary>
        InstanceableNode,

        /// <summary>
        /// Transform can be simplified
        /// </summary>
        SimplifiableTransform,

        /// <summary>
        /// Node has too many children
        /// </summary>
        HighChildCount,

        /// <summary>
        /// Node is too deep in hierarchy
        /// </summary>
        DeepNode
    }
} 