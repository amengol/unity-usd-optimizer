using System.Threading.Tasks;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Optimization.Interfaces
{
    /// <summary>
    /// Interface for material optimization operations
    /// </summary>
    public interface IMaterialOptimizer
    {
        /// <summary>
        /// Compresses textures in a material to reduce memory usage
        /// </summary>
        /// <param name="material">The material containing textures to compress</param>
        /// <param name="compressionQuality">Compression quality (0-1)</param>
        /// <returns>Material with compressed textures</returns>
        Task<Material> CompressTexturesAsync(Material material, float compressionQuality);

        /// <summary>
        /// Batches multiple materials into a single optimized material
        /// </summary>
        /// <param name="materials">Array of materials to batch</param>
        /// <returns>Batched material</returns>
        Task<Material> BatchMaterialsAsync(Material[] materials);

        /// <summary>
        /// Optimizes shader complexity by simplifying material properties
        /// </summary>
        /// <param name="material">The material to optimize</param>
        /// <param name="targetComplexity">Target shader complexity (0-1)</param>
        /// <returns>Material with optimized shader</returns>
        Task<Material> OptimizeShaderAsync(Material material, float targetComplexity);

        /// <summary>
        /// Merges similar materials to reduce draw calls
        /// </summary>
        /// <param name="materials">Array of materials to analyze and merge</param>
        /// <param name="similarityThreshold">Threshold for material similarity (0-1)</param>
        /// <returns>Array of merged materials</returns>
        Task<Material[]> MergeSimilarMaterialsAsync(Material[] materials, float similarityThreshold);
    }
} 