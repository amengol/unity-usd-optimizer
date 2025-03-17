using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using USDOptimizer.Core.Analysis.Interfaces;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Analysis.Implementations
{
    /// <summary>
    /// Implementation of IMeshAnalyzer for analyzing mesh data in USD scenes
    /// </summary>
    public class MeshAnalyzer : IMeshAnalyzer
    {
        private const int HIGH_POLYGON_THRESHOLD = 100000; // 100k polygons
        private const float HIGH_DENSITY_THRESHOLD = 1000f; // 1000 vertices per unit area

        public async Task<MeshAnalysisMetrics> AnalyzeMeshAsync(Scene scene)
        {
            var metrics = new MeshAnalysisMetrics();
            
            foreach (var mesh in scene.Meshes)
            {
                // Calculate polygon count
                var polygonCount = await CalculatePolygonCountAsync(mesh);
                metrics.MeshPolygonCounts[mesh.Name] = polygonCount;
                metrics.TotalPolygonCount += polygonCount;

                // Calculate vertex count
                var vertexCount = mesh.Vertices.Count;
                metrics.MeshVertexCounts[mesh.Name] = vertexCount;
                metrics.TotalVertexCount += vertexCount;

                // Calculate vertex density
                var densityMetrics = await CalculateVertexDensityAsync(mesh);
                metrics.MeshVertexDensities[mesh.Name] = densityMetrics;

                // Analyze UV mapping
                var uvMetrics = await AnalyzeUVMappingAsync(mesh);
                metrics.MeshUVMappings[mesh.Name] = uvMetrics;

                // Check for high polygon count
                if (polygonCount > HIGH_POLYGON_THRESHOLD)
                {
                    metrics.HighPolygonMeshes.Add(mesh.Name);
                }

                // Check for UV issues
                if (!uvMetrics.HasProperUVLayout || uvMetrics.HasOverlappingUVs)
                {
                    metrics.ProblematicUVMeshes.Add(mesh.Name);
                }
            }

            // Calculate average vertex density
            if (metrics.MeshVertexDensities.Count > 0)
            {
                metrics.AverageVertexDensity = metrics.MeshVertexDensities.Values
                    .Average(m => m.VerticesPerUnitArea);
            }

            return metrics;
        }

        public async Task<int> CalculatePolygonCountAsync(Mesh mesh)
        {
            // For a triangle mesh, polygon count is indices count divided by 3
            return mesh.PolygonIndices.Count / 3;
        }

        public async Task<VertexDensityMetrics> CalculateVertexDensityAsync(Mesh mesh)
        {
            var metrics = new VertexDensityMetrics();

            // Calculate surface area using the bounding box
            var bounds = mesh.Bounds;
            metrics.SurfaceArea = CalculateSurfaceArea(bounds);

            // Calculate vertices per unit area
            if (metrics.SurfaceArea > 0)
            {
                metrics.VerticesPerUnitArea = mesh.Vertices.Count / metrics.SurfaceArea;
            }

            // Check if density exceeds threshold
            metrics.IsHighDensity = metrics.VerticesPerUnitArea > HIGH_DENSITY_THRESHOLD;

            return metrics;
        }

        public async Task<UVMappingMetrics> AnalyzeUVMappingAsync(Mesh mesh)
        {
            var metrics = new UVMappingMetrics();

            // Count UV sets (assuming one UV set per mesh for now)
            metrics.UVSetCount = 1;

            // Check for overlapping UVs
            metrics.HasOverlappingUVs = CheckForOverlappingUVs(mesh.UVs);

            // Check for UV seams
            metrics.HasUVSeams = CheckForUVSeams(mesh.UVs, mesh.PolygonIndices);

            // Check for proper UV layout
            metrics.HasProperUVLayout = CheckForProperUVLayout(mesh.UVs);

            // Collect UV issues
            if (metrics.HasOverlappingUVs)
            {
                metrics.UVIssues.Add("Mesh has overlapping UV coordinates");
            }
            if (metrics.HasUVSeams)
            {
                metrics.UVIssues.Add("Mesh has UV seams");
            }
            if (!metrics.HasProperUVLayout)
            {
                metrics.UVIssues.Add("Mesh has improper UV layout");
            }

            return metrics;
        }

        private float CalculateSurfaceArea(Bounds bounds)
        {
            // Calculate surface area of the bounding box
            return 2 * (bounds.Size.X * bounds.Size.Y + 
                       bounds.Size.Y * bounds.Size.Z + 
                       bounds.Size.Z * bounds.Size.X);
        }

        private bool CheckForOverlappingUVs(List<Vector2> uvs)
        {
            // Simple overlap check using a grid-based approach
            const int gridSize = 100;
            var grid = new HashSet<(int, int)>();

            foreach (var uv in uvs)
            {
                var gridX = (int)(uv.X * gridSize);
                var gridY = (int)(uv.Y * gridSize);
                var key = (gridX, gridY);

                if (grid.Contains(key))
                {
                    return true;
                }

                grid.Add(key);
            }

            return false;
        }

        private bool CheckForUVSeams(List<Vector2> uvs, List<int> indices)
        {
            // Check for UV seams by looking for edges with large UV discontinuities
            const float seamThreshold = 0.5f;

            for (int i = 0; i < indices.Count; i += 3)
            {
                // Check each edge of the triangle
                for (int j = 0; j < 3; j++)
                {
                    var v1 = indices[i + j];
                    var v2 = indices[i + ((j + 1) % 3)];

                    var uv1 = uvs[v1];
                    var uv2 = uvs[v2];

                    var distance = Math.Sqrt(
                        Math.Pow(uv2.X - uv1.X, 2) + 
                        Math.Pow(uv2.Y - uv1.Y, 2)
                    );

                    if (distance > seamThreshold)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckForProperUVLayout(List<Vector2> uvs)
        {
            // Check if UVs are within [0,1] range and have reasonable distribution
            const float minUVDensity = 0.1f;
            const float maxUVDensity = 0.9f;

            var uvBounds = CalculateUVBounds(uvs);
            var uvArea = (uvBounds.MaxX - uvBounds.MinX) * (uvBounds.MaxY - uvBounds.MinY);

            // Check if UVs are within valid range
            if (uvBounds.MinX < 0 || uvBounds.MaxX > 1 || 
                uvBounds.MinY < 0 || uvBounds.MaxY > 1)
            {
                return false;
            }

            // Check if UV density is reasonable
            if (uvArea < minUVDensity || uvArea > maxUVDensity)
            {
                return false;
            }

            return true;
        }

        private (float MinX, float MaxX, float MinY, float MaxY) CalculateUVBounds(List<Vector2> uvs)
        {
            if (uvs.Count == 0)
            {
                return (0, 0, 0, 0);
            }

            var minX = uvs[0].X;
            var maxX = uvs[0].X;
            var minY = uvs[0].Y;
            var maxY = uvs[0].Y;

            foreach (var uv in uvs)
            {
                minX = Math.Min(minX, uv.X);
                maxX = Math.Max(maxX, uv.X);
                minY = Math.Min(minY, uv.Y);
                maxY = Math.Max(maxY, uv.Y);
            }

            return (minX, maxX, minY, maxY);
        }
    }
} 