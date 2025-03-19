using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Logging;
using USDOptimizer.Core.Optimization.Interfaces;

namespace USDOptimizer.Core.Optimization.Implementations
{
    /// <summary>
    /// Implementation of scene optimization strategies for USD scenes
    /// </summary>
    public class SceneOptimizer : ISceneOptimizer
    {
        private readonly ILogger _logger;
        private readonly IMeshOptimizer _meshOptimizer;
        private readonly IMaterialOptimizer _materialOptimizer;

        public SceneOptimizer(IMeshOptimizer meshOptimizer, IMaterialOptimizer materialOptimizer, ILogger logger = null)
        {
            _meshOptimizer = meshOptimizer ?? throw new ArgumentNullException(nameof(meshOptimizer));
            _materialOptimizer = materialOptimizer ?? throw new ArgumentNullException(nameof(materialOptimizer));
            _logger = logger ?? new UnityLogger();
        }

        /// <summary>
        /// Optimizes the entire scene by applying all optimization techniques
        /// </summary>
        /// <param name="scene">The scene to optimize</param>
        /// <param name="settings">Optimization settings</param>
        /// <returns>Optimized scene</returns>
        public async Task<Scene> OptimizeSceneAsync(Scene scene, SceneOptimizationSettings settings)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _logger?.LogInfo($"Starting optimization of scene: {scene.Name}");

            // Create a copy of the scene to optimize
            Scene optimizedScene = new Scene
            {
                Name = $"{scene.Name}_Optimized",
                RootNode = CloneNode(scene.RootNode),
                Meshes = scene.Meshes.Select(CloneMesh).ToList(),
                Materials = scene.Materials.Select(CloneMaterial).ToList()
            };

            // Apply the optimizations according to settings
            if (settings.OptimizeInstances)
            {
                optimizedScene = await OptimizeInstancesAsync(optimizedScene, settings.InstanceSimilarityThreshold);
            }

            if (settings.FlattenHierarchy)
            {
                optimizedScene = await FlattenHierarchyAsync(optimizedScene, settings.MaxFlattenDepth);
            }

            if (settings.OptimizeTransforms)
            {
                optimizedScene = await OptimizeTransformsAsync(optimizedScene);
            }

            // Update statistics
            optimizedScene.Statistics = CalculateSceneStatistics(optimizedScene);

            _logger?.LogInfo($"Scene optimization completed: {optimizedScene.Statistics.TotalNodes} nodes, " +
                            $"{optimizedScene.Meshes.Count} meshes, {optimizedScene.Materials.Count} materials");

            return optimizedScene;
        }

        /// <summary>
        /// Optimizes instances in the scene by merging similar instances
        /// </summary>
        /// <param name="scene">The scene to optimize</param>
        /// <param name="similarityThreshold">Threshold for instance similarity (0-1)</param>
        /// <returns>Optimized scene</returns>
        public async Task<Scene> OptimizeInstancesAsync(Scene scene, float similarityThreshold)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            if (similarityThreshold < 0 || similarityThreshold > 1)
            {
                throw new ArgumentException("Similarity threshold must be between 0 and 1", nameof(similarityThreshold));
            }

            _logger?.LogInfo($"Optimizing instances with similarity threshold: {similarityThreshold}");

            // Create a copy of the scene
            Scene result = new Scene
            {
                Name = scene.Name,
                RootNode = CloneNode(scene.RootNode),
                Meshes = scene.Meshes.ToList(),
                Materials = scene.Materials.ToList()
            };

            // Find similar meshes and merge them
            Dictionary<string, List<Node>> meshGroups = new Dictionary<string, List<Node>>();

            // Group nodes by mesh
            CollectNodesByMesh(result.RootNode, meshGroups);

            int instancesMerged = 0;

            // Merge similar mesh instances
            foreach (var group in meshGroups.Values)
            {
                if (group.Count > 1)
                {
                    // For simplicity in this sample, we'll assume nodes with the same mesh are similar
                    // In a real implementation, this would use more sophisticated similarity metrics
                    
                    // Mark all but the first node as instances of the first one
                    Node primaryNode = group[0];
                    
                    for (int i = 1; i < group.Count; i++)
                    {
                        group[i].IsInstance = true;
                        group[i].InstanceSource = primaryNode.Name;
                        instancesMerged++;
                    }
                }
            }

            _logger?.LogInfo($"Instances optimized: {instancesMerged} instances merged");

