#region Coypright and GPL License

/*
 * Xecrets Net Core - Copyright 2022, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Net Core, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets Net Core is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Net Core is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Net Core.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/ please go there for more information, suggestions and
 * contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Coypright and GPL License

using System.Text.Json;
using System.Text.Json.Serialization;

using AxCrypt.Core.Crypto.Asymmetric;

namespace Xecrets.Net.Core.Crypto.Asymmetric
{
    internal class BouncyCastlePublicKeyJsonConverter : JsonConverter<IAsymmetricPublicKey>
    {
        public override IAsymmetricPublicKey? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? pem = string.Empty;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new BouncyCastlePublicKey(pem ?? throw new JsonException("'pem' unexpectedly null."));
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    _ = reader.Read();
                    switch (propertyName)
                    {
                        case "pem":
                            pem = reader.GetString();
                            break;
                    }
                }
            }

            throw new JsonException("Missing EndObject in Json reader.");
        }

        public override void Write(Utf8JsonWriter writer, IAsymmetricPublicKey value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("pem", ((BouncyCastlePublicKey)value).ToString());
            writer.WriteEndObject();
        }
    }
}
