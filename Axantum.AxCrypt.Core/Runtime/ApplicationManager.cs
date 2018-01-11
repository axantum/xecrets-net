using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class ApplicationManager
    {
        public async Task<bool> ValidateSettings()
        {
            if (Resolve.UserSettings.SettingsVersion >= New<UserSettingsVersion>().Current)
            {
                return true;
            }

            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, Texts.UserSettingsFormatChangeNeedsReset);
            await ClearAllSettings();
            await StopAndExit();
            return false;
        }

        public async Task ClearAllSettings()
        {
            await ShutdownBackgroundSafe();

            Resolve.UserSettings.Clear();
            Resolve.FileSystemState.Delete();
            Resolve.WorkFolder.FileInfo.FileItemInfo(LocalAccountService.FileName).Delete();
            New<KnownPublicKeys>().Delete();
            Resolve.UserSettings.SettingsVersion = New<UserSettingsVersion>().Current;
        }

        public async Task StopAndExit()
        {
            await ShutdownBackgroundSafe();

            New<IUIThread>().ExitApplication();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void WaitForBackgroundToComplete()
        {
            New<IProgressBackground>().WaitForIdle();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public async Task ShutdownBackgroundSafe()
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