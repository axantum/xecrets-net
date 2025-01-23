#region Coypright and GPL License

/*
 * Xecrets Net - Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/ please go there for more information, suggestions and
 * contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Coypright and GPL License

using System.Text.Json;
using System.Text.Json.Serialization;

using AxCrypt.Abstractions;

namespace Xecrets.Net.Api.Implementation
{
    public class SystemTextJsonStringSerializer : IStringSerializer
    {
        private readonly JsonSerializerContext _context;

        public SystemTextJsonStringSerializer(JsonSerializerContext context)
        {
            _context = context;
        }
        public T? Deserialize<T>(string serialized)
        {
            if (serialized.Length == 0)
            {
                return default;
            }
            return (T?)JsonSerializer.Deserialize(serialized, typeof(T), _context);
        }

        public T? Deserialize<T>(Stream stream) where T : class, new()
        {
            return (T?)JsonSerializer.Deserialize(stream, typeof(T), _context);
        }

        public string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, typeof(T), _context);
        }

        public void Serialize<T>(T value, Stream stream)
        {
            JsonSerializer.Serialize(stream, value, typeof(T), _context);
        }
    }
}
