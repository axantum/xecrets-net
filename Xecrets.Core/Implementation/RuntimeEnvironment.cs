#region Coypright and GPL License

/*
 * Xecrets.Net - Copyright © 2022-2026, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets.Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 *
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets.Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets.Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
 */

#endregion Coypright and GPL License

using AxCrypt.Core.Runtime;

using IPlatform = AxCrypt.Core.Runtime.IPlatform;
using PlatformEnum = AxCrypt.Core.Runtime.Platform;
using CorePlatform = Xecrets.Core.Public.Platform;

namespace Xecrets.Core.Implementation;

internal sealed class RuntimeEnvironment(string axCryptExtension) : IRuntimeEnvironment, IPlatform
{
    public bool IsLittleEndian => BitConverter.IsLittleEndian;

    public string AxCryptExtension { get; } = axCryptExtension;

    public PlatformEnum Platform => CorePlatform.IsWindows
        ? PlatformEnum.WindowsDesktop
        : CorePlatform.IsMacOS
            ? PlatformEnum.MacOsx
            : CorePlatform.IsLinux
                ? PlatformEnum.Linux
                : CorePlatform.IsAndroid
                    ? PlatformEnum.Android
                    : CorePlatform.IsIOS
                        ? PlatformEnum.AppleIos
                        : PlatformEnum.Unknown;

    public int StreamBufferSize => 4096;

    public bool CanTrackProcess => false;

    public int MaxConcurrency => Environment.ProcessorCount;

    public bool IsFirstInstance { get; set; } = true;

    public SynchronizationContext SynchronizationContext =>
        SynchronizationContext.Current ?? new SynchronizationContext();

    public string AppPath { get; set; } = AppContext.BaseDirectory;

    public ITiming StartTiming()
    {
        return new Timing();
    }

    public string EnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name) ?? string.Empty;
    }

    public bool IsFirstInstanceReady(TimeSpan timeout)
    {
        return true;
    }

    public void FirstInstanceIsReady()
    {
    }

    public void ExitApplication(int exitCode)
    {
        Environment.ExitCode = exitCode;
    }

    public void DebugMode(bool enable)
    {
    }

    public void RunApp(string arguments)
    {
        throw new NotSupportedException("Running another app is not supported by the default Xecrets.Core runtime.");
    }

    private sealed class Timing : ITiming
    {
        private readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public void Pause()
        {
            _stopwatch.Stop();
        }

        public void Resume()
        {
            _stopwatch.Start();
        }
    }
}
