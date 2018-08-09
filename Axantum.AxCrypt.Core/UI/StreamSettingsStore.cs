using Axantum.AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public abstract class StreamSettingsStore : TransientSettingsStore
    {
        protected void Initialize(Stream readStream)
        {
            Settings = New<IStringSerializer>().Deserialize<Dictionary<string, string>>(readStream);
        }

        protected void Save(Stream saveStream)
        {
            New<IStringSerializer>().Serialize(Settings, saveStream);
        }
    }
}