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
using System.Linq;
using System.Collections.Generic;
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

        // <summary>
        /// Does an operation on a list of files, with parallelism.
        /// </summary>
        /// <param name="files">The files to operation on.</param>
        /// <param name="work">The work to do for each file.</param>
        /// <param name="allComplete">The completion callback after *all* files have been processed.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual Task DoFilesAsync<TDataItem>(IEnumerable<TDataItem> files, Func<TDataItem, IProgressContext, Task<FileOperationContext>> work, Func<FileOperationContext, Task> allComplete)
            where TDataItem : IDataItem
        {
            Func<TDataItem, IProgressContext, Task<FileOperationContext>> singleFileOperation = (file, progress) =>
            {
                progress.Display = file.Name;
                return work((TDataItem)file, progress);
            };

            return InvokeAsync(files, singleFileOperation, allComplete);
        }

        /// <summary>
        /// Does an operation on a list of files, with parallelism.
        /// </summary>
        /// <param name="files">The files to operation on.</param>
        /// <param name="workAsync">The work to do for each file.</param>
        /// <param name="allCompleteAsync">The completion callback after *all* files have been processed.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        private async Task InvokeAsync<T>(IEnumerable<T> files, Func<T, IProgressContext, Task<FileOperationContext>> workAsync, Func<FileOperationContext, Task> allCompleteAsync)
        {
            WorkerGroupProgressContext groupProgress = new WorkerGroupProgressContext(new CancelProgressContext(new ProgressContext()), New<ISingleThread>());
            await New<IProgressBackground>().WorkAsync(nameof(DoFilesAsync),
                async (IProgressContext progress) =>
                {
                    progress.NotifyLevelStart();

                    FileOperationContext result = new FileOperationContext(string.Empty, ErrorStatus.Success);

                    result = await Task.Run(async () =>
                    {
                        foreach (T file in files)
                        {
                            try
                            {
                                result = await workAsync(file, progress);
                            }
                            catch (Exception ex) when (ex is OperationCanceledException)
                            {
                                return new FileOperationContext(file.ToString(), ErrorStatus.Canceled);
                            }
                            catch (Exception ex) when (ex is AxCryptException)
                            {
                                AxCryptException ace = ex as AxCryptException;
                                New<IReport>().Exception(ace);
                                return new FileOperationContext(ace.DisplayContext.Default(file), ace.InnerException?.Message ?? ace.Message, ace.ErrorStatus);
                            }
                            catch (Exception ex)
                            {
                                New<IReport>().Exception(ex);
                                return new FileOperationContext(file.ToString(), ex.Message, ErrorStatus.Exception);
                            }
                            if (result.ErrorStatus != ErrorStatus.Success)
                            {
                                return result;
                            }
                        }
                        return result;
                    });
                    progress.NotifyLevelFinished();
                    return result;
                },
                (FileOperationContext status) => allCompleteAsync(status),
                groupProgress).Free();
        }
    }
}