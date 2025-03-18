namespace USDOptimizer.Core.Optimization
{
    /// <summary>
    /// Defines priorities for optimization operations
    /// </summary>
    public enum OptimizationPriority
    {
        /// <summary>
        /// Lowest priority, minimal optimization
        /// </summary>
        Low = 0,
        
        /// <summary>
        /// Medium priority, balanced optimization
        /// </summary>
        Medium = 1,
        
        /// <summary>
        /// High priority, aggressive optimization
        /// </summary>
        High = 2,
        
        /// <summary>
        /// Critical priority, maximum optimization
        /// </summary>
        Critical = 3,
        
        /// <summary>
        /// Focus on quality, with minimal optimization
        /// </summary>
        Quality = 10,
        
        /// <summary>
        /// Balanced approach between quality and performance
        /// </summary>
        Balanced = 11,
        
        /// <summary>
        /// Focus on performance, with more aggressive optimization
        /// </summary>
        Performance = 12,
        
        /// <summary>
        /// Maximum optimization for highest performance, with quality tradeoffs
        /// </summary>
        MaxPerformance = 13
    }
} 