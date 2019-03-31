﻿using System;
using UnityEngine;

namespace LiteNetLib.Utils
{
    public static class NetDataWriterExtension
    {
        public static void PutValue<TType>(this NetDataWriter writer, TType value)
        {
            #region Generic Values
            if (value is bool)
            {
                writer.Put((bool)(object)value);
                return;
            }

            if (value is bool[])
            {
                writer.PutArray((bool[])(object)value);
                return;
            }

            if (value is byte)
            {
                writer.Put((byte)(object)value);
                return;
            }

            if (value is char)
            {
                writer.Put((char)(object)value);
                return;
            }

            if (value is double)
            {
                writer.Put((double)(object)value);
                return;
            }

            if (value is double[])
            {
                writer.PutArray((double[])(object)value);
                return;
            }

            if (value is float)
            {
                writer.Put((float)(object)value);
                return;
            }

            if (value is float[])
            {
                writer.PutArray((float[])(object)value);
                return;
            }

            if (value is int)
            {
                writer.Put((int)(object)value);
                return;
            }

            if (value is int[])
            {
                writer.PutArray((int[])(object)value);
                return;
            }

            if (value is long)
            {
                writer.Put((long)(object)value);
                return;
            }

            if (value is long[])
            {
                writer.PutArray((long[])(object)value);
                return;
            }

            if (value is sbyte)
            {
                writer.Put((sbyte)(object)value);
                return;
            }

            if (value is short)
            {
                writer.Put((short)(object)value);
                return;
            }

            if (value is short[])
            {
                writer.PutArray((short[])(object)value);
                return;
            }

            if (typeof(TType) == typeof(string))
            {
                writer.Put((string)(object)value);
                return;
            }

            if (value is uint)
            {
                writer.Put((uint)(object)value);
                return;
            }

            if (value is uint[])
            {
                writer.PutArray((uint[])(object)value);
                return;
            }

            if (value is ulong)
            {
                writer.Put((ulong)(object)value);
                return;
            }

            if (value is ulong[])
            {
                writer.PutArray((ulong[])(object)value);
                return;
            }

            if (value is ushort)
            {
                writer.Put((ushort)(object)value);
                return;
            }

            if (value is ushort[])
            {
                writer.PutArray((ushort[])(object)value);
                return;
            }
            #endregion

            #region Unity Values
            if (value is Color)
            {
                writer.Put((Color)(object)value);
                return;
            }

            if (value is Quaternion)
            {
                writer.Put((Quaternion)(object)value);
                return;
            }

            if (value is Vector2)
            {
                writer.Put((Vector2)(object)value);
                return;
            }

            if (value is Vector2Int)
            {
                writer.Put((Vector2Int)(object)value);
                return;
            }

            if (value is Vector3)
            {
                writer.Put((Vector3)(object)value);
                return;
            }

            if (value is Vector3Int)
            {
                writer.Put((Vector3Int)(object)value);
                return;
            }

            if (value is Vector4)
            {
                writer.Put((Vector4)(object)value);
                return;
            }
            #endregion

            if (value is INetSerializable)
            {
                ((INetSerializable)value).Serialize(writer);
                return;
            }

            throw new ArgumentException("NetDataReader cannot write type " + value.GetType().Name);
        }

        public static void Put(this NetDataWriter writer, Color value)
        {
            var r = (short)(value.r * 100f);
            var g = (short)(value.g * 100f);
            var b = (short)(value.b * 100f);
            var a = (short)(value.a * 100f);
            writer.Put(r);
            writer.Put(g);
            writer.Put(b);
            writer.Put(a);
        }

        public static void Put(this NetDataWriter writer, Quaternion value)
        {
            writer.Put(value.eulerAngles.x);
            writer.Put(value.eulerAngles.y);
            writer.Put(value.eulerAngles.z);
        }

        public static void Put(this NetDataWriter writer, Vector2 value)
        {
            writer.Put(value.x);
            writer.Put(value.y);
        }

        public static void Put(this NetDataWriter writer, Vector2Int value)
        {
            writer.Put(value.x);
            writer.Put(value.y);
        }

