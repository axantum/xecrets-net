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

using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Runtime
{
    /// <summary>
    /// An IProgressContext wrapper intended to be unique for each thread, thus enabling managing
    /// how interaction with the UI thread is handled. The requirement is that a thread should be
    /// able to be guaranteed to start in a serialized and determininstic order withing the larger
    /// context of a group threads.
    /// The scenario is for example encrypting many files, and ensuring that user prompts are
    /// presented in order, and if for example a cancel occurs no more prompts are issued.
    /// </summary>
    public class ThreadProgressContext : IProgressContext
    {
        private IProgressContext _progress;

        private bool _isSingleActiveThread;

        public ThreadProgressContext(IProgressContext progress)
        {
            _progress = progress;
        }

        public event EventHandler<ProgressEventArgs> Progressing
        {
            add
            {
                _progress.Progressing += value;
            }
            remove
            {
                _progress.Progressing -= value;
            }
        }

        public void RemoveCount(long totalCount, long progressCount)
        {
            _progress.RemoveCount(totalCount, progressCount);
        }

        public void AddTotal(long count)
        {
            _progress.AddTotal(count);
        }

        public void AddCount(long count)
        {
            _progress.AddCount(count);
        }

        public void NotifyLevelStart()
        {
            _progress.NotifyLevelStart();
        }

        public void NotifyLevelFinished()
        {
            _progress.NotifyLevelFinished();
        }

        public bool Cancel
        {
            get
            {
                return _progress.Cancel;
            }
            set
            {
                _progress.Cancel = value;
            }
        }

        public bool AllItemsConfirmed
        {
            get
            {
                return _progress.AllItemsConfirmed;
            }
            set
            {
                _progress.AllItemsConfirmed = value;
            }
        }

        public void EnterSingleThread()
        {
            if (_isSingleActiveThread)
            {
                return;
            }
            _progress.EnterSingleThread();
            _isSingleActiveThread = true;
        }

        public void LeaveSingleThread()
        {
            if (!_isSingleActiveThread)
            {
                return;
            }
            _progress.LeaveSingleThread();
            _isSingleActiveThread = false;
        }
    }
}