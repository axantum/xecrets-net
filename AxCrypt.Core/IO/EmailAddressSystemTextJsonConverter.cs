#region Coypright and GPL License

/*
 * Xecrets Net - Copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
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

using AxCrypt.Core.UI;

namespace Xecrets.Net.Core.IO
{
    internal class EmailAddressSystemTextJsonConverter : JsonConverter<EmailAddress>
    {
        /// <summary>
        /// Read a parsed email value.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns>The parsed email value.</returns>
        /// <exception cref="JsonException"></exception>
        /// <remarks>
        /// This got a bit complicated due to bug
        /// https://github.com/axantum/xecrets-net/issues/16 . Email addresses
        /// were initially incorrectly serialized as { email: { address: "value"
        /// }} but the correct form for interop with AxCrypt is { email: "value"
        /// }. Therefore we support both forms in deserialization, while now
        /// writing the correct form.
        /// </remarks>
        public override EmailAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            EmailAddress? emailAddress = null;
            if (reader.TokenType == JsonTokenType.String)
            {
                string? value = reader.GetString() ?? throw new JsonException("Expected an email string but got null.");
                emailAddress = EmailAddress.Parse(value);
                return emailAddress;
            }
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Expected JsonTokenType.StartObject or an email JsonTokenType.String value but got {reader.TokenType}.");
            }
            while (reader.Read())
            {

                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return emailAddress;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    _ = reader.Read();
                    switch (propertyName)
                    {
                        case "address":
                            string? value = reader.GetString() ?? throw new JsonException("Expected an email address string but got null.");
                            emailAddress = EmailAddress.Parse(value);
                            break;
                    }
                }
            }

            throw new JsonException("Missing EndObject in Json reader.");
        }

        public override void Write(Utf8JsonWriter writer, EmailAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Address);
        }
    }
}
