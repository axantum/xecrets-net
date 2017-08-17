using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI
{
    public abstract class StreamSettingsStore : ISettingsStore
    {
        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        protected void Initialize(Stream readStream)
        {
            using (JsonReader reader = new JsonTextReader(new StreamReader(readStream)))
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

        public virtual void Clear()
        {
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

        protected abstract void Save();

        protected void Save(Stream saveStream)
        {
            using (TextWriter writer = new StreamWriter(saveStream))
            {
                JsonSerializer serializer = CreateSerializer();
                serializer.Serialize(writer, _settings);
            }
        }
    }
}