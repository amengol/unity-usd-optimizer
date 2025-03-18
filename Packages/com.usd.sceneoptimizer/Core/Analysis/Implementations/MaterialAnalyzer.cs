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
    /// Implementation of IMaterialAnalyzer for analyzing materials in USD scenes
    /// </summary>
    public class MaterialAnalyzer : IMaterialAnalyzer
    {
        private readonly USDOptimizer.Core.Logging.ILogger _logger;
        
        // Thresholds for material and texture analysis
        private const int HIGH_TEXTURE_COUNT_THRESHOLD = 3;
        private const int HIGH_RESOLUTION_THRESHOLD = 1024 * 1024; // 1K x 1K
        
        public MaterialAnalyzer(USDOptimizer.Core.Logging.ILogger logger = null)
        {
            _logger = logger ?? new UnityLogger();
        }
        
        /// <summary>
        /// Analyzes materials in a scene to collect metrics
        /// </summary>
        public async Task<MaterialMetrics> AnalyzeMaterialsAsync(USDScene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }
            
            _logger.LogInfo($"Analyzing materials in scene: {scene.Name}");
            
            var metrics = new MaterialMetrics();
            
            await Task.Run(() => {
                // Handle case where scene has no materials
                if (scene.Materials == null || scene.Materials.Count == 0)
                {
                    _logger.LogInfo("No materials found in scene.");
                    return;
                }
                
                // Gather basic material metrics
                metrics.TotalMaterials = scene.Materials.Count;
                
                // Count unique materials by name
                metrics.UniqueMaterials = scene.Materials
                    .Select(m => m.Name)
                    .Distinct()
                    .Count();
                
                // Handle case where scene has no textures
                if (scene.Textures == null || scene.Textures.Count == 0)
                {
                    _logger.LogInfo("No textures found in scene.");
                    metrics.TotalTextures = 0;
                    return;
                }
                
                // Gather texture metrics
                metrics.TotalTextures = scene.Textures.Count;
                
                // Calculate texture memory usage and resolution stats
                long totalMemory = 0;
                int maxResolution = 0;
                int highResTextureCount = 0;
                
                foreach (var texture in scene.Textures)
                {
                    int resolution = texture.Width * texture.Height;
                    maxResolution = Math.Max(maxResolution, resolution);
                    
                    if (resolution >= HIGH_RESOLUTION_THRESHOLD)
                    {
                        highResTextureCount++;
                    }
                    
                    // Estimate memory (4 bytes per pixel for RGBA)
                    totalMemory += (long)texture.Width * texture.Height * 4;
                }
                
                metrics.TextureMemoryUsage = totalMemory;
                metrics.MaxTextureResolution = maxResolution;
                metrics.HighResTextureCount = highResTextureCount;
                
                // Count materials with many textures
                // This is a placeholder - in a real implementation, we'd have relationships between materials and textures
                metrics.HighTextureCountMaterials = scene.Materials.Count > 0 && scene.Textures.Count > 0
                    ? scene.Textures.Count / scene.Materials.Count > HIGH_TEXTURE_COUNT_THRESHOLD ? scene.Materials.Count / 2 : 0
                    : 0;
                
                _logger.LogInfo($"Material analysis complete. Found {metrics.TotalMaterials} materials, " +
                    $"{metrics.TotalTextures} textures, {metrics.HighResTextureCount} high-res textures.");
            });
            
            return metrics;
        }
    }
} 