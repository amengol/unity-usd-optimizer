using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization.Implementations;
using USDOptimizer.Core.Optimization.Interfaces;

namespace USDOptimizer.Tests.Core.Optimization
{
    [TestFixture]
    public class SceneOptimizerTests
    {
        private SceneOptimizer _sceneOptimizer;
        private IMeshOptimizer _mockMeshOptimizer;
        private IMaterialOptimizer _mockMaterialOptimizer;
        private Scene _testScene;

        [SetUp]
        public void Setup()
        {
            _mockMeshOptimizer = NSubstitute.Substitute.For<IMeshOptimizer>();
            _mockMaterialOptimizer = NSubstitute.Substitute.For<IMaterialOptimizer>();
            _sceneOptimizer = new SceneOptimizer(_mockMeshOptimizer, _mockMaterialOptimizer);
            _testScene = CreateTestScene();
        }

        [Test]
        public async Task OptimizeInstancesAsync_WithSimilarInstances_MergesInstances()
        {
            // Arrange
            var scene = CreateSceneWithSimilarInstances();
            var similarityThreshold = 0.9f;

            // Act
            var result = await _sceneOptimizer.OptimizeInstancesAsync(scene, similarityThreshold);

            // Assert
            Assert.That(result.RootNode.Children.Count, Is.EqualTo(2)); // One merged group and one unique instance
            Assert.That(result.RootNode.Children[0].Name, Does.StartWith("Merged_"));
        }

        [Test]
        public async Task FlattenHierarchyAsync_WithNestedNodes_FlattensToSpecifiedDepth()
        {
            // Arrange
            var scene = CreateSceneWithNestedHierarchy();
            var maxDepth = 1;

            // Act
            var result = await _sceneOptimizer.FlattenHierarchyAsync(scene, maxDepth);

            // Assert
            Assert.That(result.RootNode.Children.Count, Is.EqualTo(3)); // All nodes at depth 1 should be children of root
        }

        [Test]
        public async Task OptimizeTransformsAsync_WithNestedTransforms_CombinesTransforms()
        {
            // Arrange
            var scene = CreateSceneWithNestedTransforms();

            // Act
            var result = await _sceneOptimizer.OptimizeTransformsAsync(scene);

            // Assert
            var childNode = result.RootNode.Children[0];
            Assert.That(childNode.Transform, Is.EqualTo(Matrix4x4.identity));
        }

        [Test]
        public async Task OptimizeSceneAsync_WithAllSettings_AppliesAllOptimizations()
        {
            // Arrange
            var settings = new SceneOptimizationSettings
            {
                OptimizeInstances = true,
                FlattenHierarchy = true,
                OptimizeTransforms = true,
                OptimizeMeshes = true,
                OptimizeMaterials = true,
                OptimizeTextures = true,
                InstanceSimilarityThreshold = 0.9f,
                MaxFlattenDepth = 1,
                PreserveOriginal = true
            };

            // Act
            var result = await _sceneOptimizer.OptimizeSceneAsync(_testScene, settings);

            // Assert
            Assert.That(result.Name, Does.EndWith("_Optimized"));
            Assert.That(result.RootNode.Children.Count, Is.LessThan(_testScene.RootNode.Children.Count));
        }

