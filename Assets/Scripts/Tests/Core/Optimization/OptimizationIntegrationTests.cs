using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization;
using USDOptimizer.Core.Analysis.Implementations;
using USDOptimizer.Core.Analysis.Interfaces;

namespace USDOptimizer.Tests.Core.Optimization
{
    [TestFixture]
    public class OptimizationIntegrationTests
    {
        private SceneOptimizer _sceneOptimizer;
        private USDScene _testScene;
        private MeshOptimizer _meshOptimizer;
        private SceneOptimizationSettings _performanceSettings;
        private SceneOptimizationSettings _qualitySettings;
        
        [SetUp]
        public void Setup()
        {
            // Create real components (not mocks) for integration tests
            _meshOptimizer = new MeshOptimizer();
            _sceneOptimizer = new SceneOptimizer();
            
            // Create a test scene with various mesh/material data
            _testScene = CreateTestScene();
            
            // Create different optimization settings profiles
            _performanceSettings = CreatePerformanceSettings();
            _qualitySettings = CreateQualitySettings();
        }
        
        [Test]
        public async Task OptimizeScene_WithPerformanceProfile_MaximizesOptimization()
        {
            // Act
            var optimizedScene = await _sceneOptimizer.OptimizeSceneAsync(_testScene, _performanceSettings);
            
            // Assert
            Assert.That(optimizedScene, Is.Not.Null);
            Assert.That(optimizedScene.Statistics.TotalPolygons, Is.LessThan(_testScene.Statistics.TotalPolygons));
            Assert.That(optimizedScene.Statistics.TotalVertices, Is.LessThan(_testScene.Statistics.TotalVertices));
            
            // Performance profile should result in significant polygon reduction
            float polyReduction = 1.0f - ((float)optimizedScene.Statistics.TotalPolygons / _testScene.Statistics.TotalPolygons);
            Assert.That(polyReduction, Is.GreaterThan(0.4f), "Performance profile should reduce polygons by at least 40%");
            
            // Verify optimization results are tracked
            Assert.That(optimizedScene.OptimizationResults, Is.Not.Null);
            Assert.That(optimizedScene.OptimizationResults.Count, Is.GreaterThan(0));
            
            // Find specific mesh optimization result
            var meshOptResult = FindOptimizationResultByType(optimizedScene.OptimizationResults, "Mesh Optimization");
            Assert.That(meshOptResult, Is.Not.Null, "Should include mesh optimization result");
            Assert.That(meshOptResult.ItemsOptimized, Is.GreaterThan(0), "Should have optimized some meshes");
        }
        
        [Test]
        public async Task OptimizeScene_WithQualityProfile_PreservesQuality()
        {
            // Act
            var optimizedScene = await _sceneOptimizer.OptimizeSceneAsync(_testScene, _qualitySettings);
            
            // Assert
            Assert.That(optimizedScene, Is.Not.Null);
            
            // Quality profile should result in modest polygon reduction
            float polyReduction = 1.0f - ((float)optimizedScene.Statistics.TotalPolygons / _testScene.Statistics.TotalPolygons);
            Assert.That(polyReduction, Is.LessThan(0.3f), "Quality profile should reduce polygons by less than 30%");
            
            // Materials should be preserved more
            Assert.That(optimizedScene.Materials.Count, Is.GreaterThanOrEqualTo(_testScene.Materials.Count / 2),
                "Quality profile should preserve most materials");
        }
        
        [Test]
        public async Task OptimizeScene_WithLODGeneration_CreatesLODLevels()
        {
            // Arrange
            var lodSettings = new SceneOptimizationSettings
            {
                EnableLODGeneration = true,
                LODLevels = 3,
                EnableMeshSimplification = false
            };
            
            // Act
            var optimizedScene = await _sceneOptimizer.OptimizeSceneAsync(_testScene, lodSettings);
            
            // Assert
            Assert.That(optimizedScene, Is.Not.Null);
            
            // Find LOD optimization result
            var lodOptResult = FindOptimizationResultByType(optimizedScene.OptimizationResults, "Mesh Optimization");
            Assert.That(lodOptResult, Is.Not.Null, "Should include LOD generation result");
            Assert.That(lodOptResult.ItemsOptimized, Is.GreaterThan(0), "Should have generated LODs");
            Assert.That(lodOptResult.Notes, Does.Contain("LOD generation"), "Notes should mention LOD generation");
        }
        
