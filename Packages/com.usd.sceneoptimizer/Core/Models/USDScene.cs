using System;
using System.Collections.Generic;
using UnityEngine;

namespace USDOptimizer.Core.Models
{
    public class USDScene
    {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public DateTime ImportDate { get; set; }
        public List<USDNode> Nodes { get; set; } = new List<USDNode>();
        public List<Mesh> Meshes { get; set; } = new List<Mesh>();
        public List<Material> Materials { get; set; } = new List<Material>();
        public List<Texture> Textures { get; set; } = new List<Texture>();
        public SceneStatistics Statistics { get; set; } = new SceneStatistics();
        public List<OptimizationResult> OptimizationResults { get; set; } = new List<OptimizationResult>();
        
        // Property for backward compatibility
        public USDNode RootNode 
        { 
            get => Nodes.Count > 0 ? Nodes[0] : null;
            set 
            {
                if (Nodes.Count > 0)
                    Nodes[0] = value;
                else
                    Nodes.Add(value);
            }
        }
    }

    public class USDNode
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<USDNode> Children { get; set; } = new List<USDNode>();
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        
        // Additional properties for compatibility
        public Matrix4x4 Transform
        {
            get => Properties.ContainsKey("Transform") ? (Matrix4x4)Properties["Transform"] : Matrix4x4.identity;
            set => Properties["Transform"] = value;
        }
        
        public Mesh Mesh
        {
            get => Properties.ContainsKey("Mesh") ? (Mesh)Properties["Mesh"] : null;
            set => Properties["Mesh"] = value;
        }
        
        public Material Material
        {
            get => Properties.ContainsKey("Material") ? (Material)Properties["Material"] : null;
            set => Properties["Material"] = value;
        }
        
        public bool IsInstance
        {
            get => Properties.ContainsKey("IsInstance") && (bool)Properties["IsInstance"];
            set => Properties["IsInstance"] = value;
        }
        
        public string PrototypeName
        {
            get => Properties.ContainsKey("PrototypeName") ? (string)Properties["PrototypeName"] : null;
            set => Properties["PrototypeName"] = value;
        }
    }

    public class OptimizationResult
    {
        public string Type { get; set; }
        public int ItemsOptimized { get; set; }
        public string Notes { get; set; }
    }

    public class LODGroup
    {
        public string Name { get; set; }
        public Mesh OriginalMesh { get; set; }
        public List<LODLevel> LODLevels { get; set; } = new List<LODLevel>();
    }
    
    public class LODLevel
    {
        public int Level { get; set; }
        public float ScreenPercentage { get; set; }
        public Mesh Mesh { get; set; }
    }
}
