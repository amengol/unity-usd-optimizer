using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Logging;
using UnityEngine;

namespace USDOptimizer.Core.Optimization
{
    public class SceneOptimizer
    {
        private readonly USDOptimizer.Core.Logging.ILogger _logger;
        private readonly MeshOptimizer _meshOptimizer;

        public SceneOptimizer(USDOptimizer.Core.Logging.ILogger logger = null)
        {
            _logger = logger ?? new UnityLogger();
            _meshOptimizer = new MeshOptimizer(_logger);
        }

        public async Task<USDScene> OptimizeSceneAsync(USDScene scene, SceneOptimizationSettings settings)
        {
            try
            {
                _logger.LogInfo($"Starting scene optimization for: {scene.Name}");

                // Validate scene
                if (scene == null)
                {
                    throw new ArgumentNullException(nameof(scene), "Scene cannot be null");
                }

                // Validate settings
                if (settings == null)
                {
                    throw new ArgumentNullException(nameof(settings), "Settings cannot be null");
                }

                // Create a copy of the scene for optimization
                var optimizedScene = new USDScene
                {
                    FilePath = scene.FilePath,
                    Name = $"{scene.Name}_Optimized",
                    ImportDate = DateTime.Now,
                    Nodes = scene.Nodes.Select(n => CloneNode(n)).ToList(),
                    Meshes = scene.Meshes.Select(m => CloneMesh(m)).ToList(),
                    Materials = scene.Materials.Select(m => CloneMaterial(m)).ToList(),
                    Textures = scene.Textures.Select(t => CloneTexture(t)).ToList()
                };

                // Apply optimization settings
                await ApplyOptimizationSettingsAsync(optimizedScene, settings);

                _logger.LogInfo($"Scene optimization completed for: {scene.Name}");
                
                // Update and return the optimized scene
                UpdateSceneStatistics(optimizedScene);
                return optimizedScene;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error optimizing scene: {ex.Message}");
                throw;
            }
        }

        private USDNode CloneNode(USDNode original)
        {
            if (original == null) return null;
            
            var clone = new USDNode
            {
                Name = original.Name,
                Type = original.Type,
                Properties = new Dictionary<string, object>(original.Properties),
                Children = new List<USDNode>()
            };
            
            if (original.Children != null)
            {
                foreach (var child in original.Children)
                {
                    clone.Children.Add(CloneNode(child));
                }
            }
            
            return clone;
        }
        
        private USDOptimizer.Core.Models.Mesh CloneMesh(USDOptimizer.Core.Models.Mesh original)
        {
            if (original == null) return null;
            
            return new USDOptimizer.Core.Models.Mesh
            {
                Name = original.Name,
                PolygonCount = original.PolygonCount,
                VertexCount = original.VertexCount,
                Material = original.Material != null ? CloneMaterial(original.Material) : null
            };
        }
        
        private USDOptimizer.Core.Models.Material CloneMaterial(USDOptimizer.Core.Models.Material original)
        {
            if (original == null) return null;
            
            return new USDOptimizer.Core.Models.Material
            {
                Name = original.Name
            };
        }
        
        private USDOptimizer.Core.Models.Texture CloneTexture(USDOptimizer.Core.Models.Texture original)
        {
            if (original == null) return null;
            
            return new USDOptimizer.Core.Models.Texture
            {
                Name = original.Name,
                Width = original.Width,
                Height = original.Height,
                Size = original.Size
            };
        }

        private async Task ApplyOptimizationSettingsAsync(USDScene scene, SceneOptimizationSettings settings)
        {
            // List to track optimization results
            var optimizationResults = new List<USDOptimizer.Core.Models.OptimizationResult>();
            
            // Apply mesh optimization (using MeshOptimizer)
            if (settings.EnableMeshSimplification || settings.EnableLODGeneration)
            {
                int optimizedMeshCount = await _meshOptimizer.OptimizeMeshesAsync(scene, settings);
                optimizationResults.Add(new USDOptimizer.Core.Models.OptimizationResult
                {
                    Type = "Mesh Optimization",
                    ItemsOptimized = optimizedMeshCount,
                    Notes = $"Applied {(settings.EnableMeshSimplification ? "mesh simplification" : "")} " +
                            $"{(settings.EnableMeshSimplification && settings.EnableLODGeneration ? "and" : "")} " +
                            $"{(settings.EnableLODGeneration ? "LOD generation" : "")}"
                });
            }

            // Apply material optimization
            if (settings.EnableMaterialBatching)
            {
                int batchedMaterials = await OptimizeMaterialsAsync(scene);
                optimizationResults.Add(new USDOptimizer.Core.Models.OptimizationResult
                {
                    Type = "Material Optimization",
                    ItemsOptimized = batchedMaterials,
                    Notes = "Batched similar materials"
                });
            }

            // Apply texture optimization
            if (settings.EnableTextureCompression)
            {
                int compressedTextures = await OptimizeTexturesAsync(scene);
                optimizationResults.Add(new USDOptimizer.Core.Models.OptimizationResult
                {
                    Type = "Texture Optimization",
                    ItemsOptimized = compressedTextures,
                    Notes = "Applied texture compression"
                });
            }

            // Apply hierarchy optimization
            if (settings.EnableHierarchyFlattening)
            {
                int flattenedNodes = await OptimizeHierarchyAsync(scene, settings.MaxHierarchyDepth);
                optimizationResults.Add(new USDOptimizer.Core.Models.OptimizationResult
                {
                    Type = "Hierarchy Optimization",
                    ItemsOptimized = flattenedNodes,
                    Notes = $"Flattened hierarchy to maximum depth of {settings.MaxHierarchyDepth}"
                });
            }

            // Apply transform optimization
            if (settings.EnableTransformOptimization)
            {
                int optimizedTransforms = await OptimizeTransformsAsync(scene);
                optimizationResults.Add(new USDOptimizer.Core.Models.OptimizationResult
                {
                    Type = "Transform Optimization",
                    ItemsOptimized = optimizedTransforms,
                    Notes = "Optimized transforms"
                });
            }
            
            // Store optimization results in the scene
            scene.OptimizationResults = optimizationResults;
        }

