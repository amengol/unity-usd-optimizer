using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization.Implementations;

namespace USDOptimizer.Tests.Core.Optimization
{
    [TestFixture]
    public class MaterialOptimizerTests
    {
        private MaterialOptimizer _optimizer;
        private Material _testMaterial;
        private Texture _testTexture;

        [SetUp]
        public void Setup()
        {
            _optimizer = new MaterialOptimizer();
            _testTexture = CreateTestTexture();
            _testMaterial = CreateTestMaterial();
        }

        [Test]
        public async Task CompressTexturesAsync_ValidMaterial_ReturnsCompressedMaterial()
        {
            // Arrange
            var compressionQuality = 0.5f;

            // Act
            var compressedMaterial = await _optimizer.CompressTexturesAsync(_testMaterial, compressionQuality);

            // Assert
            Assert.That(compressedMaterial.Name, Is.EqualTo($"{_testMaterial.Name}_Compressed"));
            Assert.That(compressedMaterial.Properties.Count, Is.EqualTo(_testMaterial.Properties.Count));

            foreach (var property in compressedMaterial.Properties)
            {
                if (property.Value is Texture texture)
                {
                    Assert.That(texture.Width, Is.LessThanOrEqualTo(_testTexture.Width));
                    Assert.That(texture.Height, Is.LessThanOrEqualTo(_testTexture.Height));
                }
            }
        }

        [Test]
        public async Task CompressTexturesAsync_InvalidQuality_ThrowsArgumentException()
        {
            // Arrange
            var invalidQuality = 1.5f;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => 
                _optimizer.CompressTexturesAsync(_testMaterial, invalidQuality));
        }

        [Test]
        public async Task BatchMaterialsAsync_ValidMaterials_ReturnsBatchedMaterial()
        {
            // Arrange
            var material1 = CreateTestMaterial("Material1");
            var material2 = CreateTestMaterial("Material2");
            var materials = new[] { material1, material2 };

            // Act
            var batchedMaterial = await _optimizer.BatchMaterialsAsync(materials);

            // Assert
            Assert.That(batchedMaterial.Name, Is.EqualTo("BatchedMaterial"));
            Assert.That(batchedMaterial.Properties.Count, Is.GreaterThan(0));
        }

        [Test]
        public async Task BatchMaterialsAsync_SingleMaterial_ReturnsOriginalMaterial()
        {
            // Arrange
            var materials = new[] { _testMaterial };

            // Act
            var batchedMaterial = await _optimizer.BatchMaterialsAsync(materials);

            // Assert
            Assert.That(batchedMaterial, Is.SameAs(_testMaterial));
        }

        [Test]
        public async Task OptimizeShaderAsync_ValidMaterial_ReturnsOptimizedMaterial()
        {
            // Arrange
            var targetComplexity = 0.5f;

            // Act
            var optimizedMaterial = await _optimizer.OptimizeShaderAsync(_testMaterial, targetComplexity);

            // Assert
            Assert.That(optimizedMaterial.Name, Is.EqualTo($"{_testMaterial.Name}_Optimized"));
            Assert.That(optimizedMaterial.Properties.Count, Is.LessThanOrEqualTo(_testMaterial.Properties.Count));
        }

        [Test]
        public async Task OptimizeShaderAsync_InvalidComplexity_ThrowsArgumentException()
        {
            // Arrange
            var invalidComplexity = -0.5f;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => 
                _optimizer.OptimizeShaderAsync(_testMaterial, invalidComplexity));
        }

        [Test]
        public async Task MergeSimilarMaterialsAsync_ValidMaterials_ReturnsMergedMaterials()
        {
            // Arrange
            var material1 = CreateTestMaterial("Material1");
            var material2 = CreateTestMaterial("Material2");
            var material3 = CreateTestMaterial("Material3");
            var materials = new[] { material1, material2, material3 };
            var similarityThreshold = 0.8f;

            // Act
            var mergedMaterials = await _optimizer.MergeSimilarMaterialsAsync(materials, similarityThreshold);

            // Assert
            Assert.That(mergedMaterials.Length, Is.LessThanOrEqualTo(materials.Length));
            Assert.That(mergedMaterials.All(m => m.Properties.Count > 0), Is.True);
        }

        [Test]
        public async Task MergeSimilarMaterialsAsync_InvalidThreshold_ThrowsArgumentException()
        {
            // Arrange
            var materials = new[] { _testMaterial };
            var invalidThreshold = 1.5f;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => 
                _optimizer.MergeSimilarMaterialsAsync(materials, invalidThreshold));
        }

        private Material CreateTestMaterial(string name = "TestMaterial")
        {
            return new Material
            {
                Name = name,
                Shader = "Standard",
                Properties = new Dictionary<string, object>
                {
                    { "_MainTex", _testTexture },
                    { "_Color", new Vector4(1, 1, 1, 1) },
                    { "_Metallic", 0.5f },
                    { "_Smoothness", 0.5f },
                    { "_BumpMap", CreateTestTexture("BumpMap") },
                    { "_BumpScale", 1.0f },
                    { "_EmissionColor", new Vector4(0, 0, 0, 1) },
                    { "_EmissionMap", CreateTestTexture("EmissionMap") }
                }
            };
        }

        private Texture CreateTestTexture(string name = "TestTexture")
        {
            return new Texture
            {
                Name = name,
                Width = 1024,
                Height = 1024,
                Format = "RGBA32"
            };
        }
    }
} 