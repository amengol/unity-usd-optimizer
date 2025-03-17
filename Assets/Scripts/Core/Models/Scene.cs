using System.Collections.Generic;

namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Represents a USD scene with its components
    /// </summary>
    public class Scene
    {
        /// <summary>
        /// Name of the scene
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to the USD file
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Collection of meshes in the scene
        /// </summary>
        public List<Mesh> Meshes { get; set; } = new List<Mesh>();

        /// <summary>
        /// Collection of materials in the scene
        /// </summary>
        public List<Material> Materials { get; set; } = new List<Material>();

        /// <summary>
        /// Collection of textures in the scene
        /// </summary>
        public List<Texture> Textures { get; set; } = new List<Texture>();

        /// <summary>
        /// Scene hierarchy information
        /// </summary>
        public SceneHierarchy Hierarchy { get; set; }

        /// <summary>
        /// Scene metadata and properties
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a mesh in the scene
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// Name of the mesh
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to the mesh in the scene hierarchy
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Collection of vertices in the mesh
        /// </summary>
        public List<Vector3> Vertices { get; set; } = new List<Vector3>();

        /// <summary>
        /// Collection of UV coordinates
        /// </summary>
        public List<Vector2> UVs { get; set; } = new List<Vector2>();

        /// <summary>
        /// Collection of polygon indices
        /// </summary>
        public List<int> PolygonIndices { get; set; } = new List<int>();

        /// <summary>
        /// Collection of normal vectors
        /// </summary>
        public List<Vector3> Normals { get; set; } = new List<Vector3>();

        /// <summary>
        /// Collection of tangents
        /// </summary>
        public List<Vector4> Tangents { get; set; } = new List<Vector4>();

        /// <summary>
        /// Bounding box of the mesh
        /// </summary>
        public Bounds Bounds { get; set; }

        /// <summary>
        /// Material assigned to the mesh
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Mesh metadata and properties
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a 3D vector
    /// </summary>
    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    /// <summary>
    /// Represents a 2D vector
    /// </summary>
    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Represents a 4D vector
    /// </summary>
    public struct Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }

    /// <summary>
    /// Represents a bounding box
    /// </summary>
    public struct Bounds
    {
        public Vector3 Center { get; set; }
        public Vector3 Size { get; set; }

        public Bounds(Vector3 center, Vector3 size)
        {
            Center = center;
            Size = size;
        }
    }
} 