using System.Threading.Tasks;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Analysis.Interfaces
{
    /// <summary>
    /// Interface for analyzing meshes in USD scenes
    /// </summary>
    public interface IMeshAnalyzer
    {
        /// <summary>
        /// Analyzes meshes in a scene to collect metrics
        /// </summary>
        /// <param name="scene">The scene containing meshes to analyze</param>
        /// <returns>Mesh metrics collected from the scene</returns>
        Task<MeshMetrics> AnalyzeMeshesAsync(USDScene scene);
    }
    
    /// <summary>
    /// Stores metrics related to meshes in a scene
    /// </summary>
    public class MeshMetrics
    {
        /// <summary>
        /// Total number of meshes in the scene
        /// </summary>
        public int TotalMeshes { get; set; }
        
        /// <summary>
        /// Total number of polygons across all meshes
        /// </summary>
        public int TotalPolygons { get; set; }
        
        /// <summary>
        /// Total number of vertices across all meshes
        /// </summary>
        public int TotalVertices { get; set; }
        
        /// <summary>
        /// Estimated memory usage of all meshes in bytes
        /// </summary>
        public long MemoryUsage { get; set; }
        
        /// <summary>
        /// Average number of polygons per mesh
        /// </summary>
        public float AveragePolygonsPerMesh { get; set; }
        
        /// <summary>
        /// Maximum polygon count found in any single mesh
        /// </summary>
        public int MaxPolygonsInMesh { get; set; }
        
        /// <summary>
        /// Number of high-poly meshes (above a certain threshold)
        /// </summary>
        public int HighPolyMeshCount { get; set; }
    }
} 