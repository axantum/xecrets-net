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
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.MonoTouch
{
	internal class Logging : ILogging
    {
        private LogLevel _level = LogLevel.Error;

        #region ILogging Members

		public event EventHandler<LoggingEventArgs> Logged;

		public void Dispose ()
		{
		}
        
        public void SetLevel(LogLevel level)
        {
            switch (level)
            {
            case LogLevel.Fatal:
            case LogLevel.Error:
            case LogLevel.Warning:
            case LogLevel.Info:
            case LogLevel.Debug:
                _level = level;
                break;

            default:
                throw new ArgumentException("level must be a value form the LogLevel enumeration.");
            }
        }
        
        public bool IsFatalEnabled
        {
            get { return _level >= LogLevel.Fatal; }
        }
        
        public bool IsErrorEnabled
        {
            get { return _level >= LogLevel.Error; }
        }
        
        public bool IsWarningEnabled
        {
            get { return _level >= LogLevel.Warning; }
        }
        
        public bool IsInfoEnabled
        {
            get { return _level >= LogLevel.Info; }
        }
        
        public bool IsDebugEnabled
        {
            get { return _level >= LogLevel.Debug; }
        }
        
        public virtual void LogFatal (string message)
        {
            if (IsFatalEnabled)
            {
                Console.WriteLine("{1} Fatal: {0}".InvariantFormat(message, AppName));
            }
        }
        
        public void LogError(string message)
        {
            if (IsErrorEnabled)
            {
                Console.WriteLine("{1} Error: {0}".InvariantFormat(message, AppName));
            }
        }
        
        public void LogWarning(string message)
        {
            if (IsWarningEnabled)
            {
                Console.WriteLine("{1} Warning: {0}".InvariantFormat(message, AppName));
            }
        }
        
        public void LogInfo(string message)
        {
            if (IsInfoEnabled)
            {
                Console.WriteLine("{1} Info: {0}".InvariantFormat(message, AppName));
            }
        }
        
        public void LogDebug(string message)
        {
            if (IsDebugEnabled)
            {
                Console.WriteLine("{1} Debug: {0}".InvariantFormat(message, AppName));
            }
        }
        
        #endregion ILogging Members
        
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