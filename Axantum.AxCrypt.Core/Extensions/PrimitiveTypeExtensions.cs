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

        public static void SetProperty<T>(this object me, string name, T value)
        {
            PropertyInfo pi = me.GetType().GetProperty(name);
            if (!HasValueChanged<T>(me, pi, value ))
            {
                return;
            }
            pi.SetValue(me, value, null);
            MethodInfo mi = me.GetType().GetMethod("OnPropertyChanged", BindingFlags.NonPublic|BindingFlags.Instance, null, new Type[]{typeof(PropertyChangedEventArgs)}, null);
            mi.Invoke(me, new object[]{new PropertyChangedEventArgs(name)});
            return;
        }

        private static bool HasValueChanged<T>(object me, PropertyInfo pi, T value)
        {
            object o = pi.GetValue(me, null);
            if (o == null)
            {
                return value != null;
            }
            T oldValue = (T)o;
            return !oldValue.Equals(value);
        }

        public static object GetProperty(this object me, string name)
        {
            PropertyInfo pi = me.GetType().GetProperty(name);
            return pi.GetValue(me, null);
        }
    }
}