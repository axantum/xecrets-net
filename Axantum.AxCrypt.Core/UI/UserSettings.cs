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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Newtonsoft.Json;

namespace Axantum.AxCrypt.Core.UI
{
    public class UserSettings : IUserSettings
    {
        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        private IRuntimeFileInfo _persistanceFileInfo;

        private IterationCalculator _keyWrapIterationCalculator;

        public UserSettings(IRuntimeFileInfo fileInfo, IterationCalculator keyWrapIterationCalculator)
        {
            _persistanceFileInfo = fileInfo;

            _keyWrapIterationCalculator = keyWrapIterationCalculator;

            if (_persistanceFileInfo.IsExistingFile)
            {
                using (JsonReader reader = new JsonTextReader(new StreamReader(_persistanceFileInfo.OpenRead())))
                {
                    JsonSerializer serializer = CreateSerializer();
                    _settings = serializer.Deserialize<Dictionary<string, string>>(reader);
                }
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
        }

        public string CultureName
        {
            get { return Load("CultureName", "en-US"); }
            set { Store("CultureName", value); }
        }

        public Uri AxCrypt2VersionCheckUrl
        {
            get { return Load("AxCrypt2VersionCheckUrl", new Uri("https://www.axantum.com/Xecrets/RestApi.ashx/axcrypt2version/windows")); }
            set { Store("AxCrypt2VersionCheckUrl", value.ToString()); }
        }

        public Uri UpdateUrl
        {
            get { return Load("UpdateUrl", new Uri("http://www.axantum.com/")); }
            set { Store("UpdateUrl", value.ToString()); }
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
            set { Store("AxCrypt2HelpUrl", value.ToString()); }
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

        public long V1KeyWrapIterations
        {
            get { return Load("V1KeyWrapIterations", () => _keyWrapIterationCalculator.V1KeyWrapIterations()); }
            set { Store("V1KeyWrapIterations", value); }
        }

        public long V2KeyWrapIterations
        {
            get { return Load("V2KeyWrapIterations", () => _keyWrapIterationCalculator.V2KeyWrapIterations()); }
            set { Store("V2KeyWrapIterations", value); }
        }

        public Salt ThumbprintSalt
        {
            get { return Load("ThumbprintSalt", () => Factory.New<int, Salt>(512)); }
            set { Store("ThumbprintSalt", JsonConvert.SerializeObject(value)); }
        }

        public TimeSpan SessionNotificationMinimumIdle
        {
            get { return Load("WorkFolderMinimumIdle", TimeSpan.FromMilliseconds(500)); }
            set { Store("WorkFolderMinimumIdle", value); }
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

        private void Save()
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
            string value;
            if (_settings.TryGetValue(key, out value))
            {
                try
                {
                    return JsonConvert.DeserializeObject<Salt>(value);
                }
                catch (JsonException)
                {
                }
            }

            Salt fallback = fallbackAction();
            this[key] = JsonConvert.SerializeObject(fallback);
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