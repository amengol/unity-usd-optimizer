using NUnit.Framework;
using UnityEngine;
using USDSceneOptimizer;

namespace USDSceneOptimizer.Tests
{
    public class SceneAnalyzerTests
    {
        private GameObject testObject;
        private ISceneAnalyzer sceneAnalyzer;

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

            // TODO: Initialize sceneAnalyzer with actual implementation
            // sceneAnalyzer = new SceneAnalyzer();
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
        public void AnalyzeGameObject_ReturnsValidResults()
        {
            // Arrange
            Assert.IsNotNull(sceneAnalyzer, "SceneAnalyzer should be initialized");

            // Act
            var results = sceneAnalyzer.AnalyzeGameObject(testObject);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual("TestObject", results.GameObjectName);
            Assert.Greater(results.PolygonCount, 0);
            Assert.Greater(results.VertexCount, 0);
            Assert.Greater(results.MaterialCount, 0);
            Assert.GreaterOrEqual(results.MemoryUsage, 0);
        }

        [Test]
        public void AnalyzeScene_ReturnsValidResults()
        {
            // Arrange
            Assert.IsNotNull(sceneAnalyzer, "SceneAnalyzer should be initialized");

            // Act
            var results = sceneAnalyzer.AnalyzeScene();

            // Assert
            Assert.IsNotNull(results);
            Assert.GreaterOrEqual(results.TotalPolygons, 0);
            Assert.GreaterOrEqual(results.TotalVertices, 0);
            Assert.GreaterOrEqual(results.TotalMaterials, 0);
            Assert.GreaterOrEqual(results.TotalTextures, 0);
            Assert.GreaterOrEqual(results.TotalMemoryUsage, 0);
        }

        [Test]
        public void AnalyzeGameObject_NullGameObject_ThrowsException()
        {
            // Arrange
            Assert.IsNotNull(sceneAnalyzer, "SceneAnalyzer should be initialized");

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => sceneAnalyzer.AnalyzeGameObject(null));
        }
    }
} 