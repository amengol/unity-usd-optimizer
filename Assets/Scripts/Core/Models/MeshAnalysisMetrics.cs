using System.Collections.Generic;

namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Contains metrics and analysis results for mesh data
    /// </summary>
    public class MeshAnalysisMetrics
    {
        /// <summary>
        /// Total number of polygons in the scene
        /// </summary>
        public int TotalPolygonCount { get; set; }

        /// <summary>
        /// Total number of vertices in the scene
        /// </summary>
        public int TotalVertexCount { get; set; }

        /// <summary>
        /// Average vertex density across all meshes
        /// </summary>
        public float AverageVertexDensity { get; set; }

        /// <summary>
        /// Dictionary mapping mesh names to their polygon counts
        /// </summary>
        public Dictionary<string, int> MeshPolygonCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Dictionary mapping mesh names to their vertex counts
        /// </summary>
        public Dictionary<string, int> MeshVertexCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Dictionary mapping mesh names to their vertex density metrics
        /// </summary>
        public Dictionary<string, VertexDensityMetrics> MeshVertexDensities { get; set; } = new Dictionary<string, VertexDensityMetrics>();

        /// <summary>
        /// Dictionary mapping mesh names to their UV mapping metrics
        /// </summary>
        public Dictionary<string, UVMappingMetrics> MeshUVMappings { get; set; } = new Dictionary<string, UVMappingMetrics>();

        /// <summary>
        /// List of meshes that exceed recommended polygon count thresholds
        /// </summary>
        public List<string> HighPolygonMeshes { get; set; } = new List<string>();

        /// <summary>
        /// List of meshes with potential UV mapping issues
        /// </summary>
        public List<string> ProblematicUVMeshes { get; set; } = new List<string>();
    }

    /// <summary>
    /// Contains metrics related to vertex density in a mesh
    /// </summary>
    public class VertexDensityMetrics
    {
        /// <summary>
        /// Number of vertices per unit of surface area
        /// </summary>
        public float VerticesPerUnitArea { get; set; }

        /// <summary>
        /// Surface area of the mesh
        /// </summary>
        public float SurfaceArea { get; set; }

        /// <summary>
        /// Whether the vertex density exceeds recommended thresholds
        /// </summary>
        public bool IsHighDensity { get; set; }
    }

    /// <summary>
    /// Contains metrics related to UV mapping in a mesh
    /// </summary>
    public class UVMappingMetrics
    {
        /// <summary>
        /// Number of UV sets in the mesh
        /// </summary>
        public int UVSetCount { get; set; }

        /// <summary>
        /// Whether the mesh has overlapping UVs
        /// </summary>
        public bool HasOverlappingUVs { get; set; }

        /// <summary>
        /// Whether the mesh has UV seams
        /// </summary>
        public bool HasUVSeams { get; set; }

        /// <summary>
        /// Whether the mesh has proper UV layout
        /// </summary>
        public bool HasProperUVLayout { get; set; }

        /// <summary>
        /// List of UV mapping issues found
        /// </summary>
        public List<string> UVIssues { get; set; } = new List<string>();
    }
} 