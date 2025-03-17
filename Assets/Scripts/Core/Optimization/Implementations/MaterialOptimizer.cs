using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Optimization.Interfaces;

namespace USDOptimizer.Core.Optimization.Implementations
{
    /// <summary>
    /// Implementation of IMaterialOptimizer for material optimization operations
    /// </summary>
    public class MaterialOptimizer : IMaterialOptimizer
    {
        private const float DefaultSimilarityThreshold = 0.8f;
        private const float DefaultCompressionQuality = 0.7f;
        private const float DefaultTargetComplexity = 0.5f;

        public async Task<Material> CompressTexturesAsync(Material material, float compressionQuality)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (compressionQuality < 0 || compressionQuality > 1)
                throw new ArgumentException("Compression quality must be between 0 and 1");

            var optimizedMaterial = new Material
            {
                Name = $"{material.Name}_Compressed",
                Shader = material.Shader,
                Properties = new Dictionary<string, object>(material.Properties)
            };

            await Task.Run(() =>
            {
                foreach (var property in material.Properties)
                {
                    if (property.Value is Texture texture)
                    {
                        // Calculate new dimensions based on compression quality
                        var newWidth = (int)(texture.Width * compressionQuality);
                        var newHeight = (int)(texture.Height * compressionQuality);

                        // Create compressed texture
                        var compressedTexture = new Texture
                        {
                            Name = $"{texture.Name}_Compressed",
                            Width = newWidth,
                            Height = newHeight,
                            Format = DetermineCompressedFormat(texture.Format, compressionQuality)
                        };

                        optimizedMaterial.Properties[property.Key] = compressedTexture;
                    }
                }
            });

            return optimizedMaterial;
        }

        public async Task<Material> BatchMaterialsAsync(Material[] materials)
        {
            if (materials == null || materials.Length == 0)
                throw new ArgumentException("No materials provided for batching");

            if (materials.Length == 1)
                return materials[0];

            var batchedMaterial = new Material
            {
                Name = "BatchedMaterial",
                Shader = materials[0].Shader, // Use shader from first material
                Properties = new Dictionary<string, object>()
            };

            await Task.Run(() =>
            {
                // Combine all unique properties
                var allProperties = materials.SelectMany(m => m.Properties.Keys).Distinct();

                foreach (var property in allProperties)
                {
                    var values = materials.Select(m => m.Properties.ContainsKey(property) ? m.Properties[property] : null)
                                        .Where(v => v != null)
                                        .Distinct()
                                        .ToList();

                    if (values.Count == 1)
                    {
                        // If all materials have the same value, use it
                        batchedMaterial.Properties[property] = values[0];
                    }
                    else if (values[0] is Texture)
                    {
                        // If it's a texture, create an atlas or use the largest texture
                        var textures = values.Cast<Texture>().ToList();
                        var largestTexture = textures.OrderByDescending(t => t.Width * t.Height).First();
                        batchedMaterial.Properties[property] = largestTexture;
                    }
                    else
                    {
                        // For other properties, use a default value or the most common value
                        batchedMaterial.Properties[property] = values[0];
                    }
                }
            });

            return batchedMaterial;
        }

        public async Task<Material> OptimizeShaderAsync(Material material, float targetComplexity)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (targetComplexity < 0 || targetComplexity > 1)
                throw new ArgumentException("Target complexity must be between 0 and 1");

            var optimizedMaterial = new Material
            {
                Name = $"{material.Name}_Optimized",
                Shader = material.Shader,
                Properties = new Dictionary<string, object>()
            };

            await Task.Run(() =>
            {
                // Sort properties by complexity impact
                var properties = material.Properties.OrderByDescending(p => GetPropertyComplexity(p.Value));

                // Keep only the most important properties based on target complexity
                var propertyCount = (int)(properties.Count() * targetComplexity);
                foreach (var property in properties.Take(propertyCount))
                {
                    optimizedMaterial.Properties[property.Key] = property.Value;
                }
            });

