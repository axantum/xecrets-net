#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
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

using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Notification;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Header;
using AxCrypt.Core.Ipc;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;

using Xecrets.Net.Core.IO;

namespace Xecrets.Net.Core
{
    [JsonSerializable(typeof(IAsymmetricKeyPair))]
    [JsonSerializable(typeof(IAsymmetricPublicKey))]
    [JsonSerializable(typeof(IAsymmetricPrivateKey))]
    [JsonSerializable(typeof(UserPublicKey))]
    [JsonSerializable(typeof(UserKeyPair))]
    [JsonSerializable(typeof(EmailAddress))]
    [JsonSerializable(typeof(Salt))]
    [JsonSerializable(typeof(KnownPublicKeys))]
    [JsonSerializable(typeof(ActiveFile))]
    [JsonSerializable(typeof(ActiveFileProperties))]
    [JsonSerializable(typeof(ActiveFileStatus))]
    [JsonSerializable(typeof(SymmetricKeyThumbprint))]
    [JsonSerializable(typeof(Recipients))]
    [JsonSerializable(typeof(UserAccounts))]
    [JsonSerializable(typeof(UserAccount))]
    [JsonSerializable(typeof(Offers))]
    [JsonSerializable(typeof(AccountStatus))]
    [JsonSerializable(typeof(KeyPair))]
    [JsonSerializable(typeof(CommandServiceEventArgs))]
    [JsonSerializable(typeof(FileSystemState))]
    [JsonSerializable(typeof(WatchedFolder))]
    [JsonSerializable(typeof(IdentityPublicTag))]
    [JsonSerializable(typeof(AxCryptVersion))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(NotificationApiModel))]
    public partial class JsonSourceGenerationContext : JsonSerializerContext
    {
        private static JsonSerializerOptions CreateJsonSerializerOptions(IEnumerable<JsonConverter> converters)
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            foreach (JsonConverter converter in converters)
            {
                options.Converters.Add(converter);
            }
            options.Converters.Add(new EmailAddressSystemTextJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter<AccountStatus>());
            options.Converters.Add(new JsonStringEnumConverter<SubscriptionLevel>());
            return options;
        }

        public static JsonSerializerContext CreateJsonSerializerContext(IEnumerable<JsonConverter> converters)
        {
            return new JsonSourceGenerationContext(CreateJsonSerializerOptions(converters));
        }
    }
}
