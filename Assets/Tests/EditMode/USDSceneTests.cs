using System.Collections.Generic;
using NUnit.Framework;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Tests.EditMode
{
    public class USDSceneTests
    {
        [Test]
        public void USDScene_Creation_Works()
        {
            // Create a new USDScene
            var scene = new USDScene();
            
            // Set some properties
            scene.Name = "TestScene";
            scene.FilePath = "test/path.usd";
            
            // Check properties were set correctly
            Assert.AreEqual("TestScene", scene.Name);
            Assert.AreEqual("test/path.usd", scene.FilePath);
        }
        
        [Test]
        public void USDNode_Properties_Work()
        {
            // Create a new USDNode
            var node = new USDNode();
            node.Name = "TestNode";
            
            // Test that the default collections are initialized
            Assert.NotNull(node.Children);
            Assert.NotNull(node.Properties);
            
            // Test adding a property
            node.Properties["TestProp"] = "TestValue";
            Assert.AreEqual("TestValue", node.Properties["TestProp"]);
        }
        
        [Test]
        public void USDScene_AddNode_Works()
        {
            // Create a scene and a node
            var scene = new USDScene();
            var node = new USDNode { Name = "Root" };
            
            // Add the node to the scene
            scene.Nodes = new List<USDNode> { node };
            
            // Check if the node was added correctly
            Assert.AreEqual(1, scene.Nodes.Count);
            Assert.AreEqual("Root", scene.Nodes[0].Name);
        }
    }
} 