        private async Task<int> OptimizeMaterialsAsync(USDScene scene)
        {
            // This is a placeholder implementation
            _logger.LogInfo("Starting material optimization");
            int batchedCount = 0;
            
            await Task.Run(() => {
                // Simple material batching algorithm - combine materials with the same name
                var materialGroups = new Dictionary<string, List<USDOptimizer.Core.Models.Material>>();
                
                // Group materials by name
                foreach (var material in scene.Materials)
                {
                    string key = material.Name;
                    if (!materialGroups.ContainsKey(key))
                    {
                        materialGroups[key] = new List<USDOptimizer.Core.Models.Material>();
                    }
                    // Cast to ensure we're using the same type
                    materialGroups[key].Add((USDOptimizer.Core.Models.Material)material);
                }
                
                // Batch materials in each group
                foreach (var group in materialGroups)
                {
                    if (group.Value.Count > 1)
                    {
                        // Use the first material as the "master" material
                        var masterMaterial = group.Value[0];
                        
                        // Remove other materials in the group
                        for (int i = 1; i < group.Value.Count; i++)
                        {
                            scene.Materials.Remove(group.Value[i]);
                            batchedCount++;
                        }
                        
                        _logger.LogInfo($"Batched {group.Value.Count} instances of material '{group.Key}'");
                    }
                }
            });
            
            return batchedCount;
        }

        private async Task<int> OptimizeTexturesAsync(USDScene scene)
        {
            // This is a placeholder implementation
            _logger.LogInfo("Starting texture optimization");
            int compressedCount = 0;
            
            await Task.Run(() => {
                // Simple texture compression - reduce texture size
                foreach (var texture in scene.Textures)
                {
                    // Simulate compression by reducing size
                    if (texture.Width > 1024 || texture.Height > 1024)
                    {
                        float compressionFactor = 0.5f;
                        long originalSize = texture.Size;
                        
                        texture.Width = (int)(texture.Width * compressionFactor);
                        texture.Height = (int)(texture.Height * compressionFactor);
                        texture.Size = (long)(texture.Size * compressionFactor * compressionFactor);
                        
                        _logger.LogInfo($"Compressed texture '{texture.Name}' from {originalSize} to {texture.Size} bytes");
                        compressedCount++;
                    }
                }
            });
            
            return compressedCount;
        }

        private async Task<int> OptimizeHierarchyAsync(USDScene scene, int maxDepth)
        {
            // This is a placeholder implementation
            _logger.LogInfo($"Starting hierarchy optimization with max depth: {maxDepth}");
            int flattenedCount = 0;
            
            await Task.Run(() => {
                // Flatten hierarchy by limiting depth
                flattenedCount = FlattenNodeHierarchy(scene.RootNode, 0, maxDepth);
            });
            
            return flattenedCount;
        }

        private int FlattenNodeHierarchy(USDNode node, int currentDepth, int maxDepth)
        {
            int flattenedCount = 0;
            
            if (node == null || node.Children == null || node.Children.Count == 0)
            {
                return 0;
            }
            
            // If we've reached max depth, flatten all children
            if (currentDepth >= maxDepth)
            {
                flattenedCount += FlattenNode(node);
                return flattenedCount;
            }
            
            // Otherwise, recurse into children
            List<USDNode> childrenCopy = new List<USDNode>(node.Children);
            foreach (var child in childrenCopy)
            {
                flattenedCount += FlattenNodeHierarchy(child, currentDepth + 1, maxDepth);
            }
            
            return flattenedCount;
        }

