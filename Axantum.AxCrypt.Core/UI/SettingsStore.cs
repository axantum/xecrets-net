using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    public class SettingsStore : ISettingsStore
    {
        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        private IDataStore _persistanceFileInfo;

        public SettingsStore(IDataStore dataStore)
        {
            _persistanceFileInfo = dataStore;

            if (_persistanceFileInfo == null || !_persistanceFileInfo.IsAvailable)
            {
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

        public void Clear()
        {
            if (_persistanceFileInfo != null)
            {
                _persistanceFileInfo.Delete();
            }
            _settings = new Dictionary<string, string>();
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
            if (_persistanceFileInfo == null)
            {
                return;
            }

            using (FileLockReleaser.Acquire(_persistanceFileInfo))
            {
                using (TextWriter writer = new StreamWriter(_persistanceFileInfo.OpenWrite()))
                {
                    JsonSerializer serializer = CreateSerializer();
                    serializer.Serialize(writer, _settings);
                }
            }
        }
    }
}