using NUnit.Framework;
using UnityEngine;
using USDSceneOptimizer;

namespace USDSceneOptimizer.Tests
{
    public class SceneOptimizerTests
    {
        private GameObject testObject;
        private ISceneOptimizer sceneOptimizer;
        private GameObjectAnalysisResults testAnalysisResults;

        [SetUp]
        public void Setup()
        {
            // Create a test GameObject with a mesh
            testObject = new GameObject("TestObject");
            var meshFilter = testObject.AddComponent<MeshFilter>();
            var meshRenderer = testObject.AddComponent<MeshRenderer>();
            var material = new Material(Shader.Find("Standard"));
            meshRenderer.material = material;

            // Create a simple cube mesh for testing
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-1, -1, -1),
                new Vector3(1, -1, -1),
                new Vector3(1, 1, -1),
                new Vector3(-1, 1, -1),
                new Vector3(-1, -1, 1),
                new Vector3(1, -1, 1),
                new Vector3(1, 1, 1),
                new Vector3(-1, 1, 1)
            };
            mesh.triangles = new int[]
            {
                0, 1, 2, 0, 2, 3,
                1, 5, 6, 1, 6, 2,
                5, 4, 7, 5, 7, 6,
                4, 0, 3, 4, 3, 7,
                3, 2, 6, 3, 6, 7,
                4, 5, 1, 4, 1, 0
            };
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;

            // Create test analysis results
            testAnalysisResults = new GameObjectAnalysisResults
            {
                GameObjectName = "TestObject",
                PolygonCount = 12,
                VertexCount = 8,
                MaterialCount = 1,
                MemoryUsage = 1024,
                Recommendations = new System.Collections.Generic.List<OptimizationRecommendation>
                {
                    new OptimizationRecommendation
                    {
                        Title = "Test Recommendation",
                        Description = "Test Description",
                        Priority = OptimizationPriority.Medium,
                        EstimatedImprovement = 0.5f
                    }
                }
            };

            // TODO: Initialize sceneOptimizer with actual implementation
            // sceneOptimizer = new SceneOptimizer();
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        [Test]
        public void OptimizeGameObject_ReturnsValidResults()
        {
            // Arrange
            Assert.IsNotNull(sceneOptimizer, "SceneOptimizer should be initialized");

            // Act
            var results = sceneOptimizer.OptimizeGameObject(testObject, testAnalysisResults);

            // Assert
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Success);
            Assert.GreaterOrEqual(results.MemoryReduction, 0);
            Assert.GreaterOrEqual(results.PolygonReduction, 0);
            Assert.GreaterOrEqual(results.VertexReduction, 0);
            Assert.IsNotNull(results.AppliedActions);
        }

        [Test]
        public void OptimizeScene_ReturnsValidResults()
        {
            // Arrange
            Assert.IsNotNull(sceneOptimizer, "SceneOptimizer should be initialized");
            var analysisResults = new AnalysisResults
            {
                TotalPolygons = 100,
                TotalVertices = 50,
                TotalMaterials = 5,
                TotalTextures = 10,
                TotalMemoryUsage = 2048,
                Recommendations = new System.Collections.Generic.List<OptimizationRecommendation>
                {
                    new OptimizationRecommendation
                    {
                        Title = "Test Recommendation",
                        Description = "Test Description",
                        Priority = OptimizationPriority.Medium,
                        EstimatedImprovement = 0.5f
                    }
                }
            };

            // Act
            var results = sceneOptimizer.OptimizeScene(analysisResults);

            // Assert
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Success);
            Assert.GreaterOrEqual(results.MemoryReduction, 0);
            Assert.GreaterOrEqual(results.PolygonReduction, 0);
            Assert.GreaterOrEqual(results.VertexReduction, 0);
            Assert.IsNotNull(results.AppliedActions);
        }

        [Test]
        public void OptimizeGameObject_NullGameObject_ThrowsException()
        {
            // Arrange
            Assert.IsNotNull(sceneOptimizer, "SceneOptimizer should be initialized");

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => 
                sceneOptimizer.OptimizeGameObject(null, testAnalysisResults));
        }

        [Test]
        public void OptimizeGameObject_NullAnalysisResults_ThrowsException()
        {
            // Arrange
            Assert.IsNotNull(sceneOptimizer, "SceneOptimizer should be initialized");

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => 
                sceneOptimizer.OptimizeGameObject(testObject, null));
        }
    }
} 