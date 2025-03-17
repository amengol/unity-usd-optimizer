using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using USDOptimizer.Core.Analysis.Interfaces;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Analysis.Implementations
{
    /// <summary>
    /// Implementation of IMaterialAnalyzer for analyzing material data in USD scenes
    /// </summary>
    public class MaterialAnalyzer : IMaterialAnalyzer
    {
        private const int HIGH_TEXTURE_COUNT_THRESHOLD = 8;
        private const long HIGH_TEXTURE_MEMORY_THRESHOLD = 100 * 1024 * 1024; // 100MB
        private const int HIGH_SHADER_COMPLEXITY_THRESHOLD = 20;
        private const float MATERIAL_SIMILARITY_THRESHOLD = 0.95f;

        public async Task<MaterialAnalysisMetrics> AnalyzeMaterialsAsync(Scene scene)
        {
            var metrics = new MaterialAnalysisMetrics
            {
                TotalMaterialCount = scene.Materials.Count
            };

            // Analyze each material
            foreach (var material in scene.Materials)
            {
                // Analyze texture usage
                var textureMetrics = await AnalyzeTextureUsageAsync(material);
                metrics.MaterialTextureUsage[material.Name] = textureMetrics;

                // Analyze shader complexity
                var shaderMetrics = await AnalyzeShaderComplexityAsync(material);
                metrics.MaterialShaderComplexity[material.Name] = shaderMetrics;

                // Update total texture memory
                metrics.TotalTextureMemoryBytes += textureMetrics.TotalTextureMemoryBytes;

                // Check for high texture usage
                if (textureMetrics.HasExcessiveTextureUsage)
                {
                    metrics.HighTextureUsageMaterials.Add(material.Name);
                }

                // Check for high shader complexity
                if (shaderMetrics.HasHighComplexity)
                {
                    metrics.HighComplexityMaterials.Add(material.Name);
                }
            }

            // Update unique texture count
            metrics.UniqueTextureCount = scene.Textures.Count;

            // Detect redundant materials
            metrics.RedundantMaterialGroups = await DetectMaterialRedundancyAsync(scene);

            return metrics;
        }

        public async Task<TextureUsageMetrics> AnalyzeTextureUsageAsync(Material material)
        {
            var metrics = new TextureUsageMetrics();

            // Count textures by type
            foreach (var texture in material.Textures)
            {
                var textureType = DetermineTextureType(texture);
                
                if (!metrics.TextureTypeCounts.ContainsKey(textureType))
                {
                    metrics.TextureTypeCounts[textureType] = 0;
                    metrics.TextureTypeMemoryBytes[textureType] = 0;
                }

                metrics.TextureTypeCounts[textureType]++;
                metrics.TextureTypeMemoryBytes[textureType] += CalculateTextureMemory(texture);
            }

            // Calculate total texture count and memory
            metrics.TextureCount = metrics.TextureTypeCounts.Values.Sum();
            metrics.TotalTextureMemoryBytes = metrics.TextureTypeMemoryBytes.Values.Sum();

            // Check for excessive texture usage
            metrics.HasExcessiveTextureUsage = 
                metrics.TextureCount > HIGH_TEXTURE_COUNT_THRESHOLD || 
                metrics.TotalTextureMemoryBytes > HIGH_TEXTURE_MEMORY_THRESHOLD;

            // Collect texture issues
            if (metrics.TextureCount > HIGH_TEXTURE_COUNT_THRESHOLD)
            {
                metrics.TextureIssues.Add($"Material uses {metrics.TextureCount} textures, exceeding threshold of {HIGH_TEXTURE_COUNT_THRESHOLD}");
            }
            if (metrics.TotalTextureMemoryBytes > HIGH_TEXTURE_MEMORY_THRESHOLD)
            {
                metrics.TextureIssues.Add($"Material uses {metrics.TotalTextureMemoryBytes / (1024 * 1024)}MB of texture memory, exceeding threshold of {HIGH_TEXTURE_MEMORY_THRESHOLD / (1024 * 1024)}MB");
            }

            return metrics;
        }

        public async Task<List<MaterialRedundancyGroup>> DetectMaterialRedundancyAsync(Scene scene)
        {
            var groups = new List<MaterialRedundancyGroup>();
            var processedMaterials = new HashSet<string>();

            foreach (var material1 in scene.Materials)
            {
                if (processedMaterials.Contains(material1.Name))
                {
                    continue;
                }

                var similarMaterials = new List<string>();
                var totalMemorySavings = 0L;

                foreach (var material2 in scene.Materials)
                {
                    if (material1.Name == material2.Name || processedMaterials.Contains(material2.Name))
                    {
                        continue;
                    }

                    var similarity = CalculateMaterialSimilarity(material1, material2);
                    if (similarity >= MATERIAL_SIMILARITY_THRESHOLD)
                    {
                        similarMaterials.Add(material2.Name);
                        processedMaterials.Add(material2.Name);
                        totalMemorySavings += CalculateMaterialMemory(material2);
                    }
                }

                if (similarMaterials.Count > 0)
                {
                    groups.Add(new MaterialRedundancyGroup
                    {
                        GroupName = $"RedundantGroup_{material1.Name}",
                        MaterialNames = new List<string> { material1.Name }.Concat(similarMaterials).ToList(),
                        SimilarityScore = MATERIAL_SIMILARITY_THRESHOLD,
                        PotentialMemorySavingsBytes = totalMemorySavings,
                        SuggestedReferenceMaterial = material1.Name
                    });

                    processedMaterials.Add(material1.Name);
                }
            }

            return groups;
        }

        public async Task<ShaderComplexityMetrics> AnalyzeShaderComplexityAsync(Material material)
        {
            var metrics = new ShaderComplexityMetrics();

            // Count shader properties
            metrics.PropertyCount = material.Properties.Count;

            // Count texture samplers
            metrics.TextureSamplerCount = material.Textures.Count;

            // Count shader keywords
            metrics.KeywordCount = material.ShaderKeywords.Count;

            // Count shader variants (estimated based on keywords)
            metrics.VariantCount = (int)Math.Pow(2, material.ShaderKeywords.Count);

            // Check for high complexity
            metrics.HasHighComplexity = 
                metrics.PropertyCount > HIGH_SHADER_COMPLEXITY_THRESHOLD ||
                metrics.TextureSamplerCount > HIGH_TEXTURE_COUNT_THRESHOLD ||
                metrics.KeywordCount > 5;

            // Collect complexity issues
            if (metrics.PropertyCount > HIGH_SHADER_COMPLEXITY_THRESHOLD)
            {
                metrics.ComplexityIssues.Add($"Shader has {metrics.PropertyCount} properties, exceeding threshold of {HIGH_SHADER_COMPLEXITY_THRESHOLD}");
            }
            if (metrics.TextureSamplerCount > HIGH_TEXTURE_COUNT_THRESHOLD)
            {
                metrics.ComplexityIssues.Add($"Shader uses {metrics.TextureSamplerCount} texture samplers, exceeding threshold of {HIGH_TEXTURE_COUNT_THRESHOLD}");
            }
            if (metrics.KeywordCount > 5)
            {
                metrics.ComplexityIssues.Add($"Shader has {metrics.KeywordCount} keywords, which may lead to many shader variants");
            }

            return metrics;
        }

        private TextureType DetermineTextureType(Texture texture)
        {
            // This is a simplified implementation. In a real system, you would need to
            // analyze the texture name, usage, and material properties to determine the type
            if (texture.Name.Contains("albedo", StringComparison.OrdinalIgnoreCase))
                return TextureType.Albedo;
            if (texture.Name.Contains("normal", StringComparison.OrdinalIgnoreCase))
                return TextureType.Normal;
            if (texture.Name.Contains("metallic", StringComparison.OrdinalIgnoreCase))
                return TextureType.Metallic;
            if (texture.Name.Contains("roughness", StringComparison.OrdinalIgnoreCase))
                return TextureType.Roughness;
            if (texture.Name.Contains("occlusion", StringComparison.OrdinalIgnoreCase))
                return TextureType.Occlusion;
            if (texture.Name.Contains("emission", StringComparison.OrdinalIgnoreCase))
                return TextureType.Emission;
            if (texture.Name.Contains("height", StringComparison.OrdinalIgnoreCase))
                return TextureType.Height;
            if (texture.Name.Contains("detail", StringComparison.OrdinalIgnoreCase))
            {
                if (texture.Name.Contains("albedo", StringComparison.OrdinalIgnoreCase))
                    return TextureType.DetailAlbedo;
                if (texture.Name.Contains("normal", StringComparison.OrdinalIgnoreCase))
                    return TextureType.DetailNormal;
                if (texture.Name.Contains("mask", StringComparison.OrdinalIgnoreCase))
                    return TextureType.DetailMask;
            }
            return TextureType.Custom;
        }

        private long CalculateTextureMemory(Texture texture)
        {
            // This is a simplified implementation. In a real system, you would need to
            // consider texture format, compression, and mipmap levels
            return texture.Width * texture.Height * 4; // Assuming RGBA format
        }

        private float CalculateMaterialSimilarity(Material material1, Material material2)
        {
            // This is a simplified implementation. In a real system, you would need to
            // compare all material properties, textures, and shader settings
            var similarity = 0f;
            var totalProperties = 0;

            // Compare textures
            foreach (var texture1 in material1.Textures)
            {
                totalProperties++;
                if (material2.Textures.Any(t => t.Name == texture1.Name))
                {
                    similarity += 1;
                }
            }

            // Compare shader properties
            foreach (var prop1 in material1.Properties)
            {
                totalProperties++;
                if (material2.Properties.ContainsKey(prop1.Key) && 
                    material2.Properties[prop1.Key].Equals(prop1.Value))
                {
                    similarity += 1;
                }
            }

            return totalProperties > 0 ? similarity / totalProperties : 0;
        }

        private long CalculateMaterialMemory(Material material)
        {
            // This is a simplified implementation. In a real system, you would need to
            // consider all material data including shader data
            return material.Textures.Sum(t => CalculateTextureMemory(t));
        }
    }
} 