using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Tests.PlayMode
{
    public class USDScenePlayModeTests
    {
        private USDScene _testScene;
        
        [SetUp]
        public void Setup()
        {
            _testScene = new USDScene();
            _testScene.Name = "TestPlayModeScene";
            _testScene.FilePath = "test/playmode/path.usd";
            
            // Create a simple node hierarchy
            var rootNode = new USDNode { Name = "Root" };
            var childNode = new USDNode { Name = "Child" };
            rootNode.Children.Add(childNode);
            
            _testScene.Nodes = new List<USDNode> { rootNode };
        }
        
        [Test]
        public void USDScene_RootNode_ReturnsFirstNode()
        {
            // Test the RootNode property implementation
            Assert.NotNull(_testScene.RootNode);
            Assert.AreEqual("Root", _testScene.RootNode.Name);
        }
        
        [UnityTest]
        public IEnumerator USDScene_RootNode_EmptyList_CreatesNewNode()
        {
            // Create a scene with an empty node list
            var scene = new USDScene();
            scene.Nodes = new List<USDNode>();
            
            // Access the RootNode property, which should create a new node
            var rootNode = scene.RootNode;
            
            // Wait one frame
            yield return null;
            
            // Check that a node was created
            Assert.NotNull(rootNode);
            Assert.AreEqual(1, scene.Nodes.Count);
        }
    }
} 