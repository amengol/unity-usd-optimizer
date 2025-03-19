using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using USDOptimizer.Core.Models;
using USDOptimizer.Core.Extensions;

namespace USDOptimizer.Tests
{
    public class ExtensionTests
    {
        [Test]
        public void MeshExtensions_PolygonCount_ReturnsCorrectValue()
        {
            // Arrange
            var mesh = new USDOptimizer.Core.Models.Mesh
            {
                PolygonIndices = new List<int> { 0, 1, 2, 3, 4, 5 }
            };
            
            // Act
            int result = mesh.PolygonCount();
            
            // Assert
            Assert.AreEqual(2, result); // 6 indices = 2 triangles
        }
        
        [Test]
        public void MeshExtensions_VertexCount_ReturnsCorrectValue()
        {
            // Arrange
            var mesh = new USDOptimizer.Core.Models.Mesh
            {
                Vertices = new List<USDOptimizer.Core.Models.Vector3> { 
                    new USDOptimizer.Core.Models.Vector3(0, 0, 0), 
                    new USDOptimizer.Core.Models.Vector3(1, 1, 1), 
                    new USDOptimizer.Core.Models.Vector3(1, 0, 0) 
                }
            };
            
            // Act
            int result = mesh.VertexCount();
            
            // Assert
            Assert.AreEqual(3, result);
        }
        
        [Test]
        public void TextureExtensions_Size_ReturnsCorrectValue()
        {
            // Arrange
            var texture = new USDOptimizer.Core.Models.Texture
            {
                Width = 512,
                Height = 512,
                Format = "RGBA"
            };
            
            // Act
            long result = texture.Size();
            
            // Assert
            Assert.AreEqual(512 * 512 * 4, result); // RGBA = 4 bytes per pixel
        }
        
        [Test]
        public void TextureExtensions_Size_WithDifferentFormats()
        {
            // Arrange
            var textureRGB = new USDOptimizer.Core.Models.Texture { Width = 256, Height = 256, Format = "RGB" };
            var textureRGBA16 = new USDOptimizer.Core.Models.Texture { Width = 128, Height = 128, Format = "RGBA16" };
            var textureR8 = new USDOptimizer.Core.Models.Texture { Width = 64, Height = 64, Format = "R8" };
            
            // Act
            long sizeRGB = textureRGB.Size();
            long sizeRGBA16 = textureRGBA16.Size();
            long sizeR8 = textureR8.Size();
            
            // Assert
            Assert.AreEqual(256 * 256 * 3, sizeRGB);
            Assert.AreEqual(128 * 128 * 8, sizeRGBA16);
            Assert.AreEqual(64 * 64 * 1, sizeR8);
        }
    }
} 