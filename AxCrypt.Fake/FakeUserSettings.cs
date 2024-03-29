﻿using AxCrypt.Abstractions;
using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Fake
{
    public class FakeUserSettings : UserSettings
    {
        public FakeUserSettings(IterationCalculator keyWrapIterationCalculator)
            : base(New<ISettingsStore>(), keyWrapIterationCalculator)
        {
        }

        public FakeUserSettings Initialize()
        {
            AsymmetricKeyBits = 768;
            AxCrypt2HelpUrl = new Uri("http://localhost/AxCrypt2Help");
            CultureName = "en-US";
            DebugMode = false;
            DisplayDecryptPassphrase = false;
            DisplayEncryptPassphrase = false;
            LastUpdateCheckUtc = New<INow>().Utc;
            NewestKnownVersion = "2.0.0.0";
            ThisVersion = "2.1.1234.0";
            SettingsVersion = 5;
            ThumbprintSalt = Salt.Zero;
            OfflineMode = false;
            UpdateUrl = new Uri("http://localhost/update");
            UserEmail = String.Empty;
            FewFilesThreshold = 10;

            return this;
        }
    }
}