using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization.Interfaces;

namespace USDOptimizer.Core.Optimization.Implementations
{
    /// <summary>
    /// Implementation of ISceneOptimizer for scene optimization operations
    /// </summary>
    public class SceneOptimizer : ISceneOptimizer
    {
        private readonly IMeshOptimizer _meshOptimizer;
        private readonly IMaterialOptimizer _materialOptimizer;

        public SceneOptimizer(IMeshOptimizer meshOptimizer, IMaterialOptimizer materialOptimizer)
        {
            _meshOptimizer = meshOptimizer ?? throw new ArgumentNullException(nameof(meshOptimizer));
            _materialOptimizer = materialOptimizer ?? throw new ArgumentNullException(nameof(materialOptimizer));
        }

        public async Task<Scene> OptimizeInstancesAsync(Scene scene, float similarityThreshold)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (similarityThreshold < 0 || similarityThreshold > 1)
                throw new ArgumentException("Similarity threshold must be between 0 and 1");

            var optimizedScene = new Scene
            {
                Name = $"{scene.Name}_Optimized",
                FilePath = scene.FilePath,
                Meshes = new List<Mesh>(scene.Meshes),
                Materials = new List<Material>(scene.Materials),
                Textures = new List<Texture>(scene.Textures),
                RootNode = new Node(scene.RootNode)
            };

            await Task.Run(() =>
            {
                var processedNodes = new HashSet<Node>();
                var instanceGroups = new List<List<Node>>();

                // Find similar instances
                foreach (var node in GetAllNodes(optimizedScene.RootNode))
                {
                    if (processedNodes.Contains(node) || node.Mesh == null)
                        continue;

                    var similarGroup = FindSimilarInstances(node, GetAllNodes(optimizedScene.RootNode), similarityThreshold);
                    if (similarGroup.Count > 1)
                    {
                        instanceGroups.Add(similarGroup);
                        foreach (var n in similarGroup)
                        {
                            processedNodes.Add(n);
                        }
                    }
                    else
                    {
                        processedNodes.Add(node);
                    }
                }

                // Merge similar instances
                foreach (var group in instanceGroups)
                {
                    MergeInstanceGroup(group, optimizedScene);
                }
            });

            return optimizedScene;
        }

        public async Task<Scene> FlattenHierarchyAsync(Scene scene, int maxDepth)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (maxDepth < 0)
                throw new ArgumentException("Max depth must be non-negative");

            var optimizedScene = new Scene
            {
                Name = $"{scene.Name}_Flattened",
                FilePath = scene.FilePath,
                Meshes = new List<Mesh>(scene.Meshes),
                Materials = new List<Material>(scene.Materials),
                Textures = new List<Texture>(scene.Textures),
                RootNode = new Node(scene.RootNode)
            };

            await Task.Run(() =>
            {
                FlattenNodeHierarchy(optimizedScene.RootNode, maxDepth, 0);
            });

