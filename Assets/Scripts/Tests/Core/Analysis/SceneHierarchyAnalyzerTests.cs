using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using USDOptimizer.Core.Analysis.Implementations;
using USDOptimizer.Core.Analysis.Interfaces;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Tests.Core.Analysis
{
    [TestFixture]
    public class SceneHierarchyAnalyzerTests
    {
        private SceneHierarchyAnalyzer _analyzer;
        private USDScene _testScene;
        
        [SetUp]
        public void Setup()
        {
            _analyzer = new SceneHierarchyAnalyzer();
            _testScene = new USDScene { Name = "TestScene" };
            
            // Create a basic hierarchy for testing
            _testScene.Nodes = CreateTestHierarchy();
        }
        
        [Test]
        public async Task AnalyzeHierarchyAsync_ValidScene_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeHierarchyAsync(_testScene);
            
            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(7, metrics.TotalNodes);
            Assert.AreEqual(3, metrics.MaxHierarchyDepth); // Root -> Child -> GrandChild
            Assert.Greater(metrics.AverageChildrenPerNode, 0);
            Assert.AreEqual(2, metrics.MaxChildrenPerNode);
        }
        
        [Test]
        public async Task AnalyzeHierarchyAsync_EmptyScene_ReturnsZeroValues()
        {
            // Arrange
            var emptyScene = new USDScene { Name = "EmptyScene", Nodes = new List<USDNode>() };
            
            // Act
            var metrics = await _analyzer.AnalyzeHierarchyAsync(emptyScene);
            
            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(0, metrics.TotalNodes);
            Assert.AreEqual(0, metrics.MaxHierarchyDepth);
            Assert.AreEqual(0, metrics.EmptyNodeCount);
        }
        
        [Test]
        public async Task AnalyzeHierarchyAsync_NullNodesList_HandlesGracefully()
        {
            // Arrange
            var sceneWithNullNodes = new USDScene { Name = "NullNodesScene", Nodes = null };
            
            // Act
            var metrics = await _analyzer.AnalyzeHierarchyAsync(sceneWithNullNodes);
            
            // Assert
            Assert.NotNull(metrics);
            Assert.AreEqual(0, metrics.TotalNodes);
            Assert.AreEqual(0, metrics.MaxHierarchyDepth);
        }
        
        [Test]
        public void AnalyzeHierarchyAsync_NullScene_ThrowsArgumentNullException()
        {
            // Assert
            Assert.ThrowsAsync<System.ArgumentNullException>(async () => 
                await _analyzer.AnalyzeHierarchyAsync(null));
        }
        
        [Test]
        public async Task AnalyzeHierarchyAsync_ComplexHierarchy_CalculatesDepthCorrectly()
        {
            // Arrange
            var scene = new USDScene { Name = "DeepHierarchyScene" };
            scene.Nodes = CreateDeepHierarchy(6); // 6 levels deep
            
            // Act
            var metrics = await _analyzer.AnalyzeHierarchyAsync(scene);
            
            // Assert
            Assert.AreEqual(6, metrics.MaxHierarchyDepth);
            Assert.AreEqual(6, metrics.TotalNodes);
        }
        
        [Test]
        public async Task AnalyzeHierarchyAsync_SceneWithEmptyNodes_CountsEmptyNodesCorrectly()
        {
            // Arrange
            var scene = new USDScene { Name = "EmptyNodesScene" };
            scene.Nodes = new List<USDNode>
            {
                new USDNode { Name = "Root", Children = new List<USDNode>
                {
                    new USDNode { Name = "EmptyChild1" }, // Empty, no mesh or material
                    new USDNode { Name = "EmptyChild2" }, // Empty, no mesh or material
                    new USDNode { Name = "NonEmptyChild", Mesh = new USDOptimizer.Core.Models.Mesh { Name = "TestMesh" } }
                }}
            };
            
            // Act
            var metrics = await _analyzer.AnalyzeHierarchyAsync(scene);
            
            // Assert
            Assert.AreEqual(4, metrics.TotalNodes);
            Assert.AreEqual(2, metrics.EmptyNodeCount);
        }
        
        [Test]
        public async Task AnalyzeHierarchyAsync_SceneWithMixedNodeTypes_CountsNodeTypesCorrectly()
        {
            // Arrange
            var mesh = new USDOptimizer.Core.Models.Mesh { Name = "TestMesh" };
            var material = new USDOptimizer.Core.Models.Material { Name = "TestMaterial" };
            
            var scene = new USDScene { Name = "MixedTypesScene" };
            scene.Nodes = new List<USDNode>
            {
                new USDNode { Name = "Root", Children = new List<USDNode>
                {
                    new USDNode { Name = "MeshNode", Mesh = mesh },
                    new USDNode { Name = "MaterialNode", Material = material },
                    new USDNode { Name = "TransformNode" } // Just a transform node
                }}
            };
            
            // Act
            var metrics = await _analyzer.AnalyzeHierarchyAsync(scene);
            
            // Assert
            Assert.AreEqual(4, metrics.TotalNodes);
            Assert.IsTrue(metrics.NodeTypeCounts.ContainsKey("Mesh"));
            Assert.IsTrue(metrics.NodeTypeCounts.ContainsKey("Material"));
            Assert.IsTrue(metrics.NodeTypeCounts.ContainsKey("Transform"));
            Assert.AreEqual(1, metrics.NodeTypeCounts["Mesh"]);
            Assert.AreEqual(1, metrics.NodeTypeCounts["Material"]);
            Assert.AreEqual(2, metrics.NodeTypeCounts["Transform"]); // Root and TransformNode
        }
        
        private List<USDNode> CreateTestHierarchy()
        {
            // Create a simple scene hierarchy with 3 levels:
            // Root
            // |-- Child1
            // |   |-- GrandChild1
            // |   |-- GrandChild2
            // |-- Child2
            // |-- Child3
            
            var grandChild1 = new USDNode { Name = "GrandChild1" };
            var grandChild2 = new USDNode { Name = "GrandChild2" };
            
            var child1 = new USDNode
            {
                Name = "Child1",
                Children = new List<USDNode> { grandChild1, grandChild2 }
            };
            
            var child2 = new USDNode { Name = "Child2" };
            var child3 = new USDNode { Name = "Child3" };
            
            var root = new USDNode
            {
                Name = "Root",
                Children = new List<USDNode> { child1, child2, child3 }
            };
            
            return new List<USDNode> { root };
        }
        
        private List<USDNode> CreateDeepHierarchy(int depth)
        {
            // Create a linear hierarchy with specified depth
            USDNode currentNode = new USDNode { Name = $"Level_{depth}" };
            
            for (int i = depth - 1; i > 0; i--)
            {
                var parentNode = new USDNode
                {
                    Name = $"Level_{i}",
                    Children = new List<USDNode> { currentNode }
                };
                
                currentNode = parentNode;
            }
            
            return new List<USDNode> { currentNode };
        }
    }
} 