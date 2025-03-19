using System;
using System.Collections.Generic;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Extensions
{
    /// <summary>
    /// Extension methods for the Mesh class
    /// </summary>
    public static class MeshExtensions
    {
        /// <summary>
        /// Gets the polygon count of the mesh
        /// </summary>
        public static int PolygonCount(this Mesh mesh)
        {
            if (mesh == null)
                return 0;
                
            // A mesh is typically represented as triangles, so we divide by 3
            return mesh.PolygonIndices?.Count / 3 ?? 0;
        }
        
        /// <summary>
        /// Gets the vertex count of the mesh
        /// </summary>
        public static int VertexCount(this Mesh mesh)
        {
            if (mesh == null)
                return 0;
                
            return mesh.Vertices?.Count ?? 0;
        }
        
        /// <summary>
        /// Gets the material of the mesh
        /// </summary>
        public static Material Material(this Mesh mesh)
        {
            // In this context, Mesh doesn't store materials directly
            // This is a placeholder to satisfy the code
            return null;
        }
    }
} 