#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI
{
    public class UserSettings : IUserSettings
    {
        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        private IRuntimeFileInfo _persistanceFileInfo;

        public UserSettings(IRuntimeFileInfo fileInfo)
        {
            _persistanceFileInfo = fileInfo;

            JsonSerializer serializer = CreateSerializer();

            if (_persistanceFileInfo.Exists)
            {
                using (JsonReader reader = new JsonTextReader(new StreamReader(_persistanceFileInfo.OpenRead())))
                {
                    _settings = serializer.Deserialize<Dictionary<string, string>>(reader);
                }
            }
        }

        public static IRuntimeFileInfo DefaultPathInfo
        {
            get
            {
                return OS.Current.FileInfo(Path.Combine(OS.Current.WorkFolder.FullName, "UserSettings.txt"));
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

        public string CultureName
        {
            get { return Get("CultureName", "en-US"); }
            set { Set("CultureName", value); }
        }

        public Uri AxCrypt2VersionCheckUrl
        {
            get { return Get("AxCrypt2VersionCheckUrl", new Uri("https://www.axantum.com/Xecrets/RestApi.ashx/axcrypt2version/windows")); }
            set { Set("AxCrypt2VersionCheckUrl", value.ToString()); }
        }

        public Uri UpdateUrl
        {
            get { return Get("UpdateUrl", new Uri("http://www.axantum.com/")); }
            set { Set("UpdateUrl", value.ToString()); }
        }

        public DateTime LastUpdateCheckUtc
        {
            get { return Get("LastUpdateCheckUtc", DateTime.MinValue); }
            set { Set("LastUpdateCheckUtc", value); }
        }

        public string NewestKnownVersion
        {
            get { return Get("NewestKnownVersion", String.Empty); }
            set { Set("NewestKnownVersion", value); }
        }

        public bool DebugMode
        {
            get { return Get("DebugMode", false); }
            set { Set("DebugMode", value); }
        }

        public Uri AxCrypt2HelpUrl
        {
            get { return Get("AxCrypt2HelpUrl", new Uri("http://www.axantum.com/AxCrypt/AxCryptNetHelp.html")); }
            set { Set("AxCrypt2HelpUrl", value.ToString()); }
        }

        public bool DisplayEncryptPassphrase
        {
            get { return Get("DisplayEncryptPassphrase", true); }
            set { Set("DisplayEncryptPassphrase", value); }
        }

        public bool DisplayDecryptPassphrase
        {
            get { return Get("DisplayDecryptPassphrase", true); }
            set { Set("DisplayDecryptPassphrase", value); }
        }

        public long KeyWrapIterations
        {
            get { return Get("KeyWrapIterations", () => KeyWrapIterationCalculator.CalculatedKeyWrapIterations); }
            set { Set("KeyWrapIterations", value); }
        }

        public KeyWrapSalt ThumbprintSalt
        {
            get { return Get("ThumbprintSalt", () => new KeyWrapSalt(AesKey.DefaultKeyLength)); }
            set { Set("ThumbprintSalt", JsonConvert.SerializeObject(value)); }
        }

        public TimeSpan SessionChangedMinimumIdle
        {
            get { return Get("WorkFolderMinimumIdle", TimeSpan.FromMilliseconds(500)); }
            set { Set("WorkFolderMinimumIdle", value); }
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
            JsonSerializer serializer = CreateSerializer();
            using (TextWriter writer = new StreamWriter(_persistanceFileInfo.OpenWrite()))
            {
                serializer.Serialize(writer, _settings);
            }
        }

        public T Get<T>(string key)
        {
            return Get(key, default(T));
        }

        public T Get<T>(string key, Func<T> fallbackAction)
        {
            string value;
            if (_settings.TryGetValue(key, out value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (FormatException)
                {
                }
            }

            T fallback = fallbackAction();
            this[key] = Convert.ToString(fallback);
            return fallback;
        }

        public KeyWrapSalt Get(string key, Func<KeyWrapSalt> fallbackAction)
        {
            string value;
            if (_settings.TryGetValue(key, out value))
            {
                try
                {
                    return JsonConvert.DeserializeObject<KeyWrapSalt>(value);
                }
                catch (JsonException)
                {
                }
            }

            KeyWrapSalt fallback = fallbackAction();
            this[key] = JsonConvert.SerializeObject(fallback);
            return fallback;
        }

        public T Get<T>(string key, T fallback)
        {
            string value;
            if (!_settings.TryGetValue(key, out value))
            {
                return fallback;
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public Uri Get(string key, Uri fallback)
        {
            string value;
            if (!_settings.TryGetValue(key, out value))
            {
                return fallback;
            }
            return new Uri(value);
        }

        public TimeSpan Get(string key, TimeSpan fallback)
        {
            string value;
            if (!_settings.TryGetValue(key, out value))
            {
                return fallback;
            }
            return TimeSpan.Parse(value);
        }

        public void Set<T>(string key, T value)
        {
            this[key] = Convert.ToString(value);
        }
    }
}