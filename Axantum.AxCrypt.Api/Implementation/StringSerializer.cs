#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Api.Implementation
{
    public class StringSerializer : IStringSerializer
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public StringSerializer(IEnumerable<CustomSerializer> converters)
        {
            _serializerSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                Converters = converters.ToArray(),
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            };
        }

        public StringSerializer()
            : this(new CustomSerializer[0])
        {
        }

        public T Deserialize<T>(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized, _serializerSettings);
        }

        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _serializerSettings);
        }

        public T Deserialize<T>(Stream stream) where T : class, new()
        {
            using (JsonReader reader = new JsonTextReader(new StreamReader(stream)))
            {
                JsonSerializer serializer = JsonSerializer.Create(_serializerSettings);
                T value = serializer.Deserialize<T>(reader) ?? new T();
                return value;
            }
        }

        public void Serialize<T>(T value, Stream stream)
        {
            using (TextWriter writer = new StreamWriter(stream))
            {
                JsonSerializer serializer = JsonSerializer.Create(_serializerSettings);
                serializer.Serialize(writer, value);
            }
        }
    }
}