using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Fake
{
    public class FakeUserSettings : UserSettings
    {
        public FakeUserSettings(IterationCalculator keyWrapIterationCalculator) : base(keyWrapIterationCalculator)
        {
            this.AsymmetricKeyBits = 768;
            this.AxCrypt2HelpUrl = new Uri("http://localhost/AxCrypt2Help");
            this.CultureName = "en-US";
            this.DebugMode = false;
            this.DisplayDecryptPassphrase = false;
            this.DisplayEncryptPassphrase = false;
            this.LastUpdateCheckUtc = New<INow>().Utc;
            this.NewestKnownVersion = "2.0.0.0";
            this.SessionNotificationMinimumIdle = TimeSpan.FromSeconds(1);
            this.SettingsVersion = 5;
            this.ThumbprintSalt = Salt.Zero;
            this.TryBrokenFile = false;
            this.UpdateUrl = new Uri("http://localhost/update");
            this.UserEmail = String.Empty;
        }

        protected override void Save()
        {
        }
    }
}