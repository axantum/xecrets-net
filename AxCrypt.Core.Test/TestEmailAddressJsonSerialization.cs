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

using AxCrypt.Abstractions;
using AxCrypt.Core.UI;
using AxCrypt.Mono;

using NUnit.Framework;

using Xecrets.Net.Api.Implementation;
using Xecrets.Net.Core.IO;

using static AxCrypt.Abstractions.TypeResolve;

namespace Xecrets.Net.Core.Test
{
    [TestFixture]
    internal class TestEmailAddressJsonSerialization
    {
        [SetUp]
        public void Setup()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            options.Converters.Add(new EmailAddressSystemTextJsonConverter());

            TypeMap.Register.Singleton<IStringSerializer>(() => new SystemTextJsonStringSerializer(new JsonSourceGenerationContext(options)));
            TypeMap.Register.Singleton<IEmailParser>(() => new EmailParser());
        }

        private const string _addressJson = @"{
  ""address"": ""svante@axantum.com""
}";

        [Test]
        public void TestSerialization()
        {
            var serializer = New<IStringSerializer>();

            EmailAddress address = EmailAddress.Parse("svante@axantum.com");
            string json = serializer.Serialize(address);

            Assert.That(json, Is.EqualTo(_addressJson));

        }

        [Test]
        public void TestDeserialization()
        {
            var serializer = New<IStringSerializer>();

            EmailAddress address = EmailAddress.Parse("svante@axantum.com");
            EmailAddress deserialized = serializer.Deserialize<EmailAddress>(_addressJson);

            Assert.That(deserialized, Is.EqualTo(address));
        }
    }
}
