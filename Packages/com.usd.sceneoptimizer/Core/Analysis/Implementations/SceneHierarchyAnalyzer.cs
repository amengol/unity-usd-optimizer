using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using USDOptimizer.Core.Analysis.Interfaces;
using USDOptimizer.Core.Logging;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Analysis.Implementations
{
    /// <summary>
    /// Implementation of ISceneHierarchyAnalyzer for analyzing hierarchy in USD scenes
    /// </summary>
    public class SceneHierarchyAnalyzer : ISceneHierarchyAnalyzer
    {
        private readonly USDOptimizer.Core.Logging.ILogger _logger;
        
        // Thresholds for hierarchy analysis
        private const int DEEP_HIERARCHY_THRESHOLD = 5;
        
        public SceneHierarchyAnalyzer(USDOptimizer.Core.Logging.ILogger logger = null)
        {
            _logger = logger ?? new UnityLogger();
        }
        
        /// <summary>
        /// Analyzes scene hierarchy to collect metrics
        /// </summary>
        public async Task<HierarchyMetrics> AnalyzeHierarchyAsync(USDScene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }
            
            _logger.LogInfo($"Analyzing hierarchy in scene: {scene.Name}");
            
            var metrics = new HierarchyMetrics();
            
            await Task.Run(() => {
                // Handle case where scene has no nodes
                if (scene.Nodes == null || scene.Nodes.Count == 0)
                {
                    _logger.LogInfo("No nodes found in scene.");
                    return;
                }
                
                // Count total nodes
                metrics.TotalNodes = CountNodesRecursive(scene.Nodes);
                
                // Calculate hierarchy metrics
                CalculateHierarchyMetrics(scene.Nodes, metrics);
                
                // Count node types
                metrics.NodeTypeCounts = CountNodeTypes(scene.Nodes);
                
                _logger.LogInfo($"Hierarchy analysis complete. Found {metrics.TotalNodes} nodes, " +
                    $"max depth: {metrics.MaxHierarchyDepth}, empty nodes: {metrics.EmptyNodeCount}.");
            });
            
            return metrics;
        }
        
        /// <summary>
        /// Recursively counts all nodes in a hierarchy
        /// </summary>
        private int CountNodesRecursive(List<USDNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return 0;
            }
            
            int count = nodes.Count;
            
            foreach (var node in nodes)
            {
                if (node.Children != null)
                {
                    count += CountNodesRecursive(node.Children);
                }
            }
            
            return count;
        }
        
        /// <summary>
        /// Calculates various hierarchy metrics for a scene
        /// </summary>
        private void CalculateHierarchyMetrics(List<USDNode> rootNodes, HierarchyMetrics metrics)
        {
            int totalChildren = 0;
            int maxChildren = 0;
            int emptyNodeCount = 0;
            int deeplyNestedCount = 0;
            
            // Calculate max hierarchy depth and count empty nodes
            metrics.MaxHierarchyDepth = CalculateMaxDepthAndCountNodes(
                rootNodes, 
                1, 
                ref totalChildren, 
                ref maxChildren, 
                ref emptyNodeCount, 
                ref deeplyNestedCount);
            
            // Calculate average children per node
            int nonLeafNodes = metrics.TotalNodes - metrics.EmptyNodeCount;
            metrics.AverageChildrenPerNode = nonLeafNodes > 0 
                ? (float)totalChildren / nonLeafNodes 
                : 0;
                
            metrics.MaxChildrenPerNode = maxChildren;
            metrics.EmptyNodeCount = emptyNodeCount;
            metrics.DeeplyNestedNodeCount = deeplyNestedCount;
        }
        
        /// <summary>
        /// Recursively calculates max depth and counts various node types
        /// </summary>
        private int CalculateMaxDepthAndCountNodes(
            List<USDNode> nodes, 
            int currentDepth, 
            ref int totalChildren, 
            ref int maxChildren, 
            ref int emptyNodeCount, 
            ref int deeplyNestedCount)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return currentDepth - 1;
            }
            
            int maxDepth = currentDepth;
            
            foreach (var node in nodes)
            {
                // Count empty nodes (no mesh or material)
                if (node.Mesh == null && node.Material == null)
                {
                    emptyNodeCount++;
                }
                
                // Count deeply nested nodes
                if (currentDepth > DEEP_HIERARCHY_THRESHOLD)
                {
                    deeplyNestedCount++;
                }
                
                // Update max children
                if (node.Children != null)
                {
                    totalChildren += node.Children.Count;
                    maxChildren = Math.Max(maxChildren, node.Children.Count);
                    
                    // Recurse into children
                    int childDepth = CalculateMaxDepthAndCountNodes(
                        node.Children, 
                        currentDepth + 1, 
                        ref totalChildren, 
                        ref maxChildren, 
                        ref emptyNodeCount, 
                        ref deeplyNestedCount);
                        
                    maxDepth = Math.Max(maxDepth, childDepth);
                }
            }
            
            return maxDepth;
        }
        
        /// <summary>
        /// Counts the occurrence of each node type in the scene
        /// </summary>
        private Dictionary<string, int> CountNodeTypes(List<USDNode> nodes)
        {
            var typeCounts = new Dictionary<string, int>();
            
            CountNodeTypesRecursive(nodes, typeCounts);
            
            return typeCounts;
        }
        
        /// <summary>
        /// Recursively counts node types in a hierarchy
        /// </summary>
        private void CountNodeTypesRecursive(List<USDNode> nodes, Dictionary<string, int> typeCounts)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return;
            }
            
            foreach (var node in nodes)
            {
                // Determine node type
                string nodeType = "Transform"; // Default type
                
                if (node.Mesh != null)
                {
                    nodeType = "Mesh";
                }
                else if (node.Material != null)
                {
                    nodeType = "Material";
                }
                
                // Increment count for this type
                if (!typeCounts.ContainsKey(nodeType))
                {
                    typeCounts[nodeType] = 0;
                }
                typeCounts[nodeType]++;
                
                // Recurse into children
                if (node.Children != null)
                {
                    CountNodeTypesRecursive(node.Children, typeCounts);
                }
            }
        }
    }
} 