using System.Collections.Generic;

namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Contains metrics and analysis results for material data
    /// </summary>
    public class MaterialAnalysisMetrics
    {
        /// <summary>
        /// Total number of materials in the scene
        /// </summary>
        public int TotalMaterialCount { get; set; }

        /// <summary>
        /// Total number of unique textures used
        /// </summary>
        public int UniqueTextureCount { get; set; }

        /// <summary>
        /// Total memory usage of all textures
        /// </summary>
        public long TotalTextureMemoryBytes { get; set; }

        /// <summary>
        /// Dictionary mapping material names to their texture usage metrics
        /// </summary>
        public Dictionary<string, TextureUsageMetrics> MaterialTextureUsage { get; set; } = new Dictionary<string, TextureUsageMetrics>();

        /// <summary>
        /// Dictionary mapping material names to their shader complexity metrics
        /// </summary>
        public Dictionary<string, ShaderComplexityMetrics> MaterialShaderComplexity { get; set; } = new Dictionary<string, ShaderComplexityMetrics>();

        /// <summary>
        /// List of redundant material groups
        /// </summary>
        public List<MaterialRedundancyGroup> RedundantMaterialGroups { get; set; } = new List<MaterialRedundancyGroup>();

        /// <summary>
        /// List of materials with high shader complexity
        /// </summary>
        public List<string> HighComplexityMaterials { get; set; } = new List<string>();

        /// <summary>
        /// List of materials with excessive texture usage
        /// </summary>
        public List<string> HighTextureUsageMaterials { get; set; } = new List<string>();
    }

    /// <summary>
    /// Contains metrics related to texture usage in a material
    /// </summary>
    public class TextureUsageMetrics
    {
        /// <summary>
        /// Number of textures used in the material
        /// </summary>
        public int TextureCount { get; set; }

        /// <summary>
        /// Total memory usage of all textures
        /// </summary>
        public long TotalTextureMemoryBytes { get; set; }

        /// <summary>
        /// Dictionary mapping texture types to their counts
        /// </summary>
        public Dictionary<TextureType, int> TextureTypeCounts { get; set; } = new Dictionary<TextureType, int>();

        /// <summary>
        /// Dictionary mapping texture types to their memory usage
        /// </summary>
        public Dictionary<TextureType, long> TextureTypeMemoryBytes { get; set; } = new Dictionary<TextureType, long>();

        /// <summary>
        /// Whether the material has excessive texture usage
        /// </summary>
        public bool HasExcessiveTextureUsage { get; set; }

        /// <summary>
        /// List of texture usage issues found
        /// </summary>
        public List<string> TextureIssues { get; set; } = new List<string>();
    }

    /// <summary>
    /// Contains metrics related to shader complexity
    /// </summary>
    public class ShaderComplexityMetrics
    {
        /// <summary>
        /// Number of shader properties
        /// </summary>
        public int PropertyCount { get; set; }

        /// <summary>
        /// Number of texture samplers
        /// </summary>
        public int TextureSamplerCount { get; set; }

        /// <summary>
        /// Number of shader keywords
        /// </summary>
        public int KeywordCount { get; set; }

        /// <summary>
        /// Number of shader variants
        /// </summary>
        public int VariantCount { get; set; }

        /// <summary>
        /// Whether the shader has high complexity
        /// </summary>
        public bool HasHighComplexity { get; set; }

        /// <summary>
        /// List of shader complexity issues found
        /// </summary>
        public List<string> ComplexityIssues { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents a group of redundant materials
    /// </summary>
    public class MaterialRedundancyGroup
    {
        /// <summary>
        /// Name of the material group
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// List of material names in this group
        /// </summary>
        public List<string> MaterialNames { get; set; } = new List<string>();

        /// <summary>
        /// Similarity score between materials (0-1)
        /// </summary>
        public float SimilarityScore { get; set; }

        /// <summary>
        /// Potential memory savings if materials are merged
        /// </summary>
        public long PotentialMemorySavingsBytes { get; set; }

        /// <summary>
        /// Suggested material to keep (reference material)
        /// </summary>
        public string SuggestedReferenceMaterial { get; set; }
    }

    /// <summary>
    /// Types of textures that can be used in materials
    /// </summary>
    public enum TextureType
    {
        Albedo,
        Normal,
        Metallic,
        Roughness,
        Occlusion,
        Emission,
        Height,
        DetailAlbedo,
        DetailNormal,
        DetailMask,
        Custom
    }
} 