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

using AxCrypt.Api.Model;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Header;
using AxCrypt.Core.Ipc;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;

using Xecrets.Net.Core.IO;

using static AxCrypt.Abstractions.TypeResolve;

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
    internal partial class JsonSourceGenerationContext : JsonSerializerContext
    {
        private static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var asymmetricFactory = New<IAsymmetricFactory>();
            asymmetricFactory.GetConverters().ToList().ForEach(c => options.Converters.Add(c));

            options.Converters.Add(new EmailAddressSystemTextJsonConverter());

            return options;
        }

        public static JsonSerializerContext CreateJsonSerializerContext()
        {
            return new JsonSourceGenerationContext(CreateJsonSerializerOptions());
        }

    }
}
