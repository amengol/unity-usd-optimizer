using System.Collections.Generic;

namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Represents a material with shader, properties, and textures
    /// </summary>
    public class Material
    {
        /// <summary>
        /// The name of the material
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The name of the shader used by this material
        /// </summary>
        public string ShaderName { get; set; }
        
        /// <summary>
        /// Material properties (colors, floats, etc.)
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Textures used by this material, keyed by property name
        /// </summary>
        public Dictionary<string, Texture> Textures { get; set; } = new Dictionary<string, Texture>();
    }
    
    /// <summary>
    /// Represents a texture
    /// </summary>
    public class Texture
    {
        /// <summary>
        /// The name of the texture
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The width of the texture in pixels
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// The height of the texture in pixels
        /// </summary>
        public int Height { get; set; }
        
        /// <summary>
        /// The format of the texture
        /// </summary>
        public string Format { get; set; }
        
        /// <summary>
        /// Whether the texture has mipmaps
        /// </summary>
        public bool MipMaps { get; set; }
        
        /// <summary>
        /// The compression format of the texture
        /// </summary>
        public string Compression { get; set; }
    }
} 