        [Test]
        public async Task OptimizeScene_CompareProfiles_ShowsExpectedDifferences()
        {
            // Act - optimize with both profiles
            var performanceOptimizedScene = await _sceneOptimizer.OptimizeSceneAsync(_testScene, _performanceSettings);
            var qualityOptimizedScene = await _sceneOptimizer.OptimizeSceneAsync(_testScene, _qualitySettings);
            
            // Assert
            Assert.That(performanceOptimizedScene.Statistics.TotalPolygons, 
                Is.LessThan(qualityOptimizedScene.Statistics.TotalPolygons), 
                "Performance profile should reduce polygons more than quality profile");
            
            Assert.That(qualityOptimizedScene.Statistics.TotalFileSize, 
                Is.GreaterThan(performanceOptimizedScene.Statistics.TotalFileSize),
                "Quality profile should result in larger file size than performance profile");
        }
        
        [Test]
        public async Task OptimizeScene_InvalidSettings_ThrowsArgumentException()
        {
            // Arrange - create invalid settings
            var invalidSettings = new SceneOptimizationSettings
            {
                EnableMeshSimplification = true,
                TargetPolygonCount = -100 // Invalid polygon count
            };
            
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => 
                await _sceneOptimizer.OptimizeSceneAsync(_testScene, invalidSettings));
        }
        
        [Test]
        public async Task OptimizeScene_WithCustomPipeline_ProducesExpectedResults()
        {
            // Arrange - create a custom pipeline settings
            var customSettings = new SceneOptimizationSettings
            {
                // Only enable transform optimization
                EnableMeshSimplification = false,
                EnableLODGeneration = false,
                EnableMaterialBatching = false,
                EnableTextureCompression = false,
                EnableHierarchyFlattening = false,
                EnableTransformOptimization = true
            };
            
            // Act
            var optimizedScene = await _sceneOptimizer.OptimizeSceneAsync(_testScene, customSettings);
            
            // Assert
            Assert.That(optimizedScene, Is.Not.Null);
            
            // Polygon count should remain unchanged
            Assert.That(optimizedScene.Statistics.TotalPolygons, Is.EqualTo(_testScene.Statistics.TotalPolygons));
            
            // Materials should remain unchanged
            Assert.That(optimizedScene.Materials.Count, Is.EqualTo(_testScene.Materials.Count));
            
            // Only transform optimization result should exist
            Assert.That(optimizedScene.OptimizationResults.Count, Is.EqualTo(1));
            Assert.That(optimizedScene.OptimizationResults[0].Type, Does.Contain("Transform"));
        }
        
        #region Helper Methods
        
