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