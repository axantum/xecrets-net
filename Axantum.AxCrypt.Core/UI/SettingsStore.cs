using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public class SettingsStore : StreamSettingsStore
    {
        private IDataStore _persistanceFileInfo;

        public SettingsStore(IDataStore dataStore)
        {
            _persistanceFileInfo = dataStore;

            if (_persistanceFileInfo == null || !_persistanceFileInfo.IsAvailable)
            {
                return;
            }

            using (New<FileLocker>().Acquire(_persistanceFileInfo))
            {
                Initialize(_persistanceFileInfo.OpenRead());
            }
        }

        public override void Clear()
        {
            using (New<FileLocker>().Acquire(_persistanceFileInfo))
            {
                if (_persistanceFileInfo != null)
                {
                    _persistanceFileInfo.Delete();
                }
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
                Save(_persistanceFileInfo.OpenWrite());
            }
        }
    }
}