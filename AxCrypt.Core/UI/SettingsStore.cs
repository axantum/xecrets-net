using AxCrypt.Core.IO;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI
{
    public class SettingsStore : StreamSettingsStore
    {
        private readonly IDataStore? _persistanceFileInfo;

        public SettingsStore(IDataStore dataStore)
        {
            _persistanceFileInfo = dataStore;

            if (_persistanceFileInfo == null || !_persistanceFileInfo.IsAvailable)
            {
                return;
            }

            using (New<FileLocker>().Acquire(_persistanceFileInfo))
            {
                using Stream readStream = _persistanceFileInfo.OpenRead();
                Initialize(readStream);
            }
        }

        public override void Clear()
        {
            using (New<FileLocker>().Acquire(_persistanceFileInfo!))
            {
                _persistanceFileInfo?.Delete();
            }
            base.Clear();
        }

        protected override void Save()
        {
            if (_persistanceFileInfo == null)
            {
                return;
            }

            using (New<FileLocker>().Acquire(_persistanceFileInfo))
            {
                using Stream writeStream = _persistanceFileInfo.OpenWrite();
                Save(writeStream);
            }
        }
    }
}
