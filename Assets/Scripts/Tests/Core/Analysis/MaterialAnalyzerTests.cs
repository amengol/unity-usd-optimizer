using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using USDOptimizer.Core.Analysis.Implementations;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Tests.Core.Analysis
{
    [TestFixture]
    public class MaterialAnalyzerTests
    {
        private MaterialAnalyzer _analyzer;
        private Scene _testScene;
        private Material _testMaterial;

        [SetUp]
        public void Setup()
        {
            _analyzer = new MaterialAnalyzer();
            _testScene = new Scene { Name = "TestScene" };
            _testMaterial = CreateTestMaterial();
            _testScene.Materials.Add(_testMaterial);
        }

        [Test]
        public async Task AnalyzeMaterialsAsync_ValidMaterial_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeMaterialsAsync(_testScene);

            // Assert
            Assert.That(metrics.TotalMaterialCount, Is.EqualTo(1));
            Assert.That(metrics.UniqueTextureCount, Is.EqualTo(2));
            Assert.That(metrics.MaterialTextureUsage[_testMaterial.Name].TextureCount, Is.EqualTo(2));
            Assert.That(metrics.MaterialShaderComplexity[_testMaterial.Name].PropertyCount, Is.EqualTo(3));
        }

        [Test]
        public async Task AnalyzeTextureUsageAsync_ValidMaterial_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeTextureUsageAsync(_testMaterial);

            // Assert
            Assert.That(metrics.TextureCount, Is.EqualTo(2));
            Assert.That(metrics.TextureTypeCounts[TextureType.Albedo], Is.EqualTo(1));
            Assert.That(metrics.TextureTypeCounts[TextureType.Normal], Is.EqualTo(1));
            Assert.That(metrics.HasExcessiveTextureUsage, Is.False);
        }

        [Test]
        public async Task AnalyzeTextureUsageAsync_HighTextureCount_DetectsExcessiveUsage()
        {
            // Arrange
            var material = CreateTestMaterialWithManyTextures();

            // Act
            var metrics = await _analyzer.AnalyzeTextureUsageAsync(material);

            // Assert
            Assert.That(metrics.HasExcessiveTextureUsage, Is.True);
            Assert.That(metrics.TextureIssues, Contains.Item($"Material uses {metrics.TextureCount} textures, exceeding threshold of 8"));
        }

        [Test]
        public async Task DetectMaterialRedundancyAsync_SimilarMaterials_DetectsRedundancy()
        {
            // Arrange
            var material1 = CreateTestMaterial();
            var material2 = CreateTestMaterial();
            material2.Name = "SimilarMaterial";
            _testScene.Materials.Add(material2);

            // Act
            var groups = await _analyzer.DetectMaterialRedundancyAsync(_testScene);

            // Assert
            Assert.That(groups.Count, Is.EqualTo(1));
            Assert.That(groups[0].MaterialNames.Count, Is.EqualTo(2));
            Assert.That(groups[0].SimilarityScore, Is.GreaterThanOrEqualTo(0.95f));
        }

        [Test]
        public async Task AnalyzeShaderComplexityAsync_ValidMaterial_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeShaderComplexityAsync(_testMaterial);

            // Assert
            Assert.That(metrics.PropertyCount, Is.EqualTo(3));
            Assert.That(metrics.TextureSamplerCount, Is.EqualTo(2));
            Assert.That(metrics.KeywordCount, Is.EqualTo(2));
            Assert.That(metrics.HasHighComplexity, Is.False);
        }

        [Test]
        public async Task AnalyzeShaderComplexityAsync_HighComplexity_DetectsHighComplexity()
        {
            // Arrange
            var material = CreateTestMaterialWithHighComplexity();

            // Act
            var metrics = await _analyzer.AnalyzeShaderComplexityAsync(material);

            // Assert
            Assert.That(metrics.HasHighComplexity, Is.True);
            Assert.That(metrics.ComplexityIssues, Contains.Item($"Shader has {metrics.PropertyCount} properties, exceeding threshold of 20"));
        }

        private Material CreateTestMaterial()
        {
            return new Material
            {
                Name = "TestMaterial",
                Textures = new List<Texture>
                {
                    new Texture { Name = "albedo", Width = 1024, Height = 1024 },
                    new Texture { Name = "normal", Width = 1024, Height = 1024 }
                },
                Properties = new Dictionary<string, object>
                {
                    { "Metallic", 0.5f },
                    { "Roughness", 0.7f },
                    { "Emission", 0.0f }
                },
                ShaderKeywords = new List<string> { "NORMALMAP", "METALLIC" }
            };
        }

        private Material CreateTestMaterialWithManyTextures()
        {
            var material = new Material { Name = "HighTextureMaterial" };
            
            // Add 10 textures to exceed the threshold
            for (int i = 0; i < 10; i++)
            {
                material.Textures.Add(new Texture 
                { 
                    Name = $"texture_{i}", 
                    Width = 1024, 
                    Height = 1024 
                });
            }

            material.Properties = new Dictionary<string, object>
            {
                { "Metallic", 0.5f },
                { "Roughness", 0.7f }
            };

            material.ShaderKeywords = new List<string> { "NORMALMAP" };

            return material;
        }

        private Material CreateTestMaterialWithHighComplexity()
        {
            var material = new Material { Name = "HighComplexityMaterial" };
            
            // Add many properties to exceed the threshold
            for (int i = 0; i < 25; i++)
            {
                material.Properties.Add($"Property_{i}", i);
            }

            material.Textures = new List<Texture>
            {
                new Texture { Name = "albedo", Width = 1024, Height = 1024 }
            };

            material.ShaderKeywords = new List<string> 
            { 
                "NORMALMAP", "METALLIC", "ROUGHNESS", "OCCLUSION", "EMISSION", "HEIGHT" 
            };

            return material;
        }
    }
} 