        [Test]
        public void Constructor_NullMeshOptimizer_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SceneOptimizer(null, _mockMaterialOptimizer));
        }

        [Test]
        public void Constructor_NullMaterialOptimizer_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SceneOptimizer(_mockMeshOptimizer, null));
        }

        [Test]
        public async Task OptimizeInstancesAsync_NullScene_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _sceneOptimizer.OptimizeInstancesAsync(null, 0.9f));
            Assert.That(ex.ParamName, Is.EqualTo("scene"));
        }

        [Test]
        public async Task OptimizeInstancesAsync_InvalidSimilarityThreshold_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
                await _sceneOptimizer.OptimizeInstancesAsync(_testScene, 1.1f));
            Assert.That(ex.Message, Does.Contain("Similarity threshold must be between 0 and 1"));
        }

        private Scene CreateTestScene()
        {
            var scene = new Scene
            {
                Name = "TestScene",
                RootNode = new Node { Name = "Root" }
            };

            var mesh = new Mesh
            {
                Name = "TestMesh",
                Vertices = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0) },
                UVs = new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1) },
                PolygonIndices = new List<int> { 0, 1, 2 },
                BoundingBox = new Bounds { Center = new Vector3(0.5f, 0.5f, 0), Size = new Vector3(1, 1, 0) }
            };

            var material = new Material { Name = "TestMaterial" };

            scene.Meshes.Add(mesh);
            scene.Materials.Add(material);

            var node1 = new Node
            {
                Name = "Node1",
                Mesh = mesh,
                Material = material,
                Transform = Matrix4x4.identity
            };

            var node2 = new Node
            {
                Name = "Node2",
                Mesh = mesh,
                Material = material,
                Transform = Matrix4x4.identity
            };

            scene.RootNode.Children.Add(node1);
            scene.RootNode.Children.Add(node2);

            return scene;
        }

        private Scene CreateSceneWithSimilarInstances()
        {
            var scene = new Scene
            {
                Name = "SimilarInstancesScene",
                RootNode = new Node { Name = "Root" }
            };

            var mesh = new Mesh
            {
                Name = "TestMesh",
                Vertices = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0) },
                UVs = new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1) },
                PolygonIndices = new List<int> { 0, 1, 2 },
                BoundingBox = new Bounds { Center = new Vector3(0.5f, 0.5f, 0), Size = new Vector3(1, 1, 0) }
            };

            var material = new Material { Name = "TestMaterial" };

            scene.Meshes.Add(mesh);
            scene.Materials.Add(material);

            // Create three similar instances
            for (int i = 0; i < 3; i++)
            {
                var node = new Node
                {
                    Name = $"Instance{i}",
                    Mesh = mesh,
                    Material = material,
                    Transform = Matrix4x4.identity
                };
                scene.RootNode.Children.Add(node);
            }

            // Create one different instance
            var differentNode = new Node
            {
                Name = "DifferentInstance",
                Mesh = mesh,
                Material = material,
                Transform = Matrix4x4.Scale(new Vector3(2, 2, 2))
            };
            scene.RootNode.Children.Add(differentNode);

            return scene;
        }

        private Scene CreateSceneWithNestedHierarchy()
        {
            var scene = new Scene
            {
                Name = "NestedHierarchyScene",
                RootNode = new Node { Name = "Root" }
            };

            var mesh = new Mesh
            {
                Name = "TestMesh",
                Vertices = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0) },
                UVs = new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1) },
                PolygonIndices = new List<int> { 0, 1, 2 },
                BoundingBox = new Bounds { Center = new Vector3(0.5f, 0.5f, 0), Size = new Vector3(1, 1, 0) }
            };

            var material = new Material { Name = "TestMaterial" };

            scene.Meshes.Add(mesh);
            scene.Materials.Add(material);

            // Create a nested hierarchy
            var level1Node = new Node
            {
                Name = "Level1",
                Mesh = mesh,
                Material = material,
                Transform = Matrix4x4.identity
            };

            var level2Node = new Node
            {
                Name = "Level2",
                Mesh = mesh,
                Material = material,
                Transform = Matrix4x4.identity
            };

            var level3Node = new Node
            {
                Name = "Level3",
                Mesh = mesh,
                Material = material,
                Transform = Matrix4x4.identity
            };

            level2Node.Children.Add(level3Node);
            level1Node.Children.Add(level2Node);
            scene.RootNode.Children.Add(level1Node);

            return scene;
        }

        private Scene CreateSceneWithNestedTransforms()
        {
            var scene = new Scene
            {
                Name = "NestedTransformsScene",
                RootNode = new Node { Name = "Root" }
            };

            var mesh = new Mesh
            {
                Name = "TestMesh",
                Vertices = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0) },
                UVs = new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1) },
                PolygonIndices = new List<int> { 0, 1, 2 },
                BoundingBox = new Bounds { Center = new Vector3(0.5f, 0.5f, 0), Size = new Vector3(1, 1, 0) }
            };

            var material = new Material { Name = "TestMaterial" };

            scene.Meshes.Add(mesh);
            scene.Materials.Add(material);

            // Create nodes with nested transforms that can be combined
            var parentNode = new Node
            {
                Name = "Parent",
                Transform = Matrix4x4.Scale(new Vector3(2, 2, 2))
            };

            var childNode = new Node
            {
                Name = "Child",
                Mesh = mesh,
                Material = material,
                Transform = Matrix4x4.Scale(new Vector3(3, 3, 3))
            };

            parentNode.Children.Add(childNode);
            scene.RootNode.Children.Add(parentNode);

            return scene;
        }
    }
} 