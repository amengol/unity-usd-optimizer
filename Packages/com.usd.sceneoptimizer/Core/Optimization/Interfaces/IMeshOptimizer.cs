using System.Threading.Tasks;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Optimization.Interfaces
{
    /// <summary>
    /// Interface for mesh optimization operations
    /// </summary>
    public interface IMeshOptimizer
    {
        /// <summary>
        /// Generates LODs for a mesh
        /// </summary>
        /// <param name="mesh">The mesh to generate LODs for</param>
        /// <param name="lodLevels">Number of LOD levels to generate</param>
        /// <param name="reductionFactors">Reduction factors for each LOD level (0-1)</param>
        /// <returns>Array of LOD meshes</returns>
        Task<USDOptimizer.Core.Models.Mesh[]> GenerateLODsAsync(USDOptimizer.Core.Models.Mesh mesh, int lodLevels, float[] reductionFactors);

        /// <summary>
        /// Simplifies a mesh by reducing polygon count while maintaining shape
        /// </summary>
        /// <param name="mesh">The mesh to simplify</param>
        /// <param name="targetPolygonCount">Target number of polygons</param>
        /// <returns>Simplified mesh</returns>
        Task<USDOptimizer.Core.Models.Mesh> SimplifyMeshAsync(USDOptimizer.Core.Models.Mesh mesh, int targetPolygonCount);

        /// <summary>
        /// Optimizes vertex data by removing redundant vertices and optimizing UVs
        /// </summary>
        /// <param name="mesh">The mesh to optimize</param>
        /// <returns>Optimized mesh</returns>
        Task<USDOptimizer.Core.Models.Mesh> OptimizeVerticesAsync(USDOptimizer.Core.Models.Mesh mesh);

        /// <summary>
        /// Merges multiple meshes into a single optimized mesh
        /// </summary>
        /// <param name="meshes">Array of meshes to merge</param>
        /// <returns>Merged and optimized mesh</returns>
        Task<USDOptimizer.Core.Models.Mesh> MergeMeshesAsync(USDOptimizer.Core.Models.Mesh[] meshes);
    }
} 