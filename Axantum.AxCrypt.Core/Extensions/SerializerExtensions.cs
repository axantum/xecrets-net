using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class SerializerExtensions
    {
        public static T Deserialize<T>(this IStringSerializer serializer, IDataStore serializedStore)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }
            if (serializedStore == null)
            {
                throw new ArgumentNullException("serializedStore");
            }

            using (StreamReader reader = new StreamReader(serializedStore.OpenRead(), Encoding.UTF8))
            {
                string serialized = reader.ReadToEnd();
                return serializer.Deserialize<T>(serialized);
            }
        }
    }
}