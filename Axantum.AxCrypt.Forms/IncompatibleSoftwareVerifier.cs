using System;
using Axantum.AxCrypt.Core.IO;

using static Axantum.AxCrypt.Abstractions.TypeResolve;


namespace Axantum.AxCrypt.Forms
{
    public class IncompatibleSoftwareVerifier
    {
        private readonly Lazy<bool> _isLavasoftApplicationInstalled = new Lazy<bool>(CheckLavasofApplicationInstallationState);

        public bool IsLavasoftApplicationInstalled => _isLavasoftApplicationInstalled.Value;

        private static bool CheckLavasofApplicationInstallationState()
        {
            return New<IDataStore>(@"C:\Windows\system32\LavasoftTcpService64.dll").IsAvailable;
        }
    }
}
