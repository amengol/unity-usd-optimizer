using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using USDOptimizer.Core.Analysis.Interfaces;
using USDOptimizer.Core.Logging;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Extensions;

namespace USDOptimizer.Core.Analysis.Implementations
{
    /// <summary>
    /// Implementation of ISceneAnalyzer for analyzing USD scenes
    /// </summary>
    public class SceneAnalyzer : ISceneAnalyzer
    {
        private readonly USDOptimizer.Core.Logging.ILogger _logger;
        private readonly IMeshAnalyzer _meshAnalyzer;
        private readonly IMaterialAnalyzer _materialAnalyzer;
        private readonly ISceneHierarchyAnalyzer _hierarchyAnalyzer;
        
        public SceneAnalyzer(
            IMeshAnalyzer meshAnalyzer, 
            IMaterialAnalyzer materialAnalyzer, 
            ISceneHierarchyAnalyzer hierarchyAnalyzer,
            USDOptimizer.Core.Logging.ILogger logger = null)
        {
            _meshAnalyzer = meshAnalyzer ?? throw new ArgumentNullException(nameof(meshAnalyzer));
            _materialAnalyzer = materialAnalyzer ?? throw new ArgumentNullException(nameof(materialAnalyzer));
            _hierarchyAnalyzer = hierarchyAnalyzer ?? throw new ArgumentNullException(nameof(hierarchyAnalyzer));
            _logger = logger ?? new UnityLogger();
        }
        
        /// <summary>
        /// Legacy synchronous method for backward compatibility
        /// </summary>
        public AnalysisResults AnalyzeScene()
        {
            throw new NotImplementedException("Please use AnalyzeSceneAsync instead");
        }
        
        /// <summary>
        /// Analyzes a USD scene to collect statistics and metrics
        /// </summary>
        public async Task<AnalysisResults> AnalyzeSceneAsync(USDScene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }
            
            _logger.LogInfo($"Starting analysis of scene: {scene.Name}");
            
            var results = new AnalysisResults();
            
            // Run specialized analyzers in parallel
            var meshTask = _meshAnalyzer.AnalyzeMeshesAsync(scene);
            var materialTask = _materialAnalyzer.AnalyzeMaterialsAsync(scene);
            var hierarchyTask = _hierarchyAnalyzer.AnalyzeHierarchyAsync(scene);
            
            // Wait for all analyzers to complete
            await Task.WhenAll(meshTask, materialTask, hierarchyTask);
            
            // Store specialized metrics
            results.MeshMetrics = meshTask.Result;
            results.MaterialMetrics = materialTask.Result;
            results.HierarchyMetrics = hierarchyTask.Result;
            
            // Merge metrics into main results
            results.TotalNodes = results.HierarchyMetrics.TotalNodes;
            results.TotalPolygons = results.MeshMetrics.TotalPolygons;
            results.TotalVertices = results.MeshMetrics.TotalVertices;
            results.TotalMaterials = results.MaterialMetrics.TotalMaterials;
            results.TotalTextures = results.MaterialMetrics.TotalTextures;
            results.HierarchyDepth = results.HierarchyMetrics.MaxHierarchyDepth;
            results.NodeTypeCounts = results.HierarchyMetrics.NodeTypeCounts;
            
            // Calculate file and memory size
            results.FileSize = scene.FilePath != null ? GetFileSize(scene) : 0;
            results.TotalMemoryUsage = results.MeshMetrics.MemoryUsage + results.MaterialMetrics.TextureMemoryUsage;
            
            // Generate recommendations
            results.Recommendations = GenerateRecommendations(results);
            
            // Calculate optimization potential
            results.OptimizationPotential = CalculateOptimizationPotential(results);
            
            _logger.LogInfo($"Analysis completed. Found {results.TotalNodes} nodes, {results.TotalPolygons} polygons, {results.TotalMaterials} materials.");
            
            return results;
        }
        
