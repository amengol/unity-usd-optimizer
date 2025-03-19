using System;

namespace USDOptimizer.Core.Models
{
    /// <summary>
    /// Represents a 4x4 matrix
    /// </summary>
    public struct Matrix4x4 : IEquatable<Matrix4x4>
    {
        public float m00, m01, m02, m03;
        public float m10, m11, m12, m13;
        public float m20, m21, m22, m23;
        public float m30, m31, m32, m33;
        
        /// <summary>
        /// Identity matrix
        /// </summary>
        public static Matrix4x4 identity => new Matrix4x4
        {
            m00 = 1f, m01 = 0f, m02 = 0f, m03 = 0f,
            m10 = 0f, m11 = 1f, m12 = 0f, m13 = 0f,
            m20 = 0f, m21 = 0f, m22 = 1f, m23 = 0f,
            m30 = 0f, m31 = 0f, m32 = 0f, m33 = 1f
        };
        
        /// <summary>
        /// Creates a translation matrix
        /// </summary>
        public static Matrix4x4 Translate(USDOptimizer.Core.Models.Vector3 vector)
        {
            Matrix4x4 result = identity;
            result.m03 = vector.x;
            result.m13 = vector.y;
            result.m23 = vector.z;
            return result;
        }
        
        /// <summary>
        /// Creates a scale matrix
        /// </summary>
        public static Matrix4x4 Scale(USDOptimizer.Core.Models.Vector3 vector)
        {
            Matrix4x4 result = identity;
            result.m00 = vector.x;
            result.m11 = vector.y;
            result.m22 = vector.z;
            return result;
        }
        
        public float this[int row, int column]
        {
            get
            {
                return row switch
                {
                    0 => column switch
                    {
                        0 => m00,
                        1 => m01,
                        2 => m02,
                        3 => m03,
                        _ => throw new IndexOutOfRangeException("Column index out of range")
                    },
                    1 => column switch
                    {
                        0 => m10,
                        1 => m11,
                        2 => m12,
                        3 => m13,
                        _ => throw new IndexOutOfRangeException("Column index out of range")
                    },
                    2 => column switch
                    {
                        0 => m20,
                        1 => m21,
                        2 => m22,
                        3 => m23,
                        _ => throw new IndexOutOfRangeException("Column index out of range")
                    },
                    3 => column switch
                    {
                        0 => m30,
                        1 => m31,
                        2 => m32,
                        3 => m33,
                        _ => throw new IndexOutOfRangeException("Column index out of range")
                    },
                    _ => throw new IndexOutOfRangeException("Row index out of range")
                };
            }
            set
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0: m00 = value; break;
                            case 1: m01 = value; break;
                            case 2: m02 = value; break;
                            case 3: m03 = value; break;
                            default: throw new IndexOutOfRangeException("Column index out of range");
                        }
                        break;
                    case 1:
                        switch (column)
                        {
                            case 0: m10 = value; break;
                            case 1: m11 = value; break;
                            case 2: m12 = value; break;
                            case 3: m13 = value; break;
                            default: throw new IndexOutOfRangeException("Column index out of range");
                        }
                        break;
                    case 2:
                        switch (column)
                        {
                            case 0: m20 = value; break;
                            case 1: m21 = value; break;
                            case 2: m22 = value; break;
                            case 3: m23 = value; break;
                            default: throw new IndexOutOfRangeException("Column index out of range");
                        }
                        break;
                    case 3:
                        switch (column)
                        {
                            case 0: m30 = value; break;
                            case 1: m31 = value; break;
                            case 2: m32 = value; break;
                            case 3: m33 = value; break;
                            default: throw new IndexOutOfRangeException("Column index out of range");
                        }
                        break;
                    default:
                        throw new IndexOutOfRangeException("Row index out of range");
                }
            }
        }
        
        /// <summary>
        /// Converts from UnityEngine.Matrix4x4
        /// </summary>
        public static implicit operator Matrix4x4(UnityEngine.Matrix4x4 matrix)
        {
            Matrix4x4 result = new Matrix4x4();
            
            result.m00 = matrix.m00;
            result.m01 = matrix.m01;
            result.m02 = matrix.m02;
            result.m03 = matrix.m03;
            
            result.m10 = matrix.m10;
            result.m11 = matrix.m11;
            result.m12 = matrix.m12;
            result.m13 = matrix.m13;
            
            result.m20 = matrix.m20;
            result.m21 = matrix.m21;
            result.m22 = matrix.m22;
            result.m23 = matrix.m23;
            
            result.m30 = matrix.m30;
            result.m31 = matrix.m31;
            result.m32 = matrix.m32;
            result.m33 = matrix.m33;
            
            return result;
        }
        
        /// <summary>
        /// Converts to UnityEngine.Matrix4x4
        /// </summary>
        public static implicit operator UnityEngine.Matrix4x4(Matrix4x4 matrix)
        {
            UnityEngine.Matrix4x4 result = new UnityEngine.Matrix4x4();
            
            result.m00 = matrix.m00;
            result.m01 = matrix.m01;
            result.m02 = matrix.m02;
            result.m03 = matrix.m03;
            
            result.m10 = matrix.m10;
            result.m11 = matrix.m11;
            result.m12 = matrix.m12;
            result.m13 = matrix.m13;
            
            result.m20 = matrix.m20;
            result.m21 = matrix.m21;
            result.m22 = matrix.m22;
            result.m23 = matrix.m23;
            
            result.m30 = matrix.m30;
            result.m31 = matrix.m31;
            result.m32 = matrix.m32;
            result.m33 = matrix.m33;
            
            return result;
        }
        
        public static bool operator ==(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return lhs.m00 == rhs.m00 && lhs.m01 == rhs.m01 && lhs.m02 == rhs.m02 && lhs.m03 == rhs.m03 &&
                   lhs.m10 == rhs.m10 && lhs.m11 == rhs.m11 && lhs.m12 == rhs.m12 && lhs.m13 == rhs.m13 &&
                   lhs.m20 == rhs.m20 && lhs.m21 == rhs.m21 && lhs.m22 == rhs.m22 && lhs.m23 == rhs.m23 &&
                   lhs.m30 == rhs.m30 && lhs.m31 == rhs.m31 && lhs.m32 == rhs.m32 && lhs.m33 == rhs.m33;
        }
        
        public static bool operator !=(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return !(lhs == rhs);
        }
        
        public override bool Equals(object obj)
        {
            return obj is Matrix4x4 other && Equals(other);
        }
        
        public bool Equals(Matrix4x4 other)
        {
            return m00 == other.m00 && m01 == other.m01 && m02 == other.m02 && m03 == other.m03 &&
                   m10 == other.m10 && m11 == other.m11 && m12 == other.m12 && m13 == other.m13 &&
                   m20 == other.m20 && m21 == other.m21 && m22 == other.m22 && m23 == other.m23 &&
                   m30 == other.m30 && m31 == other.m31 && m32 == other.m32 && m33 == other.m33;
        }
        
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(m00);
            hashCode.Add(m01);
            hashCode.Add(m02);
            hashCode.Add(m03);
            hashCode.Add(m10);
            hashCode.Add(m11);
            hashCode.Add(m12);
            hashCode.Add(m13);
            hashCode.Add(m20);
            hashCode.Add(m21);
            hashCode.Add(m22);
            hashCode.Add(m23);
            hashCode.Add(m30);
            hashCode.Add(m31);
            hashCode.Add(m32);
            hashCode.Add(m33);
            return hashCode.ToHashCode();
        }
    }
} 