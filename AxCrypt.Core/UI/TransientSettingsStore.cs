using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.UI
{
    public class TransientSettingsStore : ISettingsStore
    {
        protected Dictionary<string, string> Settings = new Dictionary<string, string>();

        public virtual string this[string key]
        {
            get
            {
                string value;
                if (!Settings.TryGetValue(key, out value))
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
                Settings[key] = value;
                Save();
            }
        }

        protected virtual void Save()
        {
        }

        public virtual void Clear()
        {
            Settings = new Dictionary<string, string>();
        }
    }
}
