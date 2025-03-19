using System;
using System.Runtime.CompilerServices;

namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Represents a Vector with 3 components
    /// </summary>
    public struct Vector3 : IEquatable<Vector3>
    {
        /// <summary>
        /// X component
        /// </summary>
        public float x;
        
        /// <summary>
        /// Y component
        /// </summary>
        public float y;
        
        /// <summary>
        /// Z component
        /// </summary>
        public float z;
        
        /// <summary>
        /// Creates a new Vector3
        /// </summary>
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        /// <summary>
        /// Converts from UnityEngine.Vector3
        /// </summary>
        public static implicit operator Vector3(UnityEngine.Vector3 vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }
        
        /// <summary>
        /// Converts to UnityEngine.Vector3
        /// </summary>
        public static implicit operator UnityEngine.Vector3(Vector3 vector)
        {
            return new UnityEngine.Vector3(vector.x, vector.y, vector.z);
        }
        
        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }
        
        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return !(lhs == rhs);
        }
        
        public override bool Equals(object obj)
        {
            return obj is Vector3 other && Equals(other);
        }
        
        public bool Equals(Vector3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
        
        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
    
    /// <summary>
    /// Represents a Vector with 2 components
    /// </summary>
    public struct Vector2 : IEquatable<Vector2>
    {
        /// <summary>
        /// X component
        /// </summary>
        public float x;
        
        /// <summary>
        /// Y component
        /// </summary>
        public float y;
        
        /// <summary>
        /// Creates a new Vector2
        /// </summary>
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        
        /// <summary>
        /// Converts from UnityEngine.Vector2
        /// </summary>
        public static implicit operator Vector2(UnityEngine.Vector2 vector)
        {
            return new Vector2(vector.x, vector.y);
        }
        
        /// <summary>
        /// Converts to UnityEngine.Vector2
        /// </summary>
        public static implicit operator UnityEngine.Vector2(Vector2 vector)
        {
            return new UnityEngine.Vector2(vector.x, vector.y);
        }
        
        public static bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }
        
        public static bool operator !=(Vector2 lhs, Vector2 rhs)
        {
            return !(lhs == rhs);
        }
        
        public override bool Equals(object obj)
        {
            return obj is Vector2 other && Equals(other);
        }
        
        public bool Equals(Vector2 other)
        {
            return x == other.x && y == other.y;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
        
        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
} 