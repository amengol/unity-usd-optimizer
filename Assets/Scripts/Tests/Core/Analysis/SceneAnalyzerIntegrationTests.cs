using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using USDOptimizer.Core.Analysis.Implementations;
using USDOptimizer.Core.Analysis.Interfaces;
using USDOptimizer.Core.Models;
using USDSceneOptimizer;

namespace USDOptimizer.Tests.Core.Analysis
{
    [TestFixture]
    public class SceneAnalyzerIntegrationTests
    {
        private SceneAnalyzer _sceneAnalyzer;
        private IMeshAnalyzer _mockMeshAnalyzer;
        private IMaterialAnalyzer _mockMaterialAnalyzer;
        private ISceneHierarchyAnalyzer _mockHierarchyAnalyzer;
        private USDScene _testScene;
        
        [SetUp]
        public void Setup()
        {
            // Create mock analyzers
            _mockMeshAnalyzer = Substitute.For<IMeshAnalyzer>();
            _mockMaterialAnalyzer = Substitute.For<IMaterialAnalyzer>();
            _mockHierarchyAnalyzer = Substitute.For<ISceneHierarchyAnalyzer>();
            
            // Setup test data
            SetupMockAnalyzers();
            
            // Create test scene
            _testScene = CreateTestScene();
            
            // Create SceneAnalyzer with mock components
            _sceneAnalyzer = new SceneAnalyzer(
                _mockMeshAnalyzer, 
                _mockMaterialAnalyzer, 
                _mockHierarchyAnalyzer);
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up resources if needed
        }
        
        [Test]
        public async Task AnalyzeSceneAsync_CombinesResultsFromSpecializedAnalyzers()
        {
            // Act
            var results = await _sceneAnalyzer.AnalyzeSceneAsync(_testScene);
            
            // Assert
            Assert.NotNull(results);
            
            // Verify it uses data from mesh analyzer
            Assert.AreEqual(100, results.TotalPolygons);
            Assert.AreEqual(200, results.TotalVertices);
            
            // Verify it uses data from material analyzer
            Assert.AreEqual(5, results.TotalMaterials);
            Assert.AreEqual(10, results.TotalTextures);
            
            // Verify it uses data from hierarchy analyzer
            Assert.AreEqual(15, results.TotalNodes);
            Assert.AreEqual(3, results.HierarchyDepth);
            
            // Verify combined metrics
            Assert.Greater(results.TotalMemoryUsage, 0);
            Assert.GreaterOrEqual(results.OptimizationPotential, 0);
            Assert.LessOrEqual(results.OptimizationPotential, 100);
        }
        
        [Test]
        public async Task AnalyzeSceneAsync_GeneratesRecommendationsBasedOnMetrics()
        {
            // Act
            var results = await _sceneAnalyzer.AnalyzeSceneAsync(_testScene);
            
            // Assert
            Assert.NotNull(results.Recommendations);
            Assert.Greater(results.Recommendations.Count, 0);
            
            // Verify recommendation structure
            var firstRecommendation = results.Recommendations[0];
            Assert.NotNull(firstRecommendation.Title);
            Assert.NotNull(firstRecommendation.Description);
            Assert.NotNull(firstRecommendation.Category);
        }
        
        [Test]
        public void AnalyzeSceneAsync_NullScene_ThrowsArgumentNullException()
        {
            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _sceneAnalyzer.AnalyzeSceneAsync(null));
        }
        