        private USDScene CreateTestScene()
        {
            var scene = new USDScene
            {
                Name = "TestScene",
                FilePath = "test_scene.usd",
                ImportDate = DateTime.Now,
                Meshes = new List<Mesh>(),
                Materials = new List<Material>(),
                Textures = new List<Texture>(),
                RootNode = new Node
                {
                    Name = "Root",
                    Children = new List<Node>()
                }
            };
            
            // Add meshes with varying complexity
            for (int i = 0; i < 10; i++)
            {
                scene.Meshes.Add(new Mesh
                {
                    Name = $"Mesh_{i}",
                    PolygonCount = 1000 * (i + 1),
                    VertexCount = 2000 * (i + 1)
                });
            }
            
            // Add materials
            for (int i = 0; i < 5; i++)
            {
                var material = new Material
                {
                    Name = $"Material_{i}"
                };
                scene.Materials.Add(material);
            }
            
            // Add textures
            for (int i = 0; i < 8; i++)
            {
                scene.Textures.Add(new Texture
                {
                    Name = $"Texture_{i}",
                    Width = 1024,
                    Height = 1024,
                    Size = 1024 * 1024 * 4 // 4MB texture
                });
            }
            
            // Build scene hierarchy
            for (int i = 0; i < 3; i++)
            {
                var parentNode = new Node
                {
                    Name = $"Parent_{i}",
                    Children = new List<Node>()
                };
                
                for (int j = 0; j < 4; j++)
                {
                    var childNode = new Node
                    {
                        Name = $"Child_{i}_{j}",
                        Mesh = scene.Meshes[(i * 4 + j) % scene.Meshes.Count],
                        Material = scene.Materials[(i * 4 + j) % scene.Materials.Count],
                        Children = new List<Node>()
                    };
                    
                    // Add deeper nesting
                    for (int k = 0; k < 2; k++)
                    {
                        childNode.Children.Add(new Node
                        {
                            Name = $"GrandChild_{i}_{j}_{k}",
                            Children = new List<Node>()
                        });
                    }
                    
                    parentNode.Children.Add(childNode);
                }
                
                scene.RootNode.Children.Add(parentNode);
            }
            
            // Add some duplicate meshes for instancing
            for (int i = 0; i < 5; i++)
            {
                var instanceNode = new Node
                {
                    Name = $"Instance_{i}",
                    Mesh = scene.Meshes[0], // All use the same mesh
                    Material = scene.Materials[0], // All use the same material
                    Children = new List<Node>()
                };
                
                scene.RootNode.Children.Add(instanceNode);
            }
            
            // Calculate scene statistics
            scene.Statistics = new SceneStatistics
            {
                TotalNodes = CountNodes(scene.RootNode),
                TotalPolygons = scene.Meshes.Sum(m => m.PolygonCount),
                TotalVertices = scene.Meshes.Sum(m => m.VertexCount),
                TotalMaterials = scene.Materials.Count,
                TotalTextures = scene.Textures.Count,
                TotalFileSize = scene.Textures.Sum(t => t.Size) + (scene.Meshes.Sum(m => m.VertexCount) * 0.05f),
                NodeTypeCounts = new Dictionary<string, int>
                {
                    { "Mesh", scene.Meshes.Count },
                    { "Material", scene.Materials.Count },
                    { "Texture", scene.Textures.Count },
                    { "Transform", CountNodes(scene.RootNode) }
                }
            };
            
            return scene;
        }
        
        private SceneOptimizationSettings CreatePerformanceSettings()
        {
            return new SceneOptimizationSettings
            {
                EnableLODGeneration = true,
                LODLevels = 3,
                EnableMeshSimplification = true,
                TargetPolygonCount = 5000,
                EnableTextureCompression = true,
                EnableMaterialBatching = true,
                EnableShaderOptimization = true,
                EnableInstanceOptimization = true,
                SimilarityThreshold = 0.8f,
                EnableHierarchyFlattening = true,
                MaxHierarchyDepth = 2,
                EnableTransformOptimization = true
            };
        }
        
        private SceneOptimizationSettings CreateQualitySettings()
        {
            return new SceneOptimizationSettings
            {
                EnableLODGeneration = true,
                LODLevels = 2,
                EnableMeshSimplification = false,
                TargetPolygonCount = 10000,
                EnableTextureCompression = true,
                EnableMaterialBatching = false,
                EnableShaderOptimization = true,
                EnableInstanceOptimization = true,
                SimilarityThreshold = 0.4f,
                EnableHierarchyFlattening = false,
                MaxHierarchyDepth = 4,
                EnableTransformOptimization = true
            };
        }
        
        private int CountNodes(Node node)
        {
            if (node == null)
            {
                return 0;
            }
            
            int count = 1; // Count this node
            
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    count += CountNodes(child);
                }
            }
            
            return count;
        }
        
        private OptimizationResult FindOptimizationResultByType(List<OptimizationResult> results, string type)
        {
            return results?.Find(r => r.Type.Contains(type));
        }
        
        #endregion
    }
} 