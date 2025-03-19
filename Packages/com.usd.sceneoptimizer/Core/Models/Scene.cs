using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Represents a 3D scene with meshes, materials, and a hierarchy of nodes
    /// </summary>
    public class Scene
    {
        /// <summary>
        /// The name of the scene
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The root node of the scene hierarchy
        /// </summary>
        public Node RootNode { get; set; } = new Node { Name = "Root" };
        
        /// <summary>
        /// List of all meshes in the scene
        /// </summary>
        public List<Mesh> Meshes { get; set; } = new List<Mesh>();
        
        /// <summary>
        /// List of all materials in the scene
        /// </summary>
        public List<Material> Materials { get; set; } = new List<Material>();
        
        /// <summary>
        /// Statistics about the scene
        /// </summary>
        public SceneStatistics Statistics { get; set; } = new SceneStatistics();

        /// <summary>
        /// Converts a USDScene to a Scene
        /// </summary>
        public static Scene FromUSDScene(USDScene usdScene)
        {
            if (usdScene == null) return null;
            
            var scene = new Scene
            {
                Name = usdScene.Name,
                Meshes = usdScene.Meshes?.ToList() ?? new List<Mesh>(),
                Materials = usdScene.Materials?.ToList() ?? new List<Material>()
            };
            
            if (usdScene.RootNode != null)
            {
                scene.RootNode = ConvertNodeFromUSDNode(usdScene.RootNode, scene);
            }
            
            // Convert statistics
            if (usdScene.Statistics != null)
            {
                scene.Statistics = new SceneStatistics
                {
                    TotalNodes = usdScene.Statistics.TotalNodes,
                    TotalVertices = usdScene.Statistics.TotalVertices,
                    TotalPolygons = usdScene.Statistics.TotalPolygons,
                    TotalMaterials = usdScene.Statistics.TotalMaterials,
                    EstimatedMemoryUsageMB = 0,
                    EstimatedFileSizeMB = usdScene.Statistics.TotalFileSize
                };
            }
            
            return scene;
        }
        
        private static Node ConvertNodeFromUSDNode(USDNode usdNode, Scene scene)
        {
            if (usdNode == null) return null;
            
            var node = new Node
            {
                Name = usdNode.Name,
                Transform = usdNode.Transform,
                Mesh = usdNode.Mesh,
                Material = usdNode.Material,
                IsInstance = usdNode.IsInstance,
                InstanceSource = usdNode.PrototypeName,
                Children = new List<Node>()
            };
            
            // Convert children
            if (usdNode.Children != null)
            {
                foreach (var childUSDNode in usdNode.Children)
                {
                    var childNode = ConvertNodeFromUSDNode(childUSDNode, scene);
                    if (childNode != null)
                    {
                        node.Children.Add(childNode);
                    }
                }
            }
            
            return node;
        }
        
        /// <summary>
        /// Converts a Scene to a USDScene
        /// </summary>
        public USDScene ToUSDScene()
        {
            var usdScene = new USDScene
            {
                Name = this.Name,
                Meshes = this.Meshes?.ToList() ?? new List<Mesh>(),
                Materials = this.Materials?.ToList() ?? new List<Material>()
            };
            
            if (this.RootNode != null)
            {
                usdScene.RootNode = ConvertNodeToUSDNode(this.RootNode);
            }
            
            // Convert statistics
            if (this.Statistics != null)
            {
                usdScene.Statistics = new SceneStatistics
                {
                    TotalNodes = this.Statistics.TotalNodes,
                    TotalVertices = this.Statistics.TotalVertices,
                    TotalPolygons = this.Statistics.TotalPolygons,
                    TotalMaterials = this.Statistics.TotalMaterials,
                    TotalFileSize = this.Statistics.EstimatedFileSizeMB
                };
            }
            
            return usdScene;
        }
        
        private static USDNode ConvertNodeToUSDNode(Node node)
        {
            if (node == null) return null;
            
            var usdNode = new USDNode
            {
                Name = node.Name,
                Transform = node.Transform,
                Mesh = node.Mesh,
                Material = node.Material,
                IsInstance = node.IsInstance,
                PrototypeName = node.InstanceSource,
                Children = new List<USDNode>()
            };
            
            // Convert children
            if (node.Children != null)
            {
                foreach (var childNode in node.Children)
                {
                    var childUSDNode = ConvertNodeToUSDNode(childNode);
                    if (childUSDNode != null)
                    {
                        usdNode.Children.Add(childUSDNode);
                    }
                }
            }
            
            return usdNode;
        }
    }
    
    /// <summary>
    /// Statistics about a scene
    /// </summary>
    public class SceneStatistics
    {
        public int TotalNodes { get; set; }
        public int TotalInstances { get; set; }
        public int TotalPolygons { get; set; }
        public int TotalVertices { get; set; }
        public int TotalMaterials { get; set; }
        public float EstimatedMemoryUsageMB { get; set; }
        public float EstimatedFileSizeMB { get; set; }
        
        // Missing properties from errors
        public int TotalTextures { get; set; }
        public float TotalFileSize { get; set; }
        public Dictionary<string, int> NodeTypeCounts { get; set; } = new Dictionary<string, int>();
    }
} 