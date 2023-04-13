using AxCrypt.Abstractions;
using AxCrypt.Core.Runtime;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI
{
    public abstract class StreamSettingsStore : TransientSettingsStore
    {
        protected void Initialize(Stream readStream)
        {
            Settings = New<IStringSerializer>().Deserialize<Dictionary<string, string>>(readStream) ?? throw new InternalErrorException("Could not deserialize the SettingsStore.");
        }

        protected void Save(Stream saveStream)
        {
            New<IStringSerializer>().Serialize(Settings, saveStream);
        }
    }
}
