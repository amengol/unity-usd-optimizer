using System.Collections.Generic;
using UnityEngine;

namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Represents a node in the scene hierarchy
    /// </summary>
    public class Node
    {
        /// <summary>
        /// The name of the node
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The transform matrix of the node
        /// </summary>
        public Matrix4x4 Transform { get; set; } = Matrix4x4.identity;
        
        /// <summary>
        /// The mesh assigned to this node (can be null)
        /// </summary>
        public Mesh Mesh { get; set; }
        
        /// <summary>
        /// The material assigned to this node (can be null)
        /// </summary>
        public Material Material { get; set; }
        
        /// <summary>
        /// Whether this node is an instance of another node
        /// </summary>
        public bool IsInstance { get; set; }
        
        /// <summary>
        /// The name of the source node if this is an instance
        /// </summary>
        public string InstanceSource { get; set; }
        
        /// <summary>
        /// Children of this node
        /// </summary>
        public List<Node> Children { get; set; } = new List<Node>();
        
        /// <summary>
        /// Adds a range of children to this node
        /// </summary>
        public void AddRange(IEnumerable<Node> nodes)
        {
            Children.AddRange(nodes);
        }
    }
} 