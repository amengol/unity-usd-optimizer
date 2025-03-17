using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization.Implementations;

namespace USDOptimizer.Tests.Core.Optimization
{
    [TestFixture]
    public class MeshOptimizerTests
    {
        private MeshOptimizer _optimizer;
        private Mesh _testMesh;
        private Material _testMaterial;

        [SetUp]
        public void Setup()
        {
            _optimizer = new MeshOptimizer();
            _testMaterial = new Material { Name = "TestMaterial" };
            _testMesh = CreateTestMesh();
        }

        [Test]
        public async Task GenerateLODsAsync_ValidMesh_ReturnsCorrectNumberOfLODs()
        {
            // Arrange
            var lodLevels = 3;
            var reductionFactors = new float[] { 0.75f, 0.5f, 0.25f };

            // Act
            var lods = await _optimizer.GenerateLODsAsync(_testMesh, lodLevels, reductionFactors);

            // Assert
            Assert.That(lods.Length, Is.EqualTo(lodLevels));
            Assert.That(lods[0].PolygonIndices.Count, Is.LessThan(_testMesh.PolygonIndices.Count));
            Assert.That(lods[1].PolygonIndices.Count, Is.LessThan(lods[0].PolygonIndices.Count));
            Assert.That(lods[2].PolygonIndices.Count, Is.LessThan(lods[1].PolygonIndices.Count));
        }

        [Test]
        public async Task GenerateLODsAsync_InvalidParameters_ThrowsArgumentException()
        {
            // Arrange
            var invalidLevels = 0;
            var invalidFactors = new float[] { 0.5f };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => 
                _optimizer.GenerateLODsAsync(_testMesh, invalidLevels, invalidFactors));
        }

        [Test]
        public async Task SimplifyMeshAsync_ValidMesh_ReturnsSimplifiedMesh()
        {
            // Arrange
            var targetPolygonCount = 100;

            // Act
            var simplifiedMesh = await _optimizer.SimplifyMeshAsync(_testMesh, targetPolygonCount);

            // Assert
            Assert.That(simplifiedMesh.PolygonIndices.Count, Is.LessThanOrEqualTo(targetPolygonCount * 3));
            Assert.That(simplifiedMesh.Vertices.Count, Is.LessThanOrEqualTo(_testMesh.Vertices.Count));
        }

        [Test]
        public async Task OptimizeVerticesAsync_ValidMesh_ReturnsOptimizedMesh()
        {
            // Act
            var optimizedMesh = await _optimizer.OptimizeVerticesAsync(_testMesh);

            // Assert
            Assert.That(optimizedMesh.Vertices.Count, Is.LessThanOrEqualTo(_testMesh.Vertices.Count));
            Assert.That(optimizedMesh.PolygonIndices.Count, Is.EqualTo(_testMesh.PolygonIndices.Count));
        }

        [Test]
        public async Task MergeMeshesAsync_ValidMeshes_ReturnsMergedMesh()
        {
            // Arrange
            var mesh1 = CreateTestMesh("Mesh1");
            var mesh2 = CreateTestMesh("Mesh2");
            var meshes = new[] { mesh1, mesh2 };

            // Act
            var mergedMesh = await _optimizer.MergeMeshesAsync(meshes);

            // Assert
            Assert.That(mergedMesh.Vertices.Count, Is.EqualTo(mesh1.Vertices.Count + mesh2.Vertices.Count));
            Assert.That(mergedMesh.PolygonIndices.Count, Is.EqualTo(mesh1.PolygonIndices.Count + mesh2.PolygonIndices.Count));
        }

        [Test]
        public async Task MergeMeshesAsync_SingleMesh_ReturnsOriginalMesh()
        {
            // Arrange
            var meshes = new[] { _testMesh };

            // Act
            var mergedMesh = await _optimizer.MergeMeshesAsync(meshes);

            // Assert
            Assert.That(mergedMesh, Is.SameAs(_testMesh));
        }

        [Test]
        public async Task MergeMeshesAsync_EmptyArray_ThrowsArgumentException()
        {
            // Arrange
            var emptyMeshes = Array.Empty<Mesh>();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => 
                _optimizer.MergeMeshesAsync(emptyMeshes));
        }

        private Mesh CreateTestMesh(string name = "TestMesh")
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();
            var indices = new List<int>();

            // Create a simple cube mesh
            for (int i = 0; i < 8; i++)
            {
                vertices.Add(new Vector3(
                    (i & 1) == 0 ? -1 : 1,
                    (i & 2) == 0 ? -1 : 1,
                    (i & 4) == 0 ? -1 : 1
                ));
                uvs.Add(new Vector2((i & 1) == 0 ? 0 : 1, (i & 2) == 0 ? 0 : 1));
                normals.Add(new Vector3(0, 1, 0));
                tangents.Add(new Vector4(1, 0, 0, -1));
            }

            // Add triangles
            indices.AddRange(new int[]
            {
                0, 1, 2, 1, 3, 2,  // Front face
                4, 5, 6, 5, 7, 6,  // Back face
                0, 4, 1, 4, 5, 1,  // Left face
                1, 5, 3, 5, 7, 3,  // Right face
                2, 3, 6, 3, 7, 6,  // Top face
                0, 2, 4, 2, 6, 4   // Bottom face
            });

            return new Mesh
            {
                Name = name,
                Material = _testMaterial,
                Vertices = vertices,
                UVs = uvs,
                Normals = normals,
                Tangents = tangents,
                PolygonIndices = indices,
                BoundingBox = new Bounds
                {
                    Center = Vector3.zero,
                    Size = new Vector3(2, 2, 2)
                }
            };
        }
    }
} 