using System.Threading.Tasks;
using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Analysis.Interfaces
{
    /// <summary>
    /// Interface for analyzing material data in USD scenes
    /// </summary>
    public interface IMaterialAnalyzer
    {
        /// <summary>
        /// Analyzes material data in the scene and returns metrics
        /// </summary>
        /// <param name="scene">The USD scene to analyze</param>
        /// <returns>Material analysis metrics</returns>
        Task<MaterialAnalysisMetrics> AnalyzeMaterialsAsync(Scene scene);

        /// <summary>
        /// Analyzes texture usage in materials
        /// </summary>
        /// <param name="material">The material to analyze</param>
        /// <returns>Texture usage metrics</returns>
        Task<TextureUsageMetrics> AnalyzeTextureUsageAsync(Material material);

        /// <summary>
        /// Detects redundant materials in the scene
        /// </summary>
        /// <param name="scene">The USD scene to analyze</param>
        /// <returns>List of redundant material groups</returns>
        Task<List<MaterialRedundancyGroup>> DetectMaterialRedundancyAsync(Scene scene);

        /// <summary>
        /// Analyzes shader complexity for a material
        /// </summary>
        /// <param name="material">The material to analyze</param>
        /// <returns>Shader complexity metrics</returns>
        Task<ShaderComplexityMetrics> AnalyzeShaderComplexityAsync(Material material);
    }
} 