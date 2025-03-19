using System;
using System.Linq;
using System.Threading.Tasks;
using USDOptimizer.Core.Analysis.Interfaces;
using USDOptimizer.Core.Logging;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Extensions;

namespace USDOptimizer.Core.Analysis.Implementations
{
    /// <summary>
    /// Implementation of IMeshAnalyzer for analyzing meshes in USD scenes
    /// </summary>
    public class MeshAnalyzer : IMeshAnalyzer
    {
        private readonly USDOptimizer.Core.Logging.ILogger _logger;
        
        // Thresholds for mesh complexity
        private const int HIGH_POLY_THRESHOLD = 10000;
        private const int BYTES_PER_VERTEX = 32; // Estimated memory per vertex
        
        public MeshAnalyzer(USDOptimizer.Core.Logging.ILogger logger = null)
        {
            _logger = logger ?? new UnityLogger();
        }
        
        /// <summary>
        /// Analyzes meshes in a scene to collect metrics
        /// </summary>
        public async Task<MeshMetrics> AnalyzeMeshesAsync(USDScene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }
            
            _logger.LogInfo($"Analyzing meshes in scene: {scene.Name}");
            
            var metrics = new MeshMetrics();
            
            await Task.Run(() => {
                // Handle case where scene has no meshes
                if (scene.Meshes == null || scene.Meshes.Count == 0)
                {
                    _logger.LogInfo("No meshes found in scene.");
                    return;
                }
                
                // Gather basic metrics
                metrics.TotalMeshes = scene.Meshes.Count;
                metrics.TotalPolygons = scene.Meshes.Sum(m => m.PolygonCount());
                metrics.TotalVertices = scene.Meshes.Sum(m => m.VertexCount());
                
                // Calculate average and maximum
                metrics.AveragePolygonsPerMesh = metrics.TotalMeshes > 0 
                    ? (float)metrics.TotalPolygons / metrics.TotalMeshes 
                    : 0;
                    
                metrics.MaxPolygonsInMesh = scene.Meshes.Count > 0 
                    ? scene.Meshes.Max(m => m.PolygonCount()) 
                    : 0;
                
                // Count high-poly meshes
                metrics.HighPolyMeshCount = scene.Meshes.Count(m => m.PolygonCount() > HIGH_POLY_THRESHOLD);
                
                // Estimate memory usage
                metrics.MemoryUsage = metrics.TotalVertices * BYTES_PER_VERTEX;
                
                _logger.LogInfo($"Mesh analysis complete. Found {metrics.TotalMeshes} meshes, " +
                    $"{metrics.TotalPolygons} polygons, {metrics.HighPolyMeshCount} high-poly meshes.");
            });
            
            return metrics;
        }
    }
} 