            return await Task.FromResult(result);
        }

        private void CollectNodesByMesh(Node node, Dictionary<string, List<Node>> meshGroups)
        {
            if (node.Mesh != null)
            {
                string meshId = node.Mesh.Name;
                
                if (!meshGroups.ContainsKey(meshId))
                {
                    meshGroups[meshId] = new List<Node>();
                }
                
                meshGroups[meshId].Add(node);
            }
            
            foreach (var child in node.Children)
            {
                CollectNodesByMesh(child, meshGroups);
            }
        }

        /// <summary>
        /// Flattens the scene hierarchy by combining transforms
        /// </summary>
        /// <param name="scene">The scene to optimize</param>
        /// <param name="maxDepth">Maximum depth for flattening</param>
        /// <returns>Optimized scene</returns>
        public async Task<Scene> FlattenHierarchyAsync(Scene scene, int maxDepth)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            if (maxDepth < 0)
            {
                throw new ArgumentException("Maximum depth must be non-negative", nameof(maxDepth));
            }

            _logger?.LogInfo($"Flattening hierarchy with max depth: {maxDepth}");

            // Create a copy of the scene
            Scene result = new Scene
            {
                Name = scene.Name,
                RootNode = CloneNode(scene.RootNode),
                Meshes = scene.Meshes.ToList(),
                Materials = scene.Materials.ToList()
            };

            // Flatten the hierarchy
            int nodesFlattened = FlattenNodeHierarchy(result.RootNode, 0, maxDepth);

            _logger?.LogInfo($"Hierarchy flattened: {nodesFlattened} nodes flattened");

            return await Task.FromResult(result);
        }

        private int FlattenNodeHierarchy(Node node, int currentDepth, int maxDepth)
        {
            int nodesFlattened = 0;
            
            // Process children only if we're within max depth
            if (currentDepth <= maxDepth)
            {
                // Make a copy since we'll be modifying the collection
                List<Node> children = new List<Node>(node.Children);
                
                foreach (var child in children)
                {
                    // Recursively process child's hierarchy
                    nodesFlattened += FlattenNodeHierarchy(child, currentDepth + 1, maxDepth);
                }
                
                // If we're at maxDepth, flatten all descendants to this level
                if (currentDepth == maxDepth)
                {
                    List<Node> descendants = new List<Node>();
                    CollectDescendants(node, descendants);
                    
                    // Add all descendants with geometries to the current node and remove intermediate nodes
                    foreach (var descendant in descendants)
                    {
                        if (descendant.Mesh != null)
                        {
                            // Create a copy with combined transform
                            Node flattenedNode = new Node
                            {
                                Name = descendant.Name,
                                Mesh = descendant.Mesh,
                                Material = descendant.Material,
                                Transform = descendant.Transform, // In a real implementation, this would combine transforms
                                IsInstance = descendant.IsInstance,
                                InstanceSource = descendant.InstanceSource
                            };
                            
                            node.Children.Add(flattenedNode);
                            nodesFlattened++;
                        }
                    }
                    
                    // Remove all existing children as they've been flattened
                    node.Children.Clear();
                    node.Children.AddRange(children.Where(c => c.Mesh != null)); // Keep direct children with meshes
                }
            }
            
            return nodesFlattened;
        }

        private void CollectDescendants(Node node, List<Node> descendants)
        {
            foreach (var child in node.Children)
            {
                descendants.Add(child);
                CollectDescendants(child, descendants);
            }
        }

        /// <summary>
        /// Optimizes transforms in the scene by combining and simplifying them
        /// </summary>
        /// <param name="scene">The scene to optimize</param>
        /// <returns>Optimized scene</returns>
        public async Task<Scene> OptimizeTransformsAsync(Scene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            _logger?.LogInfo("Optimizing transforms");

            // Create a copy of the scene
            Scene result = new Scene
            {
                Name = scene.Name,
                RootNode = CloneNode(scene.RootNode),
                Meshes = scene.Meshes.ToList(),
                Materials = scene.Materials.ToList()
            };

            // Optimize the transforms
            int transformsOptimized = OptimizeNodeTransforms(result.RootNode);

            _logger?.LogInfo($"Transforms optimized: {transformsOptimized} transforms simplified");

            return await Task.FromResult(result);
        }

        private int OptimizeNodeTransforms(Node node)
        {
            int transformsOptimized = 0;
            
            // Optimize this node's transform
            if (OptimizeTransform(node))
            {
                transformsOptimized++;
            }
            
            // Recursively optimize child transforms
            foreach (var child in node.Children)
            {
                transformsOptimized += OptimizeNodeTransforms(child);
            }
            
            return transformsOptimized;
        }

        private bool OptimizeTransform(Node node)
        {
            // In a real implementation, this would simplify rotation matrices, reduce precision of values, etc.
            // For this sample, we'll just check if the transform is nearly identity
            Matrix4x4 identity = Matrix4x4.identity;
            
            // Check if the transform is close to identity
            bool isNearlyIdentity = true;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    float expected = i == j ? 1.0f : 0.0f;
                    if (Math.Abs(node.Transform[i, j] - expected) > 0.0001f)
                    {
                        isNearlyIdentity = false;
                        break;
                    }
                }
                if (!isNearlyIdentity) break;
            }
            
            if (isNearlyIdentity)
            {
                // If it's nearly identity, set it to exact identity
                node.Transform = identity;
                return true;
            }
            
            return false;
        }

        private Node CloneNode(Node original)
        {
            if (original == null) return null;
            
            Node clone = new Node
            {
                Name = original.Name,
                Mesh = original.Mesh, // Just reference the same mesh
                Material = original.Material, // Just reference the same material
                Transform = original.Transform,
                IsInstance = original.IsInstance,
                InstanceSource = original.InstanceSource,
                Children = new List<Node>()
            };
            
            foreach (var child in original.Children)
            {
                clone.Children.Add(CloneNode(child));
            }
            
            return clone;
        }

        private Mesh CloneMesh(Mesh original)
        {
            return new Mesh
            {
                Name = original.Name,
                Vertices = new List<Vector3>(original.Vertices),
                UVs = new List<Vector2>(original.UVs),
                PolygonIndices = new List<int>(original.PolygonIndices),
                BoundingBox = original.BoundingBox
            };
        }

        private Material CloneMaterial(Material original)
        {
            return new Material
            {
                Name = original.Name,
                ShaderName = original.ShaderName,
                Properties = new Dictionary<string, object>(original.Properties),
                Textures = new Dictionary<string, Texture>(original.Textures)
            };
        }

        private SceneStatistics CalculateSceneStatistics(Scene scene)
        {
            int totalNodes = CountNodes(scene.RootNode);
            int totalInstances = CountInstances(scene.RootNode);
            int totalPolygons = scene.Meshes.Sum(m => m.PolygonIndices.Count / 3);
            int totalVertices = scene.Meshes.Sum(m => m.Vertices.Count);
            int totalMaterials = scene.Materials.Count;
            
            return new SceneStatistics
            {
                TotalNodes = totalNodes,
                TotalInstances = totalInstances,
                TotalPolygons = totalPolygons,
                TotalVertices = totalVertices,
                TotalMaterials = totalMaterials,
                EstimatedMemoryUsageMB = EstimateMemoryUsage(scene),
                EstimatedFileSizeMB = EstimateFileSize(scene)
            };
        }

        private int CountNodes(Node node)
        {
            int count = 1; // Count this node
            
            foreach (var child in node.Children)
            {
                count += CountNodes(child);
            }
            
            return count;
        }

        private int CountInstances(Node node)
        {
            int count = node.IsInstance ? 1 : 0;
            
            foreach (var child in node.Children)
            {
                count += CountInstances(child);
            }
            
            return count;
        }

        private float EstimateMemoryUsage(Scene scene)
        {
            // Simple estimation in MB
            float vertexSize = 0.000012f; // ~12 bytes per vertex
            float polygonSize = 0.000004f; // ~4 bytes per index
            float textureSize = 0.5f; // Average 0.5MB per texture
            
            float vertexMemory = scene.Meshes.Sum(m => m.Vertices.Count) * vertexSize;
            float polygonMemory = scene.Meshes.Sum(m => m.PolygonIndices.Count) * polygonSize;
            float textureMemory = scene.Materials.Sum(m => m.Textures.Count) * textureSize;
            
            return vertexMemory + polygonMemory + textureMemory;
        }

        private float EstimateFileSize(Scene scene)
        {
            // Simple estimation in MB
            float baseSize = 0.1f;
            float perNodeSize = 0.0002f;
            float perVertexSize = 0.00001f;
            float perPolygonSize = 0.000004f;
            float perMaterialSize = 0.005f;
            float perTextureSize = 0.2f;
            
            int totalNodes = CountNodes(scene.RootNode);
            int totalVertices = scene.Meshes.Sum(m => m.Vertices.Count);
            int totalIndices = scene.Meshes.Sum(m => m.PolygonIndices.Count);
            int totalMaterials = scene.Materials.Count;
            int totalTextures = scene.Materials.Sum(m => m.Textures.Count);
            
            return baseSize + 
                   (totalNodes * perNodeSize) + 
                   (totalVertices * perVertexSize) + 
                   (totalIndices * perPolygonSize) + 
                   (totalMaterials * perMaterialSize) + 
                   (totalTextures * perTextureSize);
        }
    }
} 