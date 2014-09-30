#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.IO;
using System;
using System.Threading;

namespace Axantum.AxCrypt.Core.Runtime
{
    public interface IRuntimeEnvironment
    {
        bool IsLittleEndian { get; }

        string AxCryptExtension { get; }

        Platform Platform { get; }

        int StreamBufferSize { get; }

        DateTime UtcNow { get; }

        ILauncher Launch(string path);

        ITiming StartTiming();

        IWebCaller CreateWebCaller();

        bool CanTrackProcess { get; }

        string EnvironmentVariable(string name);

        int MaxConcurrency { get; }

        bool IsFirstInstance { get; }

        bool FirstInstanceRunning(TimeSpan timeout);

        void ExitApplication(int exitCode);

        void DebugMode(bool enable);

        SynchronizationContext SynchronizationContext { get; }
    }
}