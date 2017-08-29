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
using Axantum.AxCrypt.Core.IO;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Desktop
{
    public class Report : IReport
    {
        private INow _now;

        private string _currentFilePath;
        private string _previousFilePath;

        private IDataStore _currentLogFile;
        private IDataStore _previousLogFile;

        public Report(string folderPath)
        {
            _now = New<INow>();
            _currentFilePath = Path.Combine(folderPath, "ReportSnapshot.txt");
            _previousFilePath = Path.Combine(folderPath, "ReportSnapshot.1.txt");
            _previousLogFile = New<IDataStore>(_previousFilePath);
            _currentLogFile = New<IDataStore>(_currentFilePath);
        }

        public void Exception(Exception ex)
        {
            MoveCurrentLogFileContentToPreviousLogFileIfSizeIncreaseMoreThan1MB();
            using (FileLock fileLock = New<FileLocker>().Acquire(_currentLogFile))
            {
                StringBuilder sb = new StringBuilder();
                if (!_currentLogFile.IsAvailable)
                {
                    sb.AppendLine(Texts.ReportSnapshotIntro).AppendLine();
                }

                AxCryptException ace = ex as AxCryptException;
                string displayContext = ace?.DisplayContext ?? string.Empty;
                sb.AppendFormat("----------- Exception at {0} -----------", _now.Utc.ToString("u")).AppendLine();
                sb.AppendLine(displayContext);
                sb.AppendLine(ex?.ToString() ?? "(null)");

                using (StreamWriter writer = new StreamWriter(_currentLogFile.OpenUpdate(), Encoding.UTF8))
                {
                    writer.Write(sb.ToString());
                }
            }
        }

        public void Open()
        {
            Process.Start(_currentFilePath);
        }

        private void MoveCurrentLogFileContentToPreviousLogFileIfSizeIncreaseMoreThan1MB()
        {
            if (_currentLogFile.Length() > 1000000)
            {
                _currentLogFile.MoveTo(_previousFilePath);
            }
        }
    }
}