#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public class UserSettings
    {
        public static int CurrentSettingsVersion { get { return 10; } }
#if DEBUG
        private const int ASYMMETRIC_KEY_BITS = 768;
#else
        private const int ASYMMETRIC_KEY_BITS = 4096;
#endif

        private ISettingsStore _settingsStore;

        private IterationCalculator _keyWrapIterationCalculator;

        public UserSettings(ISettingsStore settingsStore, IterationCalculator keyWrapIterationCalculator)
        {
            if (settingsStore == null)
            {
                throw new ArgumentNullException(nameof(settingsStore));
            }

            _settingsStore = settingsStore;

            _keyWrapIterationCalculator = keyWrapIterationCalculator;

            if (_settingsStore[nameof(SettingsVersion)].Length == 0)
            {
                _settingsStore[nameof(SettingsVersion)] = Convert.ToString(CurrentSettingsVersion, CultureInfo.InvariantCulture);
            }
        }

        public void Clear()
        {
            _settingsStore.Clear();
        }

        public string CultureName
        {
            get { return Load(nameof(CultureName), CultureInfo.CurrentUICulture.Name); }
            set { Store(nameof(CultureName), value); }
        }

        public Uri RestApiBaseUrl
        {
            get { return Load(nameof(RestApiBaseUrl), new Uri("{0}api/".InvariantFormat(AccountWebUrl))); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                Store(nameof(RestApiBaseUrl), value.ToString());
            }
        }

        public Uri UpdateUrl
        {
            get { return Load(nameof(UpdateUrl), new Uri("http://www.axcrypt.net/download/")); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                Store(nameof(UpdateUrl), value.ToString());
            }
        }

        public Uri AccountWebUrl
        {
            get { return Load(nameof(AccountWebUrl), new Uri("https://account.axcrypt.net/")); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                Store(nameof(AccountWebUrl), value.ToString());
            }
        }

        public UpdateLevels UpdateLevel
        {
            get { return (UpdateLevels)Load(nameof(UpdateLevel), (int)UpdateLevels.None); }
            set { Store(nameof(UpdateLevel), (int)value); }
        }

        public TimeSpan ApiTimeout
        {
            get { return Load(nameof(ApiTimeout), TimeSpan.FromSeconds(15)); }
            set { Store(nameof(ApiTimeout), value); }
        }

        public DateTime LastUpdateCheckUtc
        {
            get { return Load(nameof(LastUpdateCheckUtc), DateTime.MinValue); }
            set { Store(nameof(LastUpdateCheckUtc), value); }
        }

        public string NewestKnownVersion
        {
            get { return Load(nameof(NewestKnownVersion), string.Empty); }
            set { Store(nameof(NewestKnownVersion), value); }
        }

        public string ThisVersion
        {
            get { return Load(nameof(ThisVersion), string.Empty); }
            set { Store(nameof(ThisVersion), value); }
        }

        public bool DebugMode
        {
            get { return Load(nameof(DebugMode), false); }
            set { Store(nameof(DebugMode), value); }
        }

        public bool RestoreFullWindow
        {
            get { return Load(nameof(RestoreFullWindow), false); }
            set { Store(nameof(RestoreFullWindow), value); }
        }

        public bool IsFileImportHelpMessageAlreadyDisplayed
        {
            get { return Load(nameof(IsFileImportHelpMessageAlreadyDisplayed), false); }
            set { Store(nameof(IsFileImportHelpMessageAlreadyDisplayed), value); }
        }

        public Uri AxCrypt2HelpUrl
        {
            get { return Load(nameof(AxCrypt2HelpUrl), new Uri("http://www.axcrypt.net/documentation/get-started/")); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                Store(nameof(AxCrypt2HelpUrl), value.ToString());
            }
        }

        public bool DisplayEncryptPassphrase
        {
            get { return Load(nameof(DisplayEncryptPassphrase), true); }
            set { Store(nameof(DisplayEncryptPassphrase), value); }
        }

        public bool DisplayDecryptPassphrase
        {
            get { return Load(nameof(DisplayDecryptPassphrase), false); }
            set { Store(nameof(DisplayDecryptPassphrase), value); }
        }

        public virtual long GetKeyWrapIterations(Guid cryptoId)
        {
            return Load(cryptoId.ToString("N"), () => _keyWrapIterationCalculator.KeyWrapIterations(cryptoId));
        }

        public virtual void SetKeyWrapIterations(Guid cryptoId, long keyWrapIterations)
        {
            Store(cryptoId.ToString("N"), keyWrapIterations);
        }

        public virtual Salt ThumbprintSalt
        {
            get { return Load(nameof(ThumbprintSalt), () => New<int, Salt>(512)); }
            set { Store(nameof(ThumbprintSalt), Resolve.Serializer.Serialize(value)); }
        }

        public int SettingsVersion
        {
            get { return Load("SettingsVersion", 0); }
            set { Store("SettingsVersion", value); }
        }

        public int AsymmetricKeyBits
        {
            get { return Load(nameof(AsymmetricKeyBits), ASYMMETRIC_KEY_BITS); }
            set { Store(nameof(AsymmetricKeyBits), value); }
        }

        public string UserEmail
        {
            get { return Load(nameof(UserEmail), String.Empty); }
            set { Store(nameof(UserEmail), value); }
        }

        public bool TryBrokenFile
        {
            get { return Load(nameof(TryBrokenFile), false); }
            set { Store(nameof(TryBrokenFile), value); }
        }

        public bool IsFirstSignIn
        {
            get { return Load(nameof(IsFirstSignIn), true); }
            set { Store(nameof(IsFirstSignIn), value); }
        }

        public bool OfflineMode
        {
            get { return Load(nameof(OfflineMode), false); }
            set { Store(nameof(OfflineMode), value); }
        }

        public LegacyConversionMode LegacyConversionMode
        {
            get { return (LegacyConversionMode)Load(nameof(LegacyConversionMode), (int)LegacyConversionMode.NotDecided); }
            set { Store(nameof(LegacyConversionMode), (int)value); }
        }

        public PremiumStatus LastKnownPremiumStatus
        {
            get { return (PremiumStatus)Load(nameof(LastKnownPremiumStatus), (int)PremiumStatus.Unknown); }
            set { Store(nameof(LastKnownPremiumStatus), (int)value); }
        }

        public int LastKnownPremiumDaysLeft
        {
            get { return Load(nameof(LastKnownPremiumDaysLeft), 0); }
            set { Store(nameof(LastKnownPremiumDaysLeft), value); }
        }

        public T Load<T>(string key)
        {
            return Load(key, default(T));
        }

        public T Load<T>(string key, Func<T> fallbackAction)
        {
            if (fallbackAction == null)
            {
                throw new ArgumentNullException("fallbackAction");
            }

            string value = _settingsStore[key];
            if (value.Length > 0)
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
                }
                catch (FormatException fex)
                {
                    New<IReport>().Exception(fex);
                }
            }

            T fallback = fallbackAction();
            _settingsStore[key] = Convert.ToString(fallback, CultureInfo.InvariantCulture);
            return fallback;
        }

        public Salt Load(string key, Func<Salt> fallbackAction)
        {
            if (fallbackAction == null)
            {
                throw new ArgumentNullException("fallbackAction");
            }

            string value = _settingsStore[key];
            if (value.Length > 0)
            {
                try
                {
                    return Resolve.Serializer.Deserialize<Salt>(value);
                }
                catch (JsonException jex)
                {
                    New<IReport>().Exception(jex);
                }
            }

            Salt fallback = fallbackAction();
            _settingsStore[key] = Resolve.Serializer.Serialize(fallback);
            return fallback;
        }

        public T Load<T>(string key, T fallback)
        {
            string value = _settingsStore[key];
            if (value.Length == 0)
            {
                return fallback;
            }
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public Uri Load(string key, Uri fallback)
        {
            string value = _settingsStore[key];
            if (value.Length == 0)
            {
                return fallback;
            }
            return new Uri(value);
        }

        public TimeSpan Load(string key, TimeSpan fallback)
        {
            string value = _settingsStore[key];
            if (value.Length == 0)
            {
                return fallback;
            }
            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        }

        public void Store<T>(string key, T value)
        {
            _settingsStore[key] = Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}