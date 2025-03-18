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
        private GameObject _testObject;
        
        [SetUp]
        public void Setup()
        {
            // Create mock analyzers
            _mockMeshAnalyzer = Substitute.For<IMeshAnalyzer>();
            _mockMaterialAnalyzer = Substitute.For<IMaterialAnalyzer>();
            _mockHierarchyAnalyzer = Substitute.For<ISceneHierarchyAnalyzer>();
            
            // Setup test data
            SetupMockAnalyzers();
            
            // Create SceneAnalyzer with mock components
            _sceneAnalyzer = new SceneAnalyzer(_mockMeshAnalyzer, _mockMaterialAnalyzer, _mockHierarchyAnalyzer);
            
            // Create test GameObject
            _testObject = new GameObject("TestObject");
            var childObject = new GameObject("ChildObject");
            childObject.transform.parent = _testObject.transform;
            
            // Add components to test object
            var meshFilter = _testObject.AddComponent<MeshFilter>();
            meshFilter.mesh = new UnityEngine.Mesh();
            meshFilter.mesh.vertices = new Vector3[100];
            meshFilter.mesh.triangles = new int[300]; // 100 triangles
            
            var renderer = _testObject.AddComponent<MeshRenderer>();
            renderer.material = new UnityEngine.Material(Shader.Find("Standard"));
        }
        
        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_testObject);
        }
        
        [Test]
        public void AnalyzeScene_ReturnsValidResults()
        {
            // Act
            var results = _sceneAnalyzer.AnalyzeScene();
            
            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.TotalPolygons, Is.EqualTo(1000));
            Assert.That(results.TotalVertices, Is.EqualTo(2000));
            Assert.That(results.TotalMaterials, Is.EqualTo(10));
            Assert.That(results.TotalTextures, Is.EqualTo(5));
            Assert.That(results.Recommendations, Is.Not.Null);
            Assert.That(results.Recommendations.Count, Is.GreaterThan(0));
        }
        
        [Test]
        public void AnalyzeGameObject_ReturnsValidResults()
        {
            // Act
            var results = _sceneAnalyzer.AnalyzeGameObject(_testObject);
            
            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.GameObjectName, Is.EqualTo("TestObject"));
            Assert.That(results.PolygonCount, Is.EqualTo(1000));
            Assert.That(results.VertexCount, Is.EqualTo(2000));
            Assert.That(results.MaterialCount, Is.EqualTo(10));
            Assert.That(results.Recommendations, Is.Not.Null);
        }
        
        [Test]
        public void AnalyzeGameObject_NullGameObject_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _sceneAnalyzer.AnalyzeGameObject(null));
        }
        
        [Test]
        public void AnalyzeScene_GeneratesAppropriateRecommendations()
        {
            // Act
            var results = _sceneAnalyzer.AnalyzeScene();
            
            // Assert
            var highPolyRecommendation = results.Recommendations.Find(r => r.Title.Contains("High Polygon Count"));
            Assert.That(highPolyRecommendation, Is.Not.Null, "Should include high polygon count recommendation");
            Assert.That(highPolyRecommendation.Priority, Is.EqualTo(OptimizationPriority.High));
            
            var textureRecommendation = results.Recommendations.Find(r => r.Title.Contains("High Texture Usage"));
            Assert.That(textureRecommendation, Is.Not.Null, "Should include texture usage recommendation");
        }
        
        [Test]
        public void AnalyzeScene_IntegratesAllAnalyzers()
        {
            // Act
            _sceneAnalyzer.AnalyzeScene();
            
            // Assert
            _mockMeshAnalyzer.Received(1).AnalyzeMeshesAsync(Arg.Any<Scene>());
            _mockMaterialAnalyzer.Received(1).AnalyzeMaterialsAsync(Arg.Any<Scene>());
            _mockHierarchyAnalyzer.Received(1).AnalyzeHierarchyAsync(Arg.Any<Scene>());
        }
        
        [Test]
        public void AnalyzeGameObject_IntegratesRelevantAnalyzers()
        {
            // Act
            _sceneAnalyzer.AnalyzeGameObject(_testObject);
            
            // Assert
            _mockMeshAnalyzer.Received(1).AnalyzeMeshesAsync(Arg.Any<Scene>());
            _mockMaterialAnalyzer.Received(1).AnalyzeMaterialsAsync(Arg.Any<Scene>());
        }
        
        #region Helper Methods
        
        private void SetupMockAnalyzers()
        {
            // Setup mesh analyzer mock
            var meshMetrics = new MeshAnalysisMetrics
            {
                TotalPolygonCount = 1000,
                TotalVertexCount = 2000,
                HighPolyMeshes = new List<string> { "HighPolyMesh1", "HighPolyMesh2" },
                InefficientUVMappings = new List<string> { "InefficientMesh1" }
            };
            _mockMeshAnalyzer.AnalyzeMeshesAsync(Arg.Any<Scene>()).Returns(Task.FromResult(meshMetrics));
            
            // Setup material analyzer mock
            var materialMetrics = new MaterialAnalysisMetrics
            {
                TotalMaterialCount = 10,
                TotalTextureMemoryBytes = 10485760, // 10MB
                MaterialTextureUsage = new Dictionary<string, TextureUsageMetrics>
                {
                    { "Material1", new TextureUsageMetrics { TextureCount = 3 } },
                    { "Material2", new TextureUsageMetrics { TextureCount = 2 } }
                },
                HighTextureUsageMaterials = new List<string> { "Material1" },
                ComplexShaderMaterials = new List<string> { "Material2" }
            };
            _mockMaterialAnalyzer.AnalyzeMaterialsAsync(Arg.Any<Scene>()).Returns(Task.FromResult(materialMetrics));
            
            // Setup hierarchy analyzer mock
            var hierarchyMetrics = new SceneHierarchyMetrics
            {
                TotalNodeCount = 50,
                LeafNodeCount = 30,
                IntermediateNodeCount = 20,
                NodeDepths = new Dictionary<string, int>
                {
                    { "Root", 0 },
                    { "Level1", 1 },
                    { "Level2", 2 },
                    { "DeepNode", 15 }
                },
                NodeChildCounts = new Dictionary<string, int>
                {
                    { "Root", 3 },
                    { "Level1", 5 }
                },
                PotentialInstanceOpportunities = new List<InstanceOpportunity>
                {
                    new InstanceOpportunity { NodeName = "RepeatableObject", Count = 5 }
                }
            };
            _mockHierarchyAnalyzer.AnalyzeHierarchyAsync(Arg.Any<Scene>()).Returns(Task.FromResult(hierarchyMetrics));
        }
        
        #endregion
    }
} 