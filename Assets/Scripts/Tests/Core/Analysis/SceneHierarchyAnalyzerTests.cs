using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using USDOptimizer.Core.Analysis.Implementations;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Tests.Core.Analysis
{
    [TestFixture]
    public class SceneHierarchyAnalyzerTests
    {
        private SceneHierarchyAnalyzer _analyzer;
        private Scene _testScene;
        private Node _rootNode;

        [SetUp]
        public void Setup()
        {
            _analyzer = new SceneHierarchyAnalyzer();
            _rootNode = CreateTestHierarchy();
            _testScene = new Scene { Name = "TestScene", RootNode = _rootNode };
        }

        [Test]
        public async Task AnalyzeHierarchyAsync_ValidHierarchy_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeHierarchyAsync(_testScene);

            // Assert
            Assert.That(metrics.TotalNodeCount, Is.EqualTo(5));
            Assert.That(metrics.LeafNodeCount, Is.EqualTo(3));
            Assert.That(metrics.IntermediateNodeCount, Is.EqualTo(2));
            Assert.That(metrics.NodeChildCounts["Root"], Is.EqualTo(2));
            Assert.That(metrics.NodeChildCounts["Child1"], Is.EqualTo(2));
        }

        [Test]
        public async Task AnalyzeTransformsAsync_ValidHierarchy_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeTransformsAsync(_testScene);

            // Assert
            Assert.That(metrics.NonIdentityTransformCount, Is.EqualTo(2));
            Assert.That(metrics.NonUniformScaleCount, Is.EqualTo(1));
            Assert.That(metrics.NonZeroRotationCount, Is.EqualTo(1));
        }

        [Test]
        public async Task DetectInstancesAsync_ValidHierarchy_ReturnsCorrectMetrics()
        {
            // Arrange
            var instanceNode = CreateInstanceNode();
            _rootNode.Children.Add(instanceNode);

            // Act
            var metrics = await _analyzer.DetectInstancesAsync(_testScene);

            // Assert
            Assert.That(metrics.TotalInstanceCount, Is.EqualTo(1));
            Assert.That(metrics.UniquePrototypeCount, Is.EqualTo(1));
            Assert.That(metrics.PrototypeInstanceCounts["Prototype1"], Is.EqualTo(1));
        }

        [Test]
        public async Task AnalyzeHierarchyComplexityAsync_ValidHierarchy_ReturnsCorrectMetrics()
        {
            // Act
            var metrics = await _analyzer.AnalyzeHierarchyComplexityAsync(_testScene);

            // Assert
            Assert.That(metrics.MaxDepth, Is.EqualTo(2));
            Assert.That(metrics.HighChildCountNodeCount, Is.EqualTo(0));
        }

        [Test]
        public async Task AnalyzeHierarchyComplexityAsync_HighChildCount_DetectsHighComplexity()
        {
            // Arrange
            var highChildNode = CreateNodeWithManyChildren();
            _rootNode.Children.Add(highChildNode);

            // Act
            var metrics = await _analyzer.AnalyzeHierarchyComplexityAsync(_testScene);

            // Assert
            Assert.That(metrics.HighChildCountNodeCount, Is.EqualTo(1));
            Assert.That(metrics.ComplexityOptimizationOpportunities.Count, Is.EqualTo(1));
            Assert.That(metrics.ComplexityOptimizationOpportunities[0].Type, Is.EqualTo(OptimizationType.HighChildCount));
        }

        private Node CreateTestHierarchy()
        {
            var root = new Node
            {
                Name = "Root",
                Transform = Matrix4x4.identity,
                Children = new List<Node>
                {
                    new Node
                    {
                        Name = "Child1",
                        Transform = Matrix4x4.identity,
                        Children = new List<Node>
                        {
                            new Node { Name = "Leaf1", Transform = Matrix4x4.identity },
                            new Node { Name = "Leaf2", Transform = Matrix4x4.identity }
                        }
                    },
                    new Node
                    {
                        Name = "Child2",
                        Transform = Matrix4x4.TRS(
                            new Vector3(1, 0, 0),
                            Quaternion.Euler(0, 45, 0),
                            new Vector3(1, 2, 1)
                        ),
                        Children = new List<Node>
                        {
                            new Node { Name = "Leaf3", Transform = Matrix4x4.identity }
                        }
                    }
                }
            };

            return root;
        }

        private Node CreateInstanceNode()
        {
            return new Node
            {
                Name = "Instance1",
                Transform = Matrix4x4.identity,
                IsInstance = true,
                PrototypeName = "Prototype1"
            };
        }

        private Node CreateNodeWithManyChildren()
        {
            var node = new Node
            {
                Name = "HighChildNode",
                Transform = Matrix4x4.identity,
                Children = new List<Node>()
            };

            // Add 101 children to exceed the threshold
            for (int i = 0; i < 101; i++)
            {
                node.Children.Add(new Node
                {
                    Name = $"Child{i}",
                    Transform = Matrix4x4.identity
                });
            }

            return node;
        }
    }
} 