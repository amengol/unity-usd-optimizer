using System.Threading.Tasks;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Analysis.Interfaces
{
    /// <summary>
    /// Interface for analyzing mesh data in USD scenes
    /// </summary>
    public interface IMeshAnalyzer
    {
        /// <summary>
        /// Analyzes mesh data in the scene and returns metrics
        /// </summary>
        /// <param name="scene">The USD scene to analyze</param>
        /// <returns>Mesh analysis metrics</returns>
        Task<MeshAnalysisMetrics> AnalyzeMeshAsync(Scene scene);

        /// <summary>
        /// Calculates polygon count for a specific mesh
        /// </summary>
        /// <param name="mesh">The mesh to analyze</param>
        /// <returns>Number of polygons in the mesh</returns>
        Task<int> CalculatePolygonCountAsync(Mesh mesh);

        /// <summary>
        /// Calculates vertex density for a specific mesh
        /// </summary>
        /// <param name="mesh">The mesh to analyze</param>
        /// <returns>Vertex density metrics</returns>
        Task<VertexDensityMetrics> CalculateVertexDensityAsync(Mesh mesh);

        /// <summary>
        /// Analyzes UV mapping for a specific mesh
        /// </summary>
        /// <param name="mesh">The mesh to analyze</param>
        /// <returns>UV mapping analysis results</returns>
        Task<UVMappingMetrics> AnalyzeUVMappingAsync(Mesh mesh);
    }
} 