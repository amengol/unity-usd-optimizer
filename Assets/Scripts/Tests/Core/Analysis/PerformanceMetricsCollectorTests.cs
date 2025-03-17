using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using USDOptimizer.Core.Analysis.Implementations;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Tests.Core.Analysis
{
    [TestFixture]
    public class PerformanceMetricsCollectorTests
    {
        private PerformanceMetricsCollector _collector;
        private Scene _testScene;
        private Mesh _testMesh;
        private Material _testMaterial;
        private Texture _testTexture;

        [SetUp]
        public void Setup()
        {
            _collector = new PerformanceMetricsCollector();
            _testScene = CreateTestScene();
        }

        [Test]
        public async Task CollectMemoryUsageAsync_ValidScene_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _collector.CollectMemoryUsageAsync(_testScene);

            // Assert
            Assert.That(metrics.TotalMemoryUsageMB, Is.GreaterThan(0));
            Assert.That(metrics.MemoryUsageByMesh.ContainsKey(_testMesh.Name), Is.True);
            Assert.That(metrics.MemoryUsageByTexture.ContainsKey(_testTexture.Name), Is.True);
            Assert.That(metrics.MemoryUsageByComponent.ContainsKey("Materials"), Is.True);
            Assert.That(metrics.MemoryUsageByComponent.ContainsKey("Nodes"), Is.True);
        }

        [Test]
        public async Task AnalyzeDrawCallsAsync_ValidScene_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _collector.AnalyzeDrawCallsAsync(_testScene);

            // Assert
            Assert.That(metrics.TotalDrawCalls, Is.EqualTo(1));
            Assert.That(metrics.DrawCallsByMaterial.ContainsKey(_testMaterial.Name), Is.True);
            Assert.That(metrics.DrawCallsByMaterial[_testMaterial.Name], Is.EqualTo(1));
            Assert.That(metrics.DrawCallsByMesh.ContainsKey(_testMesh.Name), Is.True);
            Assert.That(metrics.DrawCallsByMesh[_testMesh.Name], Is.EqualTo(1));
        }

        [Test]
        public async Task AnalyzeDrawCallsAsync_MultipleDrawCalls_DetectsBatchingOpportunities()
        {
            // Arrange
            var material = new Material { Name = "TestMaterial" };
            var mesh1 = CreateTestMesh("Mesh1", material);
            var mesh2 = CreateTestMesh("Mesh2", material);
            _testScene.Meshes.Add(mesh1);
            _testScene.Meshes.Add(mesh2);

            // Act
            var metrics = await _collector.AnalyzeDrawCallsAsync(_testScene);

            // Assert
            Assert.That(metrics.TotalDrawCalls, Is.EqualTo(2));
            Assert.That(metrics.OptimizationOpportunities.Count, Is.EqualTo(1));
            Assert.That(metrics.OptimizationOpportunities[0].Type, Is.EqualTo(PerformanceOptimizationType.BatchingOptimization));
        }

        [Test]
        public async Task CollectPerformanceMetricsAsync_ValidScene_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _collector.CollectPerformanceMetricsAsync(_testScene);

            // Assert
            Assert.That(metrics.MemoryUsage, Is.Not.Null);
            Assert.That(metrics.DrawCalls, Is.Not.Null);
            Assert.That(metrics.EstimatedFrameTime, Is.GreaterThan(0));
            Assert.That(metrics.EstimatedMemoryBandwidth, Is.GreaterThan(0));
        }

        [Test]
        public async Task CollectPerformanceMetricsAsync_HighMemoryUsage_DetectsOptimizationOpportunities()
        {
            // Arrange
            var largeMesh = CreateLargeMesh();
            _testScene.Meshes.Add(largeMesh);

            // Act
            var metrics = await _collector.CollectPerformanceMetricsAsync(_testScene);

            // Assert
            Assert.That(metrics.OptimizationOpportunities.Count, Is.GreaterThan(0));
            Assert.That(metrics.OptimizationOpportunities.Any(o => o.Type == PerformanceOptimizationType.MemoryOptimization), Is.True);
        }

        private Scene CreateTestScene()
        {
            _testMaterial = new Material { Name = "TestMaterial" };
            _testTexture = new Texture { Name = "TestTexture", Width = 1024, Height = 1024 };
            _testMesh = CreateTestMesh("TestMesh", _testMaterial);

            return new Scene
            {
                Name = "TestScene",
                Meshes = new List<Mesh> { _testMesh },
                Materials = new List<Material> { _testMaterial },
                Textures = new List<Texture> { _testTexture },
                RootNode = new Node
                {
                    Name = "Root",
                    Transform = Matrix4x4.identity,
                    Children = new List<Node>
                    {
                        new Node
                        {
                            Name = "Child",
                            Transform = Matrix4x4.identity,
                            Mesh = _testMesh,
                            Material = _testMaterial
                        }
                    }
                }
            };
        }

        private Mesh CreateTestMesh(string name, Material material)
        {
            return new Mesh
            {
                Name = name,
                Material = material,
                Vertices = new List<Vector3>
                {
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0)
                },
                PolygonIndices = new List<int> { 0, 1, 2 }
            };
        }

        private Mesh CreateLargeMesh()
        {
            var vertices = new List<Vector3>();
            var indices = new List<int>();

            // Create a large mesh with 1M vertices and 2M triangles
            for (int i = 0; i < 1000000; i++)
            {
                vertices.Add(new Vector3(i, 0, 0));
                if (i < 999999)
                {
                    indices.AddRange(new[] { i, i + 1, i + 2 });
                }
            }

            return new Mesh
            {
                Name = "LargeMesh",
                Material = _testMaterial,
                Vertices = vertices,
                PolygonIndices = indices
            };
        }
    }
} 