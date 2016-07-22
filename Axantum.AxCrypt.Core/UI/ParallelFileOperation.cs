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
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public async virtual Task DoFilesAsync<T>(IEnumerable<T> files, Func<T, IProgressContext, Task<FileOperationContext>> work, Action<FileOperationContext> allComplete)
        {
            WorkerGroupProgressContext groupProgress = new WorkerGroupProgressContext(new CancelProgressContext(new ProgressContext()), New<ISingleThread>());
            await New<IProgressBackground>().WorkAsync(nameof(DoFilesAsync),
            (IProgressContext progress) =>
            {
                progress.NotifyLevelStart();
                FileOperationContext result = new FileOperationContext(string.Empty, ErrorStatus.Success);
                Parallel.ForEach(files,
                    new ParallelOptions() { MaxDegreeOfParallelism = New<IRuntimeEnvironment>().MaxConcurrency, },
                    () =>
                    {
                        return new FileOperationContext(string.Empty, ErrorStatus.Success);
                    },
                    (file, loopState, fileOperationContext) =>
                    {
                        if (fileOperationContext.ErrorStatus != ErrorStatus.Success)
                        {
                            loopState.Stop();
                        }
                        if (loopState.IsStopped)
                        {
                            return new FileOperationContext(file.ToString(), ErrorStatus.Canceled);
                        }
                        try
                        {
                            return work(file, progress).Result;
                        }
                        catch (AggregateException ae) when (ae.InnerException is OperationCanceledException)
                        {
                            return new FileOperationContext(file.ToString(), ErrorStatus.Canceled);
                        }
                        catch (AggregateException ae) when (ae.InnerException is AxCryptException)
                        {
                            AxCryptException ace = ae.InnerException as AxCryptException;
                            New<IReport>().Exception(ace);
                            return new FileOperationContext(ace.DisplayContext.Default(file), ace.InnerException?.Message, ace.ErrorStatus);
                        }
                        catch (Exception ex)
                        {
                            New<IReport>().Exception(ex.InnerException);
                            return new FileOperationContext(file.ToString(), ex.InnerException.Message, ErrorStatus.Exception);
                        }
                    },
                    (fileOperationContext) =>
                    {
                        if (fileOperationContext.ErrorStatus != ErrorStatus.Success)
                        {
                            result = fileOperationContext;
                        }
                    }
                );
                progress.NotifyLevelFinished();
                return Task.FromResult(result);
            },
            (FileOperationContext status) =>
            {
                allComplete(status);
            },
            groupProgress).Free();
        }
    }
}