            return optimizedScene;
        }

        public async Task<Scene> OptimizeTransformsAsync(Scene scene)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            var optimizedScene = new Scene
            {
                Name = $"{scene.Name}_TransformOptimized",
                FilePath = scene.FilePath,
                Meshes = new List<Mesh>(scene.Meshes),
                Materials = new List<Material>(scene.Materials),
                Textures = new List<Texture>(scene.Textures),
                RootNode = new Node(scene.RootNode)
            };

            await Task.Run(() =>
            {
                OptimizeNodeTransforms(optimizedScene.RootNode);
            });

            return optimizedScene;
        }

        public async Task<Scene> OptimizeSceneAsync(Scene scene, SceneOptimizationSettings settings)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var optimizedScene = settings.PreserveOriginal ? new Scene(scene) : scene;

            // Apply optimizations based on settings
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

            if (settings.OptimizeMeshes)
            {
                // TODO: Implement mesh optimization
                // This would involve using the IMeshOptimizer to optimize each mesh
            }

            if (settings.OptimizeMaterials)
            {
                // TODO: Implement material optimization
                // This would involve using the IMaterialOptimizer to optimize each material
            }

            if (settings.OptimizeTextures)
            {
                // TODO: Implement texture optimization
                // This would involve using the IMaterialOptimizer to compress textures
            }

            return optimizedScene;
        }

        private List<Node> GetAllNodes(Node root)
        {
            var nodes = new List<Node> { root };
            foreach (var child in root.Children)
            {
                nodes.AddRange(GetAllNodes(child));
            }
            return nodes;
        }

        private List<Node> FindSimilarInstances(Node referenceNode, List<Node> allNodes, float similarityThreshold)
        {
            var similarNodes = new List<Node> { referenceNode };

            foreach (var node in allNodes)
            {
                if (node == referenceNode || node.Mesh == null || referenceNode.Mesh == null)
                    continue;

                if (CalculateInstanceSimilarity(referenceNode, node) >= similarityThreshold)
                {
                    similarNodes.Add(node);
                }
            }

            return similarNodes;
        }

        private float CalculateInstanceSimilarity(Node node1, Node node2)
        {
            if (node1.Mesh != node2.Mesh)
                return 0;

            if (node1.Material != node2.Material)
                return 0;

            // Compare transforms
            var transformSimilarity = CalculateTransformSimilarity(node1.Transform, node2.Transform);
            return transformSimilarity;
        }

        private float CalculateTransformSimilarity(Matrix4x4 t1, Matrix4x4 t2)
        {
            // Compare scale
            var scale1 = new Vector3(t1.M11, t1.M22, t1.M33);
            var scale2 = new Vector3(t2.M11, t2.M22, t2.M33);
            var scaleDiff = (scale1 - scale2).magnitude;
            if (scaleDiff > 0.1f)
                return 0;

            // Compare rotation (simplified)
            var rotation1 = new Vector3(t1.M12, t1.M13, t1.M21);
            var rotation2 = new Vector3(t2.M12, t2.M13, t2.M21);
            var rotationDiff = (rotation1 - rotation2).magnitude;
            if (rotationDiff > 0.1f)
                return 0;

            // Compare position
            var position1 = new Vector3(t1.M14, t1.M24, t1.M34);
            var position2 = new Vector3(t2.M14, t2.M24, t2.M34);
            var positionDiff = (position1 - position2).magnitude;
            if (positionDiff > 0.1f)
                return 0;

            return 1;
        }

        private void MergeInstanceGroup(List<Node> group, Scene scene)
        {
            if (group.Count <= 1)
                return;

            var referenceNode = group[0];
            var mergedNode = new Node
            {
                Name = $"Merged_{referenceNode.Name}",
                Mesh = referenceNode.Mesh,
                Material = referenceNode.Material,
                Transform = Matrix4x4.identity
            };

            // Calculate combined bounds
            var bounds = referenceNode.Mesh.BoundingBox;
            foreach (var node in group.Skip(1))
            {
                var transformedBounds = TransformBounds(node.Mesh.BoundingBox, node.Transform);
                bounds.Encapsulate(transformedBounds);
            }

            // Add merged node to scene
            scene.RootNode.Children.Add(mergedNode);

            // Remove original nodes
            foreach (var node in group)
            {
                RemoveNodeFromScene(node, scene);
            }
        }

        private void FlattenNodeHierarchy(Node node, int maxDepth, int currentDepth)
        {
            if (currentDepth >= maxDepth)
                return;

            var children = new List<Node>(node.Children);
            foreach (var child in children)
            {
                // Combine transforms
                child.Transform = node.Transform * child.Transform;

                // Move children to parent's level if they have no children
                if (!child.Children.Any())
                {
                    node.Children.Remove(child);
                    node.Parent?.Children.Add(child);
                    child.Parent = node.Parent;
                }
                else
                {
                    FlattenNodeHierarchy(child, maxDepth, currentDepth + 1);
                }
            }
        }

        private void OptimizeNodeTransforms(Node node)
        {
            // Combine parent and child transforms if possible
            foreach (var child in node.Children)
            {
                if (CanCombineTransforms(node.Transform, child.Transform))
                {
                    child.Transform = node.Transform * child.Transform;
                    node.Transform = Matrix4x4.identity;
                }
                OptimizeNodeTransforms(child);
            }
        }

        private bool CanCombineTransforms(Matrix4x4 parent, Matrix4x4 child)
        {
            // Check if transforms can be combined without significant loss of precision
            var combined = parent * child;
            var decomposed = DecomposeMatrix(combined);
            var parentDecomposed = DecomposeMatrix(parent);
            var childDecomposed = DecomposeMatrix(child);

            // Compare decomposed values
            return Math.Abs(decomposed.scale.x - parentDecomposed.scale.x * childDecomposed.scale.x) < 0.001f &&
                   Math.Abs(decomposed.scale.y - parentDecomposed.scale.y * childDecomposed.scale.y) < 0.001f &&
                   Math.Abs(decomposed.scale.z - parentDecomposed.scale.z * childDecomposed.scale.z) < 0.001f;
        }

        private (Vector3 position, Vector3 scale, Vector3 rotation) DecomposeMatrix(Matrix4x4 matrix)
        {
            // Extract position
            var position = new Vector3(matrix.M14, matrix.M24, matrix.M34);

            // Extract scale
            var scale = new Vector3(
                new Vector3(matrix.M11, matrix.M12, matrix.M13).magnitude,
                new Vector3(matrix.M21, matrix.M22, matrix.M23).magnitude,
                new Vector3(matrix.M31, matrix.M32, matrix.M33).magnitude
            );

            // Extract rotation (simplified)
            var rotation = new Vector3(
                Math.Atan2(matrix.M32, matrix.M33),
                Math.Atan2(-matrix.M31, Math.Sqrt(matrix.M32 * matrix.M32 + matrix.M33 * matrix.M33)),
                Math.Atan2(matrix.M21, matrix.M11)
            );

            return (position, scale, rotation);
        }

        private void RemoveNodeFromScene(Node node, Scene scene)
        {
            if (node.Parent != null)
            {
                node.Parent.Children.Remove(node);
            }
            else
            {
                scene.RootNode.Children.Remove(node);
            }
        }

        private Bounds TransformBounds(Bounds bounds, Matrix4x4 transform)
        {
            var center = transform.MultiplyPoint(bounds.Center);
            var size = transform.MultiplyVector(bounds.Size);
            return new Bounds { Center = center, Size = size };
        }
    }
} 