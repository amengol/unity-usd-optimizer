using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using USDOptimizer.Core.Analysis.Implementations;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Tests.Core.Analysis
{
    [TestFixture]
    public class MeshAnalyzerTests
    {
        private MeshAnalyzer _analyzer;
        private Scene _testScene;
        private Mesh _testMesh;

        [SetUp]
        public void Setup()
        {
            _analyzer = new MeshAnalyzer();
            _testScene = new Scene { Name = "TestScene" };
            _testMesh = CreateTestMesh();
            _testScene.Meshes.Add(_testMesh);
        }

        [Test]
        public async Task AnalyzeMeshAsync_ValidMesh_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeMeshAsync(_testScene);

            // Assert
            Assert.That(metrics.TotalPolygonCount, Is.EqualTo(2)); // 6 indices = 2 triangles
            Assert.That(metrics.TotalVertexCount, Is.EqualTo(4));
            Assert.That(metrics.MeshPolygonCounts[_testMesh.Name], Is.EqualTo(2));
            Assert.That(metrics.MeshVertexCounts[_testMesh.Name], Is.EqualTo(4));
        }

        [Test]
        public async Task CalculatePolygonCountAsync_ValidMesh_ReturnsCorrectCount()
        {
            // Act
            var count = await _analyzer.CalculatePolygonCountAsync(_testMesh);

            // Assert
            Assert.That(count, Is.EqualTo(2)); // 6 indices = 2 triangles
        }

        [Test]
        public async Task CalculateVertexDensityAsync_ValidMesh_ReturnsCorrectDensity()
        {
            // Act
            var metrics = await _analyzer.CalculateVertexDensityAsync(_testMesh);

            // Assert
            Assert.That(metrics.SurfaceArea, Is.GreaterThan(0));
            Assert.That(metrics.VerticesPerUnitArea, Is.GreaterThan(0));
        }

        [Test]
        public async Task AnalyzeUVMappingAsync_ValidMesh_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeUVMappingAsync(_testMesh);

            // Assert
            Assert.That(metrics.UVSetCount, Is.EqualTo(1));
            Assert.That(metrics.HasOverlappingUVs, Is.False);
            Assert.That(metrics.HasUVSeams, Is.False);
            Assert.That(metrics.HasProperUVLayout, Is.True);
        }

        [Test]
        public async Task AnalyzeUVMappingAsync_OverlappingUVs_DetectsOverlap()
        {
            // Arrange
            var mesh = CreateTestMeshWithOverlappingUVs();

            // Act
            var metrics = await _analyzer.AnalyzeUVMappingAsync(mesh);

            // Assert
            Assert.That(metrics.HasOverlappingUVs, Is.True);
            Assert.That(metrics.UVIssues, Contains.Item("Mesh has overlapping UV coordinates"));
        }

        [Test]
        public async Task AnalyzeUVMappingAsync_UVSeams_DetectsSeams()
        {
            // Arrange
            var mesh = CreateTestMeshWithUVSeams();

            // Act
            var metrics = await _analyzer.AnalyzeUVMappingAsync(mesh);

            // Assert
            Assert.That(metrics.HasUVSeams, Is.True);
            Assert.That(metrics.UVIssues, Contains.Item("Mesh has UV seams"));
        }

        private Mesh CreateTestMesh()
        {
            return new Mesh
            {
                Name = "TestMesh",
                Vertices = new List<Vector3>
                {
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 1, 0)
                },
                UVs = new List<Vector2>
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                },
                PolygonIndices = new List<int> { 0, 1, 2, 1, 3, 2 },
                Bounds = new Bounds(
                    new Vector3(0.5f, 0.5f, 0),
                    new Vector3(1, 1, 0)
                )
            };
        }

        private Mesh CreateTestMeshWithOverlappingUVs()
        {
            return new Mesh
            {
                Name = "OverlappingUVMesh",
                Vertices = new List<Vector3>
                {
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 1, 0)
                },
                UVs = new List<Vector2>
                {
                    new Vector2(0, 0),
                    new Vector2(0, 0), // Overlapping UV
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                },
                PolygonIndices = new List<int> { 0, 1, 2, 1, 3, 2 },
                Bounds = new Bounds(
                    new Vector3(0.5f, 0.5f, 0),
                    new Vector3(1, 1, 0)
                )
            };
        }

        private Mesh CreateTestMeshWithUVSeams()
        {
            return new Mesh
            {
                Name = "SeamedUVMesh",
                Vertices = new List<Vector3>
                {
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 1, 0)
                },
                UVs = new List<Vector2>
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(2, 1) // Large UV discontinuity
                },
                PolygonIndices = new List<int> { 0, 1, 2, 1, 3, 2 },
                Bounds = new Bounds(
                    new Vector3(0.5f, 0.5f, 0),
                    new Vector3(1, 1, 0)
                )
            };
        }
    }
} 