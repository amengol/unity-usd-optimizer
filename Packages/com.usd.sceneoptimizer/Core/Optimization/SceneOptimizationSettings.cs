using UnityEngine;
using System;

namespace USDOptimizer.Core.Optimization
{
    /// <summary>
    /// Defines settings for scene optimization
    /// </summary>
    [Serializable]
    public class SceneOptimizationSettings
    {
        // Mesh Optimization
        public bool EnableLODGeneration { get; set; } = true;
        public int LODLevels { get; set; } = 2;
        public bool EnableMeshSimplification { get; set; } = true;
        public int TargetPolygonCount { get; set; } = 8000;

        // Material Optimization
        public bool EnableTextureCompression { get; set; } = true;
        public bool EnableMaterialBatching { get; set; } = true;
        public bool EnableShaderOptimization { get; set; } = true;

        // Scene Optimization
        public bool EnableInstanceOptimization { get; set; } = true;
        public float SimilarityThreshold { get; set; } = 0.6f;
        public bool EnableHierarchyFlattening { get; set; } = true;
        public int MaxHierarchyDepth { get; set; } = 3;
        public bool EnableTransformOptimization { get; set; } = true;
    }
} 