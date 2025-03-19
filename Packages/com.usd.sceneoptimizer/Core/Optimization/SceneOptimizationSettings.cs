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
        public bool EnableMeshSimplification { get; set; } = true;
        public int TargetPolygonCount { get; set; } = 5000;
        
        // LOD Settings
        public bool EnableLODGeneration { get; set; } = true;
        public int LODLevels { get; set; } = 3;
        
        // Material Optimization
        public bool EnableMaterialBatching { get; set; } = true;
        public bool EnableShaderOptimization { get; set; } = true;
        
        // Texture Optimization
        public bool EnableTextureCompression { get; set; } = true;
        
        // Scene Optimization
        public bool EnableInstanceOptimization { get; set; } = true;
        public float SimilarityThreshold { get; set; } = 0.7f;
        
        // Hierarchy Optimization
        public bool EnableHierarchyFlattening { get; set; } = true;
        public int MaxHierarchyDepth { get; set; } = 5;
        
        // Transform Optimization
        public bool EnableTransformOptimization { get; set; } = true;
        
        // Instance Optimization (from errors)
        public bool OptimizeInstances { get; set; } = true;
        public float InstanceSimilarityThreshold { get; set; } = 0.8f;
        
        // Hierarchy Flattening (from errors)
        public bool FlattenHierarchy { get; set; } = true;
        public int MaxFlattenDepth { get; set; } = 4;
        
        public SceneOptimizationSettings()
        {
            // Default constructor with default values
        }
        
        public SceneOptimizationSettings Clone()
        {
            return new SceneOptimizationSettings
            {
                EnableMeshSimplification = this.EnableMeshSimplification,
                TargetPolygonCount = this.TargetPolygonCount,
                EnableLODGeneration = this.EnableLODGeneration,
                LODLevels = this.LODLevels,
                EnableMaterialBatching = this.EnableMaterialBatching,
                EnableShaderOptimization = this.EnableShaderOptimization,
                EnableTextureCompression = this.EnableTextureCompression,
                EnableInstanceOptimization = this.EnableInstanceOptimization,
                SimilarityThreshold = this.SimilarityThreshold,
                EnableHierarchyFlattening = this.EnableHierarchyFlattening,
                MaxHierarchyDepth = this.MaxHierarchyDepth,
                EnableTransformOptimization = this.EnableTransformOptimization,
                // New properties
                OptimizeInstances = this.OptimizeInstances,
                InstanceSimilarityThreshold = this.InstanceSimilarityThreshold,
                FlattenHierarchy = this.FlattenHierarchy,
                MaxFlattenDepth = this.MaxFlattenDepth
            };
        }
        
        public void ValidateSettings()
        {
            // Validate and fix any invalid settings
            if (TargetPolygonCount <= 0)
            {
                TargetPolygonCount = 1000;
            }
            
            if (LODLevels <= 0)
            {
                LODLevels = 1;
            }
            else if (LODLevels > 5)
            {
                LODLevels = 5;
            }
            
            if (SimilarityThreshold < 0f)
            {
                SimilarityThreshold = 0f;
            }
            else if (SimilarityThreshold > 1f)
            {
                SimilarityThreshold = 1f;
            }
            
            if (MaxHierarchyDepth <= 0)
            {
                MaxHierarchyDepth = 1;
            }
            
            // Validate new properties
            if (InstanceSimilarityThreshold < 0f)
            {
                InstanceSimilarityThreshold = 0f;
            }
            else if (InstanceSimilarityThreshold > 1f)
            {
                InstanceSimilarityThreshold = 1f;
            }
            
            if (MaxFlattenDepth <= 0)
            {
                MaxFlattenDepth = 1;
            }
        }
    }
} 