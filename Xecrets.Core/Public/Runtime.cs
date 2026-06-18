#region Coypright and GPL License

/*
 * Xecrets.Net - Copyright © 2022-2026, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets.Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets.Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets.Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
 */

#endregion Coypright and GPL License

using System.Diagnostics.CodeAnalysis;

using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Portable;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using AxCrypt.Mono.Cryptography;
using AxCrypt.Mono.Portable;

using Xecrets.Core.Implementation;
using Xecrets.Net.Api.Implementation;
using Xecrets.Net.Core;
using Xecrets.Net.Core.Crypto.Asymmetric;

using static AxCrypt.Abstractions.TypeResolve;

using IPlatform = AxCrypt.Core.Runtime.IPlatform;
using EmailParser = AxCrypt.Mono.EmailParser;

namespace Xecrets.Core.Public;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Runtime
{
    public static void Singleton<TResult>(Func<TResult> creator) => TypeMap.Register.Singleton(creator);

    public static void Singleton<TResult>(Func<TResult> creator, Action postAction) => TypeMap.Register.Singleton(creator, postAction);

    public static void Transient<TArgument, TResult>(Func<TArgument, TResult> creator) => TypeMap.Register.New(creator);

    public static void Transient<TResult>(Func<TResult> creator) => TypeMap.Register.New(creator);

    public static void Register()
    {
        Singleton(() => TimeProvider.System);
        Singleton(() => new RuntimeEnvironment(".axx"));
        Singleton<IRuntimeEnvironment>(New<RuntimeEnvironment>);
        Singleton<IPlatform>(New<RuntimeEnvironment>);
        Singleton<ILogging>(() => new NoLogging());
        Singleton<IReport>(() => new NoReport());
        Singleton<INow>(() => new Now(New<TimeProvider>()));
        Singleton<ISettingsStore>(() => new TransientSettingsStore());
        Singleton<IEmailParser>(() => new EmailParser());
        Singleton<IPortableFactory>(() => new PortableFactory());
        Transient(PortableFactory.RandomNumberGenerator);
        Transient<Aes>(() => new AesWrapper(System.Security.Cryptography.Aes.Create()));
        Transient(PortableFactory.CryptoStream);
        Transient(PortableFactory.SHA1Managed);
        Transient(PortableFactory.SHA256Managed);
        Transient(PortableFactory.HMACSHA512);
        Transient(PortableFactory.AxCryptHMACSHA1);
        Singleton(() => new UserSettingsVersion());
        Singleton(() => new UserSettings(New<ISettingsStore>(), New<IterationCalculator>()));
        Singleton<IRandomGenerator>(() => new RandomGenerator());
        Singleton<IAsymmetricFactory>(() => new NetAsymmetricFactory());
        Singleton<IProtectedData>(() => new NoProtectedData());
        Singleton(() => new CryptoFactory([]));
        Transient(() => new AxCryptFactory());
        Transient(() => new AxCryptFile());
        Transient<int, Salt>(size => new Salt(size));
        Transient(() => new IterationCalculator());
        Transient<ISystemCryptoPolicy>(() => new ProCryptoPolicy());
        Singleton<IStringSerializer>(() => new SystemTextJsonStringSerializer(JsonSourceGenerationContext.CreateJsonSerializerContext(New<IAsymmetricFactory>().GetConverters())));
    }
}
