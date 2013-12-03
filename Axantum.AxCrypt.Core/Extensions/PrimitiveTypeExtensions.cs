using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class PrimitiveTypeExtensions
    {
        public static byte[] GetLittleEndianBytes(this long value)
        {
            if (OS.Current.IsLittleEndian)
            {
                return BitConverter.GetBytes(value);
            }

            byte[] bytes = new byte[sizeof(long)];

            for (int i = 0; value != 0 && i < bytes.Length; ++i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }

        public static byte[] GetLittleEndianBytes(this int value)
        {
            if (OS.Current.IsLittleEndian)
            {
                return BitConverter.GetBytes(value);
            }

            byte[] bytes = new byte[sizeof(int)];

            for (int i = 0; value != 0 && i < bytes.Length; ++i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }

        public static byte[] GetBigEndianBytes(this long value)
        {
            if (!OS.Current.IsLittleEndian)
            {
                return BitConverter.GetBytes(value);
            }

            byte[] bytes = new byte[sizeof(long)];

            for (int i = bytes.Length - 1; value != 0 && i >= 0; --i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }

        public static T Fallback<T>(this T value, T fallback) where T : IEquatable<T>
        {
            return !value.Equals(default(T)) ? value : fallback;
        }
    }
}