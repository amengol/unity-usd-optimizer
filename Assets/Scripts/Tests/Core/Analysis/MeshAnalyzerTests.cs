using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using USDOptimizer.Core.Analysis.Implementations;
using USDOptimizer.Core.Analysis.Interfaces;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Tests.Core.Analysis
{
    [TestFixture]
    public class MeshAnalyzerTests
    {
        private MeshAnalyzer _analyzer;
        private USDScene _testScene;
        private USDOptimizer.Core.Models.Mesh _testMesh;
        private USDOptimizer.Core.Models.Material _testMaterial;

        [SetUp]
        public void Setup()
        {
            _analyzer = new MeshAnalyzer();
            _testScene = new USDScene { Name = "TestScene" };
            _testMaterial = new USDOptimizer.Core.Models.Material { Name = "TestMaterial" };
            
            // Create test meshes
            _testScene.Meshes = new List<USDOptimizer.Core.Models.Mesh>();
            _testMesh = CreateTestMesh();
            _testScene.Meshes.Add(_testMesh);
        }

        [Test]
        public async Task AnalyzeMeshesAsync_ValidScene_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeMeshesAsync(_testScene);

            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(1, metrics.TotalMeshes);
            Assert.AreEqual(6, metrics.TotalPolygons);
            Assert.AreEqual(8, metrics.TotalVertices);
            Assert.Greater(metrics.MemoryUsage, 0);
            Assert.AreEqual(6, metrics.AveragePolygonsPerMesh);
        }

        [Test]
        public async Task AnalyzeMeshesAsync_MultipleHighPolyMeshes_CountsHighPolyMeshes()
        {
            // Arrange
            var highPolyMesh1 = CreateHighPolyMesh("HighPoly1", 15000);
            var highPolyMesh2 = CreateHighPolyMesh("HighPoly2", 20000);
            _testScene.Meshes.Add(highPolyMesh1);
            _testScene.Meshes.Add(highPolyMesh2);

            // Act
            var metrics = await _analyzer.AnalyzeMeshesAsync(_testScene);

            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(3, metrics.TotalMeshes);
            Assert.AreEqual(2, metrics.HighPolyMeshCount);
            Assert.AreEqual(35006, metrics.TotalPolygons);
            Assert.AreEqual(70012, metrics.TotalVertices);
            Assert.AreEqual(20000, metrics.MaxPolygonsInMesh);
        }

        [Test]
        public async Task AnalyzeMeshesAsync_EmptyScene_ReturnsZeroValues()
        {
            // Arrange
            var emptyScene = new USDScene { Name = "EmptyScene", Meshes = new List<USDOptimizer.Core.Models.Mesh>() };

            // Act
            var metrics = await _analyzer.AnalyzeMeshesAsync(emptyScene);

            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(0, metrics.TotalMeshes);
            Assert.AreEqual(0, metrics.TotalPolygons);
            Assert.AreEqual(0, metrics.TotalVertices);
            Assert.AreEqual(0, metrics.HighPolyMeshCount);
            Assert.AreEqual(0, metrics.MemoryUsage);
        }

        [Test]
        public async Task AnalyzeMeshesAsync_NullMeshList_HandlesGracefully()
        {
            // Arrange
            var sceneWithNullMeshes = new USDScene { Name = "NullMeshScene", Meshes = null };

            // Act
            var metrics = await _analyzer.AnalyzeMeshesAsync(sceneWithNullMeshes);

            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(0, metrics.TotalMeshes);
            Assert.AreEqual(0, metrics.TotalPolygons);
        }

        [Test]
        public void AnalyzeMeshesAsync_NullScene_ThrowsArgumentNullException()
        {
            // Assert
            Assert.ThrowsAsync<System.ArgumentNullException>(async () => 
                await _analyzer.AnalyzeMeshesAsync(null));
        }

        [Test]
        public async Task AnalyzeMeshesAsync_ComplexScene_CalculatesMemoryCorrectly()
        {
            // Arrange - create a scene with known vertex counts
            var scene = new USDScene { Name = "MemoryTestScene", Meshes = new List<USDOptimizer.Core.Models.Mesh>() };
            var mesh1 = CreateMeshWithVertexCount("Mesh1", 1000);
            var mesh2 = CreateMeshWithVertexCount("Mesh2", 2000);
            scene.Meshes.Add(mesh1);
            scene.Meshes.Add(mesh2);

            // Act
            var metrics = await _analyzer.AnalyzeMeshesAsync(scene);

            // Assert - verify memory calculation (32 bytes per vertex as per implementation)
            Assert.AreEqual(3000, metrics.TotalVertices);
            Assert.AreEqual(3000 * 32, metrics.MemoryUsage);
        }

        private USDOptimizer.Core.Models.Mesh CreateTestMesh()
        {
            return new USDOptimizer.Core.Models.Mesh
            {
                Name = "TestMesh",
                PolygonCount = 6,
                VertexCount = 8,
                Material = _testMaterial
            };
        }

        private USDOptimizer.Core.Models.Mesh CreateHighPolyMesh(string name, int polyCount)
        {
            return new USDOptimizer.Core.Models.Mesh
            {
                Name = name,
                PolygonCount = polyCount,
                VertexCount = polyCount * 2, // Roughly 2 vertices per polygon
                Material = _testMaterial
            };
        }

        private USDOptimizer.Core.Models.Mesh CreateMeshWithVertexCount(string name, int vertexCount)
        {
            return new USDOptimizer.Core.Models.Mesh
            {
                Name = name,
                PolygonCount = vertexCount / 2, // Roughly 2 vertices per polygon
                VertexCount = vertexCount,
                Material = _testMaterial
            };
        }
    }
} 