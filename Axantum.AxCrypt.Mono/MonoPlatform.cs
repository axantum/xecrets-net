using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono
{
    public class MonoPlatform : IPlatform
    {
        public Platform Platform
        {
            get
            {
                OperatingSystem os = global::System.Environment.OSVersion;
                PlatformID pid = os.Platform;
                switch (pid)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                        return Platform.WindowsDesktop;

                    case PlatformID.MacOSX:
                        return Platform.MacOsx;

                    case PlatformID.Unix:
                        return Platform.Linux;

                    case PlatformID.WinCE:
                        return Platform.WindowsMobile;

                    case PlatformID.Xbox:
                        return Platform.Xbox;

                    default:
                        return Platform.Unknown;
                }
            }
        }
    }
}
