using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class ApplicationManager
    {
        public async Task<bool> ValidateSettings()
        {
            if (Resolve.UserSettings.SettingsVersion >= UserSettings.CurrentSettingsVersion)
            {
                return true;
            }

            Texts.UserSettingsFormatChangeNeedsReset.ShowWarning(Texts.WarningTitle);
            await ClearAllSettings();
            await StopAndExit();
            return false;
        }

        public async Task ClearAllSettings()
        {
            await ShutDownBackgroundSafe();

            Resolve.UserSettings.Clear();
            Resolve.FileSystemState.Delete();
            Resolve.WorkFolder.FileInfo.FileItemInfo(LocalAccountService.FileName).Delete();
            New<KnownPublicKeys>().Delete();
            Resolve.UserSettings.SettingsVersion = UserSettings.CurrentSettingsVersion;
        }

        public async Task StopAndExit()
        {
            await ShutDownBackgroundSafe();

            New<IUIThread>().Exit();
        }

        public void WaitForBackgroundToComplete()
        {
            New<IProgressBackground>().WaitForIdle();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public async Task ShutDownBackgroundSafe()
        {
            try
            {
                New<WorkFolderWatcher>().Dispose();
            }
            catch
            {
            }
            try
            {
                New<ActiveFileWatcher>().Dispose();
            }
            catch
            {
            }

            try
            {
                New<IProgressBackground>().WaitForIdle();
            }
            catch
            {
            }
        }
    }
}