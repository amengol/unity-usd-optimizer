using System;
using System.Collections.Generic;

namespace USDOptimizer.Core.Models
{
    public class USDScene
    {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public DateTime ImportDate { get; set; }
        public List<USDNode> Nodes { get; set; } = new List<USDNode>();
        public SceneStatistics Statistics { get; set; } = new SceneStatistics();
    }

    public class USDNode
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<USDNode> Children { get; set; } = new List<USDNode>();
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    public class SceneStatistics
    {
        public int TotalNodes { get; set; }
        public int TotalPolygons { get; set; }
        public int TotalVertices { get; set; }
        public int TotalMaterials { get; set; }
        public int TotalTextures { get; set; }
        public float TotalFileSize { get; set; }
        public Dictionary<string, int> NodeTypeCounts { get; set; } = new Dictionary<string, int>();
    }
} 