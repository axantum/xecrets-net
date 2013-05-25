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
using Axantum.AxCrypt.Mono;

#endregion Coypright and License

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.MonoTouch
{
    public class RuntimeEnvironment : Mono.RuntimeEnvironment
    {
        public RuntimeEnvironment() : base(TimeSpan.FromSeconds(1))
        {
        }
        
        public IFileWatcher FileWatcher(string path)
        {
            return new FileWatcher(path);
        }
        
        private IRuntimeFileInfo _temporaryDirectoryInfo;
        
        public IRuntimeFileInfo TemporaryDirectoryInfo
        {
            get
            {
                if (_temporaryDirectoryInfo == null)
                {
                    string temporaryFolderPath = Path.Combine(Path.GetTempPath(), @"AxCrypt" + Path.DirectorySeparatorChar);
                    IRuntimeFileInfo temporaryFolderInfo = FileInfo(temporaryFolderPath);
					Directory.CreateDirectory (temporaryFolderPath);
                    _temporaryDirectoryInfo = temporaryFolderInfo;
                }
                
                return _temporaryDirectoryInfo;
            }
        }
        
        public void NotifyFileChanged()
        {
            OnChanged();
        }
        
        protected virtual void OnChanged()
        {
            EventHandler<EventArgs> handler = FileChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
        
        public event EventHandler<EventArgs> FileChanged;
    }
}