        [Test]
        public async Task AnalyzeSceneAsync_CalculatesOptimizationPotentialCorrectly()
        {
            // Arrange - configure for high polygon count
            var meshMetrics = new MeshMetrics { TotalPolygons = 1500000, TotalVertices = 3000000 };
            _mockMeshAnalyzer.AnalyzeMeshesAsync(Arg.Any<USDScene>()).Returns(Task.FromResult(meshMetrics));
            
            // Act
            var results = await _sceneAnalyzer.AnalyzeSceneAsync(_testScene);
            
            // Assert
            Assert.Greater(results.OptimizationPotential, 50); // Should be high due to polygon count
            Assert.Greater(results.Recommendations.Count, 1); // Should recommend mesh simplification
            
            // Find mesh simplification recommendation
            var meshRecommendation = results.Recommendations.Find(r => r.Category == "Mesh");
            Assert.NotNull(meshRecommendation);
            Assert.AreEqual(OptimizationPriorityLevel.Critical, meshRecommendation.Priority);
        }
        
        [Test]
        public async Task AnalyzeSceneAsync_UsesAllAnalyzerComponentsCorrectly()
        {
            // Act
            await _sceneAnalyzer.AnalyzeSceneAsync(_testScene);
            
            // Assert - verify all analyzers were called exactly once
            await _mockMeshAnalyzer.Received(1).AnalyzeMeshesAsync(_testScene);
            await _mockMaterialAnalyzer.Received(1).AnalyzeMaterialsAsync(_testScene);
            await _mockHierarchyAnalyzer.Received(1).AnalyzeHierarchyAsync(_testScene);
        }
        
        private void SetupMockAnalyzers()
        {
            // Setup mesh analyzer mock
            var meshMetrics = new MeshMetrics
            {
                TotalMeshes = 50,
                TotalPolygons = 100,
                TotalVertices = 200,
                MemoryUsage = 6400, // 200 vertices * 32 bytes
                AveragePolygonsPerMesh = 2,
                MaxPolygonsInMesh = 10,
                HighPolyMeshCount = 0
            };
            _mockMeshAnalyzer.AnalyzeMeshesAsync(Arg.Any<USDScene>()).Returns(Task.FromResult(meshMetrics));
            
            // Setup material analyzer mock
            var materialMetrics = new MaterialMetrics
            {
                TotalMaterials = 5,
                TotalTextures = 10,
                TextureMemoryUsage = 10240, // 10 textures at some size
                UniqueMaterials = 3,
                HighTextureCountMaterials = 1,
                HighResTextureCount = 2,
                MaxTextureResolution = 1024 * 1024
            };
            _mockMaterialAnalyzer.AnalyzeMaterialsAsync(Arg.Any<USDScene>()).Returns(Task.FromResult(materialMetrics));
            
            // Setup hierarchy analyzer mock
            var hierarchyMetrics = new HierarchyMetrics
            {
                TotalNodes = 15,
                MaxHierarchyDepth = 3,
                AverageChildrenPerNode = 2.5f,
                MaxChildrenPerNode = 5,
                EmptyNodeCount = 2,
                DeeplyNestedNodeCount = 0,
                NodeTypeCounts = new Dictionary<string, int>
                {
                    { "Mesh", 5 },
                    { "Material", 3 },
                    { "Transform", 7 }
                }
            };
            _mockHierarchyAnalyzer.AnalyzeHierarchyAsync(Arg.Any<USDScene>()).Returns(Task.FromResult(hierarchyMetrics));
        }
        
        private USDScene CreateTestScene()
        {
            return new USDScene
            {
                Name = "TestScene",
                FilePath = "test.usda",
                ImportDate = DateTime.Now,
                Nodes = new List<USDNode>
                {
                    new USDNode { Name = "Root" }
                },
                Meshes = new List<USDOptimizer.Core.Models.Mesh>
                {
                    new USDOptimizer.Core.Models.Mesh { Name = "TestMesh" }
                },
                Materials = new List<USDOptimizer.Core.Models.Material>
                {
                    new USDOptimizer.Core.Models.Material { Name = "TestMaterial" }
                },
                Textures = new List<USDOptimizer.Core.Models.Texture>
                {
                    new USDOptimizer.Core.Models.Texture
                    {
                        Name = "TestTexture",
                        Width = 512,
                        Height = 512
                    }
                }
            };
        }
    }
} 