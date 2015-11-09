#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public class UserSettings : IUserSettings
    {
        public int CurrentSettingsVersion { get { return 10; } }
#if DEBUG
        private const int ASYMMETRIC_KEY_BITS = 768;
#else
        private const int ASYMMETRIC_KEY_BITS = 4096;
#endif

        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        private IDataStore _persistanceFileInfo;

        private IterationCalculator _keyWrapIterationCalculator;

        protected UserSettings(IterationCalculator keyWrapIterationCalculator)
        {
            _keyWrapIterationCalculator = keyWrapIterationCalculator;
        }

        public UserSettings(IDataStore fileInfo, IterationCalculator keyWrapIterationCalculator)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }

            _persistanceFileInfo = fileInfo;

            _keyWrapIterationCalculator = keyWrapIterationCalculator;

            if (!_persistanceFileInfo.IsAvailable)
            {
                _settings[nameof(SettingsVersion)] = Convert.ToString(CurrentSettingsVersion, CultureInfo.InvariantCulture);
                return;
            }

            using (JsonReader reader = new JsonTextReader(new StreamReader(_persistanceFileInfo.OpenRead())))
            {
                JsonSerializer serializer = CreateSerializer();
                _settings = serializer.Deserialize<Dictionary<string, string>>(reader) ?? new Dictionary<string, string>();
            }
        }

        private static JsonSerializer CreateSerializer()
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
            };
            return JsonSerializer.Create(serializerSettings);
        }

        public void Delete()
        {
            _persistanceFileInfo.Delete();
            _settings = new Dictionary<string, string>();
        }

        public string CultureName
        {
            get { return Load("CultureName", "en-US"); }
            set { Store("CultureName", value); }
        }

        public Uri LegacyRestApiBaseUrl
        {
            get { return Load(nameof(LegacyRestApiBaseUrl), new Uri("https://www.axantum.com/Xecrets/RestApi.ashx/")); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                Store(nameof(LegacyRestApiBaseUrl), value.ToString());
            }
        }

        public Uri RestApiBaseUrl
        {
            get { return Load(nameof(RestApiBaseUrl), new Uri("https://account.axcrypt.net/api/")); }
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
            get { return Load("UpdateUrl", new Uri("http://www.axantum.com/")); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                Store("UpdateUrl", value.ToString());
            }
        }

        public TimeSpan ApiTimeout
        {
            get { return Load(nameof(ApiTimeout), TimeSpan.FromSeconds(120)); }
            set { Store(nameof(ApiTimeout), value); }
        }

        public DateTime LastUpdateCheckUtc
        {
            get { return Load("LastUpdateCheckUtc", DateTime.MinValue); }
            set { Store("LastUpdateCheckUtc", value); }
        }

        public string NewestKnownVersion
        {
            get { return Load("NewestKnownVersion", String.Empty); }
            set { Store("NewestKnownVersion", value); }
        }

        public bool DebugMode
        {
            get { return Load("DebugMode", false); }
            set { Store("DebugMode", value); }
        }

        public Uri AxCrypt2HelpUrl
        {
            get { return Load("AxCrypt2HelpUrl", new Uri("http://www.axantum.com/AxCrypt/AxCryptNetHelp.html")); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                Store("AxCrypt2HelpUrl", value.ToString());
            }
        }

        public bool DisplayEncryptPassphrase
        {
            get { return Load("DisplayEncryptPassphrase", true); }
            set { Store("DisplayEncryptPassphrase", value); }
        }

        public bool DisplayDecryptPassphrase
        {
            get { return Load("DisplayDecryptPassphrase", true); }
            set { Store("DisplayDecryptPassphrase", value); }
        }

        public long GetKeyWrapIterations(Guid cryptoId)
        {
            return Load(cryptoId.ToString("N"), () => _keyWrapIterationCalculator.KeyWrapIterations(cryptoId));
        }

        public void SetKeyWrapIterations(Guid cryptoId, long keyWrapIterations)
        {
            Store(cryptoId.ToString("N"), keyWrapIterations);
        }

        public Salt ThumbprintSalt
        {
            get { return Load("ThumbprintSalt", () => New<int, Salt>(512)); }
            set { Store("ThumbprintSalt", Resolve.Serializer.Serialize(value)); }
        }

        public TimeSpan SessionNotificationMinimumIdle
        {
            get { return Load("WorkFolderMinimumIdle", TimeSpan.FromMilliseconds(500)); }
            set { Store("WorkFolderMinimumIdle", value); }
        }

        public int SettingsVersion
        {
            get { return Load("SettingsVersion", 0); }
            set { Store("SettingsVersion", value); }
        }

        public int AsymmetricKeyBits
        {
            get { return Load("AsymmetricKeyBits", ASYMMETRIC_KEY_BITS); }
            set { Store("AsymmetricKeyBits", value); }
        }

        public string UserEmail
        {
            get { return Load("UserEmail", String.Empty); }
            set { Store("UserEmail", value); }
        }

        public bool TryBrokenFile
        {
            get { return Load("TryBrokenFile", false); }
            set { Store("TryBrokenFile", value); }
        }

        public string this[string key]
        {
            get
            {
                string value;
                if (!_settings.TryGetValue(key, out value))
                {
                    return String.Empty;
                }
                return value;
            }
            set
            {
                if (this[key] == value)
                {
                    return;
                }
                _settings[key] = value;
                Save();
            }
        }

        protected virtual void Save()
        {
            using (TextWriter writer = new StreamWriter(_persistanceFileInfo.OpenWrite()))
            {
                JsonSerializer serializer = CreateSerializer();
                serializer.Serialize(writer, _settings);
            }
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

            string value;
            if (_settings.TryGetValue(key, out value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                }
            }

            T fallback = fallbackAction();
            this[key] = Convert.ToString(fallback, CultureInfo.InvariantCulture);
            return fallback;
        }

        public Salt Load(string key, Func<Salt> fallbackAction)
        {
            if (fallbackAction == null)
            {
                throw new ArgumentNullException("fallbackAction");
            }

            string value;
            if (_settings.TryGetValue(key, out value))
            {
                try
                {
                    return Resolve.Serializer.Deserialize<Salt>(value);
                }
                catch (JsonException)
                {
                }
            }

            Salt fallback = fallbackAction();
            this[key] = Resolve.Serializer.Serialize(fallback);
            return fallback;
        }

        public T Load<T>(string key, T fallback)
        {
            string value;
            if (!_settings.TryGetValue(key, out value))
            {
                return fallback;
            }
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public Uri Load(string key, Uri fallback)
        {
            string value;
            if (!_settings.TryGetValue(key, out value))
            {
                return fallback;
            }
            return new Uri(value);
        }

        public TimeSpan Load(string key, TimeSpan fallback)
        {
            string value;
            if (!_settings.TryGetValue(key, out value))
            {
                return fallback;
            }
            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        }

        public void Store<T>(string key, T value)
        {
            this[key] = Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}