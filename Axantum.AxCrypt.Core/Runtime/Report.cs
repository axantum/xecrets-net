#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Portable;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class Report : IReport
    {
        private string _currentFilePath;

        private string _previousFilePath;

        private long _maxSizeInBytes;

        public Report(string folderPath, long maxSizeInBytes)
        {
            _currentFilePath = New<IPath>().Combine(folderPath.NormalizeFilePath(), "ReportSnapshot.txt");
            _previousFilePath = New<IPath>().Combine(folderPath.NormalizeFilePath(), "ReportSnapshot.1.txt");
            _maxSizeInBytes = maxSizeInBytes;
        }

        public void Exception(Exception ex)
        {
            MoveCurrentLogFileContentToPreviousLogFileIfSizeIncreaseMoreThanMaxSize();
            using (FileLock fileLock = New<FileLocker>().Acquire(New<IDataStore>(_currentFilePath)))
            {
                StringBuilder sb = new StringBuilder();
                if (!fileLock.DataStore.IsAvailable)
                {
                    sb.AppendLine(Texts.ReportSnapshotIntro).AppendLine();
                }

                AxCryptException ace = ex as AxCryptException;
                string displayContext = ace?.DisplayContext ?? string.Empty;
                sb.AppendFormat("----------- Exception at {0} -----------", New<INow>().Utc.ToString("u")).AppendLine();
                sb.AppendLine(displayContext);
                sb.AppendLine(ex?.ToString() ?? "(null)");

                using (StreamWriter writer = new StreamWriter(fileLock.DataStore.OpenUpdate(), Encoding.UTF8))
                {
                    writer.Write(sb.ToString());
                }
            }
        }

        public void Open()
        {
            New<ILauncher>().Launch(_currentFilePath);
        }

        private void MoveCurrentLogFileContentToPreviousLogFileIfSizeIncreaseMoreThanMaxSize()
        {
            using (FileLock currentLock = New<FileLocker>().Acquire(New<IDataStore>(_currentFilePath)))
            {
                if (!currentLock.DataStore.IsAvailable || currentLock.DataStore.Length() <= _maxSizeInBytes)
                {
                    return;
                }
                using (FileLock previousLock = New<FileLocker>().Acquire(New<IDataStore>(_previousFilePath)))
                {
                    currentLock.DataStore.MoveTo(previousLock.DataStore.FullName);
                }
            }
        }
    }
}