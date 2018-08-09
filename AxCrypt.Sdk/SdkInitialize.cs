using System;
using System.Collections.Generic;
using System.Text;
using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.UI;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Sdk
{
    public static class SdkInitialize
    {
        public static void Initialize()
        {
            InitializeTypeFactories();
        }

        private static void InitializeTypeFactories()
        {
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new BouncyCastleAsymmetricFactory());
            TypeMap.Register.Singleton<IEmailParser>(() => new RegexEmailParser());
            TypeMap.Register.Singleton<ISettingsStore>(() => new TransientSettingsStore());
            TypeMap.Register.Singleton<UserSettings>(() => new UserSettings(New<ISettingsStore>(), New<IterationCalculator>()));
            TypeMap.Register.Singleton<INow>(() => new Now());
        }
    }
}
