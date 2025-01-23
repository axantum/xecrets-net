#region Copyright and GPL License

/*
 * Xecrets Cli - Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/ please go there for more information, suggestions and
 * contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Copyright and GPL License

using System.Text.Json;
using System.Text.Json.Serialization;

using AxCrypt.Core.Crypto.Asymmetric;

namespace Xecrets.Net.Core.Crypto.Asymmetric;

internal class NetKeyPairJsonConverter : JsonConverter<IAsymmetricKeyPair>
{
    public override IAsymmetricKeyPair? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var context = JsonSourceGenerationContext.CreateJsonSerializerContext([new NetPublicKeyJsonConverter(), new NetPrivateKeyJsonConverter()]);

        NetPublicKey? publicKey = null;
        NetPrivateKey? privateKey = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new NetKeyPair(publicKey!, privateKey);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? propertyName = reader.GetString();
                _ = reader.Read();
                switch (propertyName)
                {
                    case "publickey":
                        publicKey = (NetPublicKey?)JsonSerializer.Deserialize(ref reader, typeof(IAsymmetricPublicKey), context);
                        break;
                    case "privatekey":
                        privateKey = (NetPrivateKey?)JsonSerializer.Deserialize(ref reader, typeof(IAsymmetricPrivateKey), context);
                        break;
                }
            }
        }

        throw new JsonException("Missing EndObject in Json reader.");
    }

    public override void Write(Utf8JsonWriter writer, IAsymmetricKeyPair value, JsonSerializerOptions options)
    {
        var context = JsonSourceGenerationContext.CreateJsonSerializerContext([new NetPublicKeyJsonConverter(), new NetPrivateKeyJsonConverter()]);

        writer.WriteStartObject();
        writer.WritePropertyName("publickey");
        JsonSerializer.Serialize(writer, value.PublicKey, typeof(IAsymmetricPublicKey), context);
        writer.WritePropertyName("privatekey");
        JsonSerializer.Serialize(writer, value.PrivateKey, typeof(IAsymmetricPrivateKey), context);
        writer.WriteEndObject();
    }
}
