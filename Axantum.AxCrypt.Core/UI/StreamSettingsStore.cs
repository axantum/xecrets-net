using Axantum.AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public abstract class StreamSettingsStore : ISettingsStore
    {
        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        protected void Initialize(Stream readStream)
        {
            _settings = New<IStringSerializer>().Deserialize<Dictionary<string, string>>(readStream);
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
            New<IStringSerializer>().Serialize(_settings, saveStream);
        }
    }
}