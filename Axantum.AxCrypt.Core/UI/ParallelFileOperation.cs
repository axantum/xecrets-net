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
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI
{
    /// <summary>
    /// Performs file operations with controlled degree of parallelism.
    /// </summary>
    public class ParallelFileOperation
    {
        public ParallelFileOperation()
        {
        }

        /// <summary>
        /// Does an operation on a list of files, with parallelism.
        /// </summary>
        /// <param name="files">The files to operation on.</param>
        /// <param name="work">The work to do for each file.</param>
        /// <param name="allComplete">The completion callback after *all* files have been processed.</param>
        public virtual void DoFiles<T>(IEnumerable<T> files, Func<T, IProgressContext, FileOperationContext> work, Action<FileOperationContext> allComplete)
        {
            WorkerGroup workerGroup = null;
            Resolve.ProgressBackground.Work(
                (IProgressContext progress) =>
                {
                    using (workerGroup = new WorkerGroup(OS.Current.MaxConcurrency, progress))
                    {
                        foreach (T file in files)
                        {
                            IThreadWorker worker = workerGroup.CreateWorker(true);
                            if (workerGroup.FirstError.Status != FileOperationStatus.Success)
                            {
                                worker.Abort();
                                break;
                            }

                            T closureOverCopyOfLoopVariableFile = file;
                            worker.Work += (sender, e) =>
                            {
                                e.Result = work(closureOverCopyOfLoopVariableFile, new CancelProgressContext(e.Progress));
                            };
                            worker.Run();
                        }
                        workerGroup.WaitAllAndFinish();
                        return workerGroup.FirstError;
                    }
                },
                (FileOperationContext status) =>
                {
                    allComplete(status);
                });
        }
    }
}