        private int FlattenNode(USDNode node)
        {
            if (node == null || node.Children == null || node.Children.Count == 0)
            {
                return 0;
            }
            
            int flattenedCount = 0;
            List<USDNode> allDescendants = new List<USDNode>();
            
            // Collect all descendants
            CollectDescendants(node, allDescendants);
            
            // Clear children
            int childCount = node.Children.Count;
            node.Children.Clear();
            
            // Add only leaf nodes as direct children
            foreach (var descendant in allDescendants)
            {
                if (descendant.Children == null || descendant.Children.Count == 0)
                {
                    node.Children.Add(descendant);
                }
                else
                {
                    flattenedCount++;
                }
            }
            
            _logger.LogInfo($"Flattened node '{node.Name}' - removed {flattenedCount} intermediate nodes");
            return flattenedCount;
        }

        private void CollectDescendants(USDNode node, List<USDNode> descendants)
        {
            if (node == null || node.Children == null)
            {
                return;
            }
            
            foreach (var child in node.Children)
            {
                descendants.Add(child);
                CollectDescendants(child, descendants);
            }
        }

        private async Task<int> OptimizeTransformsAsync(USDScene scene)
        {
            // This is a placeholder implementation
            _logger.LogInfo("Starting transform optimization");
            int optimizedCount = 0;
            
            await Task.Run(() => {
                // Optimize transforms by simplifying rotation, scale, and translation values
                optimizedCount = OptimizeNodeTransforms(scene.RootNode);
            });
            
            return optimizedCount;
        }

        private int OptimizeNodeTransforms(USDNode node)
        {
            int optimizedCount = 0;
            
            // Simple transform optimization - round values to reduce precision
            // In a real implementation, this would use more sophisticated transform simplification
            
            // For each child node
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    // Optimize this node's transform
                    bool wasOptimized = OptimizeTransform(child);
                    if (wasOptimized)
                    {
                        optimizedCount++;
                    }
                    
                    // Recursively optimize children
                    optimizedCount += OptimizeNodeTransforms(child);
                }
            }
            
            return optimizedCount;
        }

        private bool OptimizeTransform(USDNode node)
        {
            // This is a placeholder implementation
            // In a real implementation, this would actually modify the transform matrix
            return true;
        }

        private void UpdateSceneStatistics(USDScene scene)
        {
            // Calculate actual statistics from the scene data
            int totalPolygons = 0;
            int totalVertices = 0;
            
            foreach (var mesh in scene.Meshes)
            {
                totalPolygons += mesh.PolygonCount;
                totalVertices += mesh.VertexCount;
            }
            
            // Count node types
            Dictionary<string, int> nodeTypeCounts = new Dictionary<string, int>();
            CountNodeTypes(scene.RootNode, nodeTypeCounts);
            
            // Create statistics object
            scene.Statistics = new SceneStatistics
            {
                TotalNodes = CountNodes(scene.RootNode),
                TotalPolygons = totalPolygons,
                TotalVertices = totalVertices,
                TotalMaterials = scene.Materials.Count,
                TotalTextures = scene.Textures.Count,
                TotalFileSize = EstimateFileSize(scene),
                NodeTypeCounts = nodeTypeCounts
            };
        }

        private int CountNodes(USDNode node)
        {
            if (node == null)
            {
                return 0;
            }
            
            int count = 1; // Count this node
            
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    count += CountNodes(child);
                }
            }
            
            return count;
        }

        private void CountNodeTypes(USDNode node, Dictionary<string, int> typeCounts)
        {
            if (node == null)
            {
                return;
            }
            
            // Determine node type
            string nodeType = "Transform"; // Default type
            
            if (node.Mesh != null)
            {
                nodeType = "Mesh";
            }
            else if (node.Material != null)
            {
                nodeType = "Material";
            }
            
            // Increment count for this type
            if (!typeCounts.ContainsKey(nodeType))
            {
                typeCounts[nodeType] = 0;
            }
            typeCounts[nodeType]++;
            
            // Recurse into children
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    CountNodeTypes(child, typeCounts);
                }
            }
        }

        private float EstimateFileSize(USDScene scene)
        {
            // Simple file size estimation
            float size = 0;
            
            // Add size for each mesh (rough estimate based on vertex and polygon count)
            foreach (var mesh in scene.Meshes)
            {
                size += mesh.VertexCount * 0.05f; // ~50 bytes per vertex
                size += mesh.PolygonCount * 0.012f; // ~12 bytes per polygon
            }
            
            // Add size for each material
            size += scene.Materials.Count * 0.5f; // ~0.5 KB per material
            
            // Add size for each texture
            foreach (var texture in scene.Textures)
            {
                size += texture.Size;
            }
            
            // Add overhead
            size *= 1.1f; // 10% overhead
            
            return size;
        }
    }
} 