using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using USDOptimizer.Core.Analysis.Implementations;
using USDOptimizer.Core.Analysis.Interfaces;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Tests.Core.Analysis
{
    [TestFixture]
    public class MaterialAnalyzerTests
    {
        private MaterialAnalyzer _analyzer;
        private USDScene _testScene;

        [SetUp]
        public void Setup()
        {
            _analyzer = new MaterialAnalyzer();
            _testScene = new USDScene 
            { 
                Name = "TestScene",
                Materials = new List<USDOptimizer.Core.Models.Material>(),
                Textures = new List<USDOptimizer.Core.Models.Texture>()
            };
            
            // Add some test materials
            for (int i = 0; i < 3; i++)
            {
                _testScene.Materials.Add(new USDOptimizer.Core.Models.Material
                {
                    Name = $"Material_{i}"
                });
            }
            
            // Add some test textures
            for (int i = 0; i < 4; i++)
            {
                _testScene.Textures.Add(new USDOptimizer.Core.Models.Texture
                {
                    Name = $"Texture_{i}",
                    Width = 512,
                    Height = 512,
                    Size = 512 * 512 * 4 // RGBA - 4 bytes per pixel
                });
            }
        }
        
        [Test]
        public async Task AnalyzeMaterialsAsync_ValidScene_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeMaterialsAsync(_testScene);
            
            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(3, metrics.TotalMaterials);
            Assert.AreEqual(4, metrics.TotalTextures);
            Assert.Greater(metrics.TextureMemoryUsage, 0);
            Assert.AreEqual(3, metrics.UniqueMaterials);
        }
        
        [Test]
        public async Task AnalyzeMaterialsAsync_HighResTextures_IdentifiesHighResTextures()
        {
            // Arrange
            _testScene.Textures.Add(new USDOptimizer.Core.Models.Texture
            {
                Name = "HighRes_1",
                Width = 2048,
                Height = 2048,
                Size = 2048 * 2048 * 4
            });
            
            _testScene.Textures.Add(new USDOptimizer.Core.Models.Texture
            {
                Name = "HighRes_2",
                Width = 4096,
                Height = 4096,
                Size = 4096 * 4096 * 4
            });
            
            // Act
            var metrics = await _analyzer.AnalyzeMaterialsAsync(_testScene);
            
            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(6, metrics.TotalTextures);
            Assert.AreEqual(2, metrics.HighResTextureCount);
            Assert.AreEqual(4096 * 4096, metrics.MaxTextureResolution);
        }
        
        [Test]
        public async Task AnalyzeMaterialsAsync_TextureMemoryCalculation_IsCorrect()
        {
            // Act
            var metrics = await _analyzer.AnalyzeMaterialsAsync(_testScene);
            
            // Assert - 4 textures at 512x512 with 4 bytes per pixel
            long expectedMemory = 4 * 512 * 512 * 4;
            Assert.AreEqual(expectedMemory, metrics.TextureMemoryUsage);
        }
        
        [Test]
        public async Task AnalyzeMaterialsAsync_EmptyScene_ReturnsZeroValues()
        {
            // Arrange
            var emptyScene = new USDScene
            {
                Name = "EmptyScene",
                Materials = new List<USDOptimizer.Core.Models.Material>(),
                Textures = new List<USDOptimizer.Core.Models.Texture>()
            };
            
            // Act
            var metrics = await _analyzer.AnalyzeMaterialsAsync(emptyScene);
            
            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(0, metrics.TotalMaterials);
            Assert.AreEqual(0, metrics.TotalTextures);
            Assert.AreEqual(0, metrics.TextureMemoryUsage);
            Assert.AreEqual(0, metrics.HighResTextureCount);
        }
        
        [Test]
        public async Task AnalyzeMaterialsAsync_NullLists_HandlesGracefully()
        {
            // Arrange
            var sceneWithNulls = new USDScene
            {
                Name = "NullListsScene",
                Materials = null,
                Textures = null
            };
            
            // Act
            var metrics = await _analyzer.AnalyzeMaterialsAsync(sceneWithNulls);
            
            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(0, metrics.TotalMaterials);
            Assert.AreEqual(0, metrics.TotalTextures);
        }
        
        [Test]
        public void AnalyzeMaterialsAsync_NullScene_ThrowsArgumentNullException()
        {
            // Assert
            Assert.ThrowsAsync<System.ArgumentNullException>(async () => 
                await _analyzer.AnalyzeMaterialsAsync(null));
        }
        
        [Test]
        public async Task AnalyzeMaterialsAsync_DuplicateMaterials_CountsUniqueCorrectly()
        {
            // Arrange
            var sceneWithDuplicates = new USDScene
            {
                Name = "DuplicatesScene",
                Materials = new List<USDOptimizer.Core.Models.Material>(),
                Textures = new List<USDOptimizer.Core.Models.Texture>()
            };
            
            // Add materials with same name (duplicates)
            sceneWithDuplicates.Materials.Add(new USDOptimizer.Core.Models.Material { Name = "Duplicate" });
            sceneWithDuplicates.Materials.Add(new USDOptimizer.Core.Models.Material { Name = "Duplicate" });
            sceneWithDuplicates.Materials.Add(new USDOptimizer.Core.Models.Material { Name = "Unique" });
            
            // Act
            var metrics = await _analyzer.AnalyzeMaterialsAsync(sceneWithDuplicates);
            
            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(3, metrics.TotalMaterials);
            Assert.AreEqual(2, metrics.UniqueMaterials); // Only 2 unique names
        }

        [Test]
        public async Task AnalyzeTextureUsageAsync_ValidMaterial_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeTextureUsageAsync(_testScene.Materials[0]);

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
            var material1 = _testScene.Materials[0];
            var material2 = _testScene.Materials[1];
            material2.Name = "SimilarMaterial";

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
            var metrics = await _analyzer.AnalyzeShaderComplexityAsync(_testScene.Materials[0]);

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