        /// <summary>
        /// Calculates optimization potential score based on analysis results
        /// </summary>
        public int CalculateOptimizationPotential(AnalysisResults results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            
            var score = 0;
            
            // Evaluate mesh complexity
            if (results.TotalPolygons > 1000000) score += 20;
            else if (results.TotalPolygons > 500000) score += 15;
            else if (results.TotalPolygons > 100000) score += 10;
            else if (results.TotalPolygons > 50000) score += 5;
            
            // Evaluate hierarchy depth
            if (results.HierarchyDepth > 10) score += 20;
            else if (results.HierarchyDepth > 7) score += 15;
            else if (results.HierarchyDepth > 5) score += 10;
            else if (results.HierarchyDepth > 3) score += 5;
            
            // Evaluate material count
            if (results.TotalMaterials > 100) score += 20;
            else if (results.TotalMaterials > 50) score += 15;
            else if (results.TotalMaterials > 20) score += 10;
            else if (results.TotalMaterials > 10) score += 5;
            
            // Evaluate texture count
            if (results.TotalTextures > 100) score += 20;
            else if (results.TotalTextures > 50) score += 15;
            else if (results.TotalTextures > 20) score += 10;
            else if (results.TotalTextures > 10) score += 5;
            
            // Evaluate memory usage
            if (results.TotalMemoryUsage > 1000000000) score += 20; // > 1 GB
            else if (results.TotalMemoryUsage > 500000000) score += 15; // > 500 MB
            else if (results.TotalMemoryUsage > 100000000) score += 10; // > 100 MB
            else if (results.TotalMemoryUsage > 50000000) score += 5; // > 50 MB
            
            // Cap at 100
            return Math.Min(score, 100);
        }
        
        /// <summary>
        /// Generates optimization recommendations based on analysis results
        /// </summary>
        private List<OptimizationRecommendation> GenerateRecommendations(AnalysisResults results)
        {
            var recommendations = new List<OptimizationRecommendation>();
            
            // Mesh recommendations
            if (results.TotalPolygons > 1000000)
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "High Polygon Count",
                    Description = "The scene contains a very high polygon count which may impact performance. Consider simplifying meshes or using LODs.",
                    Priority = OptimizationPriorityLevel.Critical,
                    Category = "Mesh",
                    ImpactScore = 90
                });
            }
            else if (results.TotalPolygons > 500000)
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "Elevated Polygon Count",
                    Description = "The scene contains a high polygon count. Consider mesh simplification techniques.",
                    Priority = OptimizationPriorityLevel.High,
                    Category = "Mesh",
                    ImpactScore = 70
                });
            }
            
            // Hierarchy recommendations
            if (results.HierarchyDepth > 10)
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "Deep Hierarchy",
                    Description = "The scene has a very deep hierarchy which may impact performance. Consider flattening the scene structure.",
                    Priority = OptimizationPriorityLevel.High,
                    Category = "Hierarchy",
                    ImpactScore = 60
                });
            }
            
            // Material recommendations
            if (results.TotalMaterials > 100)
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "Excessive Materials",
                    Description = "The scene uses a large number of materials. Consider batching similar materials to reduce draw calls.",
                    Priority = OptimizationPriorityLevel.Medium,
                    Category = "Material",
                    ImpactScore = 50
                });
            }
            
            // Texture recommendations
            if (results.MaterialMetrics.HighResTextureCount > 10)
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "High Resolution Textures",
                    Description = "The scene contains many high-resolution textures. Consider downscaling or using mipmaps.",
                    Priority = OptimizationPriorityLevel.Medium,
                    Category = "Texture",
                    ImpactScore = 40
                });
            }
            
            // Memory usage recommendations
            if (results.TotalMemoryUsage > 1000000000) // > 1 GB
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Title = "High Memory Usage",
                    Description = "The scene requires a large amount of memory. Consider optimizing meshes and textures.",
                    Priority = OptimizationPriorityLevel.Critical,
                    Category = "Memory",
                    ImpactScore = 95
                });
            }
            
            return recommendations;
        }
        
        /// <summary>
        /// Gets the file size of the scene (placeholder implementation)
        /// </summary>
        private long GetFileSize(USDScene scene)
        {
            // In a real implementation, we would check the actual file size
            // This is a placeholder that estimates size based on scene complexity
            try
            {
                if (System.IO.File.Exists(scene.FilePath))
                {
                    return new System.IO.FileInfo(scene.FilePath).Length;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not get file size: {ex.Message}");
            }
            
            // Fallback to estimation
            long estimatedSize = 0;
            
            estimatedSize += scene.Nodes?.Count * 1024 ?? 0; // 1KB per node
            estimatedSize += scene.Meshes?.Sum(m => m.VertexCount()) ?? 0; // 32 bytes per vertex
            estimatedSize += scene.Materials?.Count * 2048 ?? 0; // 2KB per material
            estimatedSize += scene.Textures?.Sum(t => t.Width * t.Height * 4) ?? 0; // 4 bytes per pixel
            
            return estimatedSize;
        }
    }
} 