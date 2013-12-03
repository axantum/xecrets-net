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

using Axantum.AxCrypt.Core.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

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

            Dictionary<string, string> settings = new Dictionary<string, string>();
            if (_persistanceFileInfo.Exists)
            {
                using (JsonReader reader = new JsonTextReader(new StreamReader(_persistanceFileInfo.OpenRead())))
                {
                    settings = serializer.Deserialize<Dictionary<string, string>>(reader);
                }
            }
            Defaults();
            foreach (KeyValuePair<string, string> kvp in settings)
            {
                _settings[kvp.Key] = kvp.Value;
            }
        }

        public static IRuntimeFileInfo DefaultPathInfo
        {
            get
            {
                return OS.Current.FileInfo(Path.Combine(OS.Current.WorkFolder.FullName, "UserSettings.txt"));
            }
        }

        private void Defaults()
        {
            CultureName = "en-US";
            AxCrypt2VersionCheckUrl = new Uri("https://www.axantum.com/Xecrets/RestApi.ashx/axcrypt2version/windows");
            UpdateUrl = new Uri("http://www.axantum.com/");
            LastUpdateCheckUtc = DateTime.MinValue;
            NewestKnownVersion = String.Empty;
            DebugMode = false;
            AxCrypt2HelpUrl = new Uri("http://www.axantum.com/AxCrypt/AxCryptNetHelp.html");
            DisplayEncryptPassphrase = true;
            DisplayDecryptPassphrase = false;
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

        public string CultureName { get { return this["CultureName"]; } set { this["CultureName"] = value; } }

        public Uri AxCrypt2VersionCheckUrl { get { return new Uri(this["AxCrypt2VersionCheckUrl"]); } set { this["AxCrypt2VersionCheckUrl"] = value.ToString(); } }

        public Uri UpdateUrl { get { return new Uri(this["UpdateUrl"]); } set { this["UpdateUrl"] = value.ToString(); } }

        public DateTime LastUpdateCheckUtc { get { return Convert.ToDateTime(this["LastUpdateCheckUtc"]); } set { this["LastUpdateCheckUtc"] = Convert.ToString(value); } }

        public string NewestKnownVersion { get { return this["NewestKnownVersion"]; } set { this["NewestKnownVersion"] = value; } }

        public bool DebugMode { get { return Convert.ToBoolean(this["DebugMode"]); } set { this["DebugMode"] = Convert.ToString(value); } }

        public Uri AxCrypt2HelpUrl { get { return new Uri(this["AxCrypt2HelpUrl"]); } set { this["AxCrypt2HelpUrl"] = value.ToString(); } }

        public bool DisplayEncryptPassphrase { get { return Convert.ToBoolean(this["DisplayEncryptPassphrase"]); } set { this["DisplayEncryptPassphrase"] = Convert.ToString(value); } }

        public bool DisplayDecryptPassphrase { get { return Convert.ToBoolean(this["DisplayDecryptPassphrase"]); } set { this["DisplayDecryptPassphrase"] = Convert.ToString(value); } }

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

        public T Get<T>(string key, T fallback)
        {
            string value;
            if (!_settings.TryGetValue(key, out value))
            {
                return fallback;
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public void Set<T>(string key, T value)
        {
            this[key] = Convert.ToString(value);
        }
    }
}