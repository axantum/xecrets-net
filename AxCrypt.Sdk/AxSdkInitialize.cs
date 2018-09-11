using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Algorithm;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Mono;
using Axantum.AxCrypt.Mono.Portable;
using AxCrypt.Sdk.Internal;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Sdk
{
    public static class AxSdkInitialize
    {
        public static void Initialize()
        {
            InitializeTypeFactories();
        }

        private static void InitializeTypeFactories()
        {
            RuntimeEnvironment.RegisterTypeFactories();

            IEnumerable<Assembly> assemblies = LoadFromFiles(GetExecutingDirectory().GetFiles("*.dll"));
            Resolve.RegisterTypeFactories(assemblies);

            TypeMap.Register.Singleton<IEmailParser>(() => new RegexEmailParser());
            TypeMap.Register.Singleton<ISettingsStore>(() => new TransientSettingsStore());
            TypeMap.Register.Singleton<INow>(() => new Now());
            TypeMap.Register.Singleton<IInternetState>(() => new AlwaysOnInternetState());

            TypeMap.Register.New<RandomNumberGenerator>(() => PortableFactory.RandomNumberGenerator());
            TypeMap.Register.New<AxCryptHMACSHA1>(() => PortableFactory.AxCryptHMACSHA1());
            TypeMap.Register.New<HMACSHA512>(() => PortableFactory.HMACSHA512());
            TypeMap.Register.New<Aes>(() => new Axantum.AxCrypt.Mono.Cryptography.AesWrapper(new System.Security.Cryptography.AesCryptoServiceProvider()));
            TypeMap.Register.New<Sha1>(() => PortableFactory.SHA1Managed());
            TypeMap.Register.New<Sha256>(() => PortableFactory.SHA256Managed());
            TypeMap.Register.New<CryptoStreamBase>(() => PortableFactory.CryptoStream());
        }

        public static DirectoryInfo GetExecutingDirectory()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return new FileInfo(location.AbsolutePath).Directory;
        }

        private static IEnumerable<Assembly> LoadFromFiles(IEnumerable<FileInfo> files)
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (FileInfo file in files)
            {
                try
                {
                    assemblies.Add(Assembly.LoadFrom(file.FullName));
                }
                catch (BadImageFormatException bifex)
                {
                    New<IReport>().Exception(bifex);
                    continue;
                }
                catch (FileLoadException flex)
                {
                    New<IReport>().Exception(flex);
                    continue;
                }
            }
            return assemblies;
        }
    }
}