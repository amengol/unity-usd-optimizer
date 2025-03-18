using System.Threading.Tasks;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Analysis.Interfaces
{
    /// <summary>
    /// Interface for analyzing materials in USD scenes
    /// </summary>
    public interface IMaterialAnalyzer
    {
        /// <summary>
        /// Analyzes materials in a scene to collect metrics
        /// </summary>
        /// <param name="scene">The scene containing materials to analyze</param>
        /// <returns>Material metrics collected from the scene</returns>
        Task<MaterialMetrics> AnalyzeMaterialsAsync(USDScene scene);
    }
    
    /// <summary>
    /// Stores metrics related to materials and textures in a scene
    /// </summary>
    public class MaterialMetrics
    {
        /// <summary>
        /// Total number of materials in the scene
        /// </summary>
        public int TotalMaterials { get; set; }
        
        /// <summary>
        /// Total number of textures in the scene
        /// </summary>
        public int TotalTextures { get; set; }
        
        /// <summary>
        /// Estimated memory usage of all textures in bytes
        /// </summary>
        public long TextureMemoryUsage { get; set; }
        
        /// <summary>
        /// Number of unique materials in the scene
        /// </summary>
        public int UniqueMaterials { get; set; }
        
        /// <summary>
        /// Number of materials with high texture count
        /// </summary>
        public int HighTextureCountMaterials { get; set; }
        
        /// <summary>
        /// Number of high-resolution textures
        /// </summary>
        public int HighResTextureCount { get; set; }
        
        /// <summary>
        /// Maximum resolution of any texture (width × height)
        /// </summary>
        public int MaxTextureResolution { get; set; }
    }
} 