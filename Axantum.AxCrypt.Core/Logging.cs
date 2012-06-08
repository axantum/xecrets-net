#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core
{
    public static class Logging
    {
        private static TraceSwitch _switch = InitializeTraceSwitch();

        public static bool IsWarningEnabled
        {
            get
            {
                return _switch.Level >= TraceLevel.Warning;
            }
        }

        public static bool IsErrorEnabled
        {
            get
            {
                return _switch.Level >= TraceLevel.Error;
            }
        }

        public static bool IsInfoEnabled
        {
            get
            {
                return _switch.Level >= TraceLevel.Info;
            }
        }

        public static bool IsDebugEnabled
        {
            get
            {
                return _switch.Level >= TraceLevel.Verbose;
            }
        }

        public static void Warning(string message)
        {
            Trace.TraceWarning(message);
        }

        public static void Error(string message)
        {
            Trace.TraceError(message);
        }

        public static void Info(string message)
        {
            Trace.TraceInformation(message);
        }

        public static void Verbose(string message)
        {
            Trace.WriteLine("{1} Debug: {0}".InvariantFormat(message, AppName));
        }

        public static void SetLevel(TraceLevel level)
        {
            _switch.Level = level;
        }

        private static TraceSwitch InitializeTraceSwitch()
        {
            TraceSwitch traceSwitch = new TraceSwitch("axCryptSwitch", "Logging levels for AxCrypt");
#if DEBUG
            traceSwitch.Level = TraceLevel.Verbose;
#else
            traceSwitch.Level = TraceLevel.Error;
#endif
            return traceSwitch;
        }

        private static string _appName;

        private static string AppName
        {
            get
            {
                if (_appName == null)
                {
                    _appName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
                }
                return _appName;
            }
        }
    }
}