            return optimizedMaterial;
        }

        public async Task<Material[]> MergeSimilarMaterialsAsync(Material[] materials, float similarityThreshold)
        {
            if (materials == null || materials.Length == 0)
                throw new ArgumentException("No materials provided for merging");

            if (similarityThreshold < 0 || similarityThreshold > 1)
                throw new ArgumentException("Similarity threshold must be between 0 and 1");

            var mergedMaterials = new List<Material>();
            var processedMaterials = new HashSet<Material>();

            await Task.Run(() =>
            {
                foreach (var material in materials)
                {
                    if (processedMaterials.Contains(material))
                        continue;

                    var similarGroup = materials.Where(m => !processedMaterials.Contains(m) &&
                                                         CalculateMaterialSimilarity(material, m) >= similarityThreshold)
                                              .ToList();

                    if (similarGroup.Count > 1)
                    {
                        // Merge similar materials
                        var mergedMaterial = new Material
                        {
                            Name = $"Merged_{material.Name}",
                            Shader = material.Shader,
                            Properties = new Dictionary<string, object>()
                        };

                        // Combine properties from all similar materials
                        var allProperties = similarGroup.SelectMany(m => m.Properties.Keys).Distinct();
                        foreach (var property in allProperties)
                        {
                            var values = similarGroup.Select(m => m.Properties.ContainsKey(property) ? m.Properties[property] : null)
                                                   .Where(v => v != null)
                                                   .Distinct()
                                                   .ToList();

                            if (values.Count == 1)
                            {
                                mergedMaterial.Properties[property] = values[0];
                            }
                            else if (values[0] is Texture)
                            {
                                var textures = values.Cast<Texture>().ToList();
                                var largestTexture = textures.OrderByDescending(t => t.Width * t.Height).First();
                                mergedMaterial.Properties[property] = largestTexture;
                            }
                            else
                            {
                                mergedMaterial.Properties[property] = values[0];
                            }
                        }

                        mergedMaterials.Add(mergedMaterial);
                        foreach (var m in similarGroup)
                        {
                            processedMaterials.Add(m);
                        }
                    }
                    else
                    {
                        mergedMaterials.Add(material);
                        processedMaterials.Add(material);
                    }
                }
            });

            return mergedMaterials.ToArray();
        }

        private string DetermineCompressedFormat(string originalFormat, float compressionQuality)
        {
            // This is a simplified version. In a real implementation, you would consider
            // the original format, platform requirements, and quality settings
            if (compressionQuality > 0.8f)
                return "RGBA32";
            else if (compressionQuality > 0.5f)
                return "RGBA16";
            else
                return "RGBA8";
        }

        private float GetPropertyComplexity(object propertyValue)
        {
            // This is a simplified version. In a real implementation, you would consider
            // the actual complexity of the property based on its type and usage
            if (propertyValue is Texture texture)
                return texture.Width * texture.Height / (1024f * 1024f); // Size in MB
            else if (propertyValue is Vector4)
                return 1.0f;
            else if (propertyValue is Vector3)
                return 0.75f;
            else if (propertyValue is Vector2)
                return 0.5f;
            else if (propertyValue is float)
                return 0.25f;
            else
                return 0.1f;
        }

        private float CalculateMaterialSimilarity(Material material1, Material material2)
        {
            if (material1.Shader != material2.Shader)
                return 0;

            var commonProperties = material1.Properties.Keys.Intersect(material2.Properties.Keys);
            if (!commonProperties.Any())
                return 0;

            var totalSimilarity = 0f;
            var propertyCount = 0;

            foreach (var property in commonProperties)
            {
                var value1 = material1.Properties[property];
                var value2 = material2.Properties[property];

                if (value1.GetType() == value2.GetType())
                {
                    if (value1 is Texture t1 && value2 is Texture t2)
                    {
                        // Compare texture properties
                        totalSimilarity += CalculateTextureSimilarity(t1, t2);
                    }
                    else if (value1 is Vector4 v1 && value2 is Vector4 v2)
                    {
                        // Compare vector values
                        totalSimilarity += CalculateVectorSimilarity(v1, v2);
                    }
                    else if (value1.Equals(value2))
                    {
                        totalSimilarity += 1;
                    }
                }
                propertyCount++;
            }

            return propertyCount > 0 ? totalSimilarity / propertyCount : 0;
        }

        private float CalculateTextureSimilarity(Texture t1, Texture t2)
        {
            // This is a simplified version. In a real implementation, you would compare
            // more texture properties and possibly analyze the actual texture data
            if (t1.Format != t2.Format)
                return 0;

            var sizeRatio = Math.Min(t1.Width * t1.Height, t2.Width * t2.Height) /
                           (float)Math.Max(t1.Width * t1.Height, t2.Width * t2.Height);

            return sizeRatio;
        }

        private float CalculateVectorSimilarity(Vector4 v1, Vector4 v2)
        {
            var diff = v1 - v2;
            var maxDiff = Math.Max(Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y)),
                                 Math.Max(Math.Abs(diff.Z), Math.Abs(diff.W)));
            return Math.Max(0, 1 - maxDiff);
        }
    }
} 