using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IUserSettings
    {
        string this[string key]
        {
            get;
            set;
        }

        T Get<T>(string key);

        T Get<T>(string key, T fallback);

        void Set<T>(string key, T value);

        string CultureName { get; set; }

        Uri AxCrypt2VersionCheckUrl { get; set; }

        Uri UpdateUrl { get; set; }

        DateTime LastUpdateCheckUtc { get; set; }

        string NewestKnownVersion { get; set; }

        bool DebugMode { get; set; }

        Uri AxCrypt2HelpUrl { get; set; }

        bool DisplayEncryptPassphrase { get; set; }

        bool DisplayDecryptPassphrase { get; set; }
    }
}