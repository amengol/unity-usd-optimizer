using System.Collections.Generic;
using UnityEngine;

namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Represents a 3D mesh with vertices, UVs, and polygon indices
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// The name of the mesh
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The vertices of the mesh
        /// </summary>
        public List<Vector3> Vertices { get; set; } = new List<Vector3>();
        
        /// <summary>
        /// The UV coordinates of the mesh
        /// </summary>
        public List<Vector2> UVs { get; set; } = new List<Vector2>();
        
        /// <summary>
        /// The indices of the mesh's polygons (triangles)
        /// </summary>
        public List<int> PolygonIndices { get; set; } = new List<int>();
        
        /// <summary>
        /// The bounding box of the mesh
        /// </summary>
        public Bounds BoundingBox { get; set; }
    }
    
    /// <summary>
    /// Represents a bounding box with center and size
    /// </summary>
    public struct Bounds
    {
        /// <summary>
        /// The center of the bounding box
        /// </summary>
        public Vector3 Center { get; set; }
        
        /// <summary>
        /// The size of the bounding box
        /// </summary>
        public Vector3 Size { get; set; }
    }
} 