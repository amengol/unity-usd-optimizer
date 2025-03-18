using System;

namespace USDOptimizer.Core.Analysis.Interfaces
{
    /// <summary>
    /// Represents a recommendation for optimizing a USD scene
    /// </summary>
    [Serializable]
    public class OptimizationRecommendation
    {
        /// <summary>
        /// Title or summary of the recommendation
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Detailed description of the issue and potential solutions
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Priority level of the recommendation
        /// </summary>
        public OptimizationPriorityLevel Priority { get; set; }
        
        /// <summary>
        /// Category of the recommendation (e.g., Mesh, Material, Hierarchy)
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Potential performance impact of implementing this recommendation (0-100)
        /// </summary>
        public int ImpactScore { get; set; }
    }
    
    /// <summary>
    /// Priority levels for optimization recommendations
    /// </summary>
    public enum OptimizationPriorityLevel
    {
        /// <summary>
        /// Low priority, minor performance impact
        /// </summary>
        Low = 0,
        
        /// <summary>
        /// Medium priority, moderate performance impact
        /// </summary>
        Medium = 1,
        
        /// <summary>
        /// High priority, significant performance impact
        /// </summary>
        High = 2,
        
        /// <summary>
        /// Critical priority, severe performance impact
        /// </summary>
        Critical = 3
    }
} 