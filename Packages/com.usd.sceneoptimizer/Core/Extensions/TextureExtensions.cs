using USDOptimizer.Core.Models;

namespace USDOptimizer.Core.Extensions
{
    /// <summary>
    /// Extension methods for the Texture class
    /// </summary>
    public static class TextureExtensions
    {
        /// <summary>
        /// Gets the size of the texture in bytes
        /// </summary>
        public static long Size(this Texture texture)
        {
            if (texture == null)
                return 0;
                
            // Assume 4 bytes per pixel (RGBA)
            int bytesPerPixel = 4;
            
            // If the texture has a specific format, we could adjust bytesPerPixel here
            if (texture.Format == "RGB")
                bytesPerPixel = 3;
            else if (texture.Format == "RGBA16")
                bytesPerPixel = 8;
            else if (texture.Format == "R8")
                bytesPerPixel = 1;
                
            // Calculate size based on dimensions
            return (long)texture.Width * texture.Height * bytesPerPixel;
        }
    }
} 