        public static void Put(this NetDataWriter writer, Vector3 value)
        {
            writer.Put(value.x);
            writer.Put(value.y);
            writer.Put(value.z);
        }

        public static void Put(this NetDataWriter writer, Vector3Int value)
        {
            writer.Put(value.x);
            writer.Put(value.y);
            writer.Put(value.z);
        }

        public static void Put(this NetDataWriter writer, Vector4 value)
        {
            writer.Put(value.x);
            writer.Put(value.y);
            writer.Put(value.z);
            writer.Put(value.w);
        }

        #region Packed Unsigned Int (Credit: https://sqlite.org/src4/doc/trunk/www/varint.wiki)
        public static void PutPackedUShort(this NetDataWriter writer, ushort value)
        {
            PutPackedULong(writer, value);
        }

        public static void PutPackedUInt(this NetDataWriter writer, uint value)
        {
            PutPackedULong(writer, value);
        }

        public static void PutPackedULong(this NetDataWriter writer, ulong value)
        {
            if (value <= 240)
            {
                writer.Put((byte)value);
                return;
            }
            if (value <= 2287)
            {
                writer.Put((byte)((value - 240) / 256 + 241));
                writer.Put((byte)((value - 240) % 256));
                return;
            }
            if (value <= 67823)
            {
                writer.Put((byte)249);
                writer.Put((byte)((value - 2288) / 256));
                writer.Put((byte)((value - 2288) % 256));
                return;
            }
            if (value <= 16777215)
            {
                writer.Put((byte)250);
                writer.Put((byte)(value & 0xFF));
                writer.Put((byte)((value >> 8) & 0xFF));
                writer.Put((byte)((value >> 16) & 0xFF));
                return;
            }
            if (value <= 4294967295)
            {
                writer.Put((byte)251);
                writer.Put((byte)(value & 0xFF));
                writer.Put((byte)((value >> 8) & 0xFF));
                writer.Put((byte)((value >> 16) & 0xFF));
                writer.Put((byte)((value >> 24) & 0xFF));
                return;
            }
            if (value <= 1099511627775)
            {
                writer.Put((byte)252);
                writer.Put((byte)(value & 0xFF));
                writer.Put((byte)((value >> 8) & 0xFF));
                writer.Put((byte)((value >> 16) & 0xFF));
                writer.Put((byte)((value >> 24) & 0xFF));
                writer.Put((byte)((value >> 32) & 0xFF));
                return;
            }
            if (value <= 281474976710655)
            {
                writer.Put((byte)253);
                writer.Put((byte)(value & 0xFF));
                writer.Put((byte)((value >> 8) & 0xFF));
                writer.Put((byte)((value >> 16) & 0xFF));
                writer.Put((byte)((value >> 24) & 0xFF));
                writer.Put((byte)((value >> 32) & 0xFF));
                writer.Put((byte)((value >> 40) & 0xFF));
                return;
            }
            if (value <= 72057594037927935)
            {
                writer.Put((byte)254);
                writer.Put((byte)(value & 0xFF));
                writer.Put((byte)((value >> 8) & 0xFF));
                writer.Put((byte)((value >> 16) & 0xFF));
                writer.Put((byte)((value >> 24) & 0xFF));
                writer.Put((byte)((value >> 32) & 0xFF));
                writer.Put((byte)((value >> 40) & 0xFF));
                writer.Put((byte)((value >> 48) & 0xFF));
                return;
            }
            // all others
            writer.Put((byte)255);
            writer.Put((byte)(value & 0xFF));
            writer.Put((byte)((value >> 8) & 0xFF));
            writer.Put((byte)((value >> 16) & 0xFF));
            writer.Put((byte)((value >> 24) & 0xFF));
            writer.Put((byte)((value >> 32) & 0xFF));
            writer.Put((byte)((value >> 40) & 0xFF));
            writer.Put((byte)((value >> 48) & 0xFF));
            writer.Put((byte)((value >> 56) & 0xFF));
        }
        #endregion
    }
}
