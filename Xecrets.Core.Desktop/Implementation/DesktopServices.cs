#region Coypright and GPL License

/*
 * Xecrets.Net - Copyright © 2022-2026, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets.Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets.Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets.Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
 */

#endregion Coypright and GPL License

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using AxCrypt.Core;
using AxCrypt.Core.IO;
using AxCrypt.Core.UI;

using static AxCrypt.Abstractions.TypeResolve;

using ProgressEventArgs = AxCrypt.Core.UI.ProgressEventArgs;

namespace Xecrets.Core.Desktop.Implementation;

internal sealed partial class DesktopServices : IDesktopServices
{
    public IFile StandardIoFile(string path) => new FileWrapper(New<IStandardIoDataStore>(path));

    public IFile File(string path) => new FileWrapper(New<IDataStore>(path));

    public IFile FindFree(string fullPath, bool overwrite)
    {
        IFile candidateFile = StandardIoFile(fullPath);
        if (overwrite || candidateFile.IsStdout)
        {
            return candidateFile;
        }

        int i = 0;
        while (candidateFile.IsAvailable)
        {
            string path = Path.GetDirectoryName(fullPath) ?? string.Empty;
            string fileName = Path.GetFileName(fullPath);
            string extension = Path.GetExtension(fileName);
            string fileNameWithoutExtension = fileName[..^extension.Length];
            string fileNameWithoutExtensionAndNumber = TrailingNumberInParenthesis().Replace(fileNameWithoutExtension, string.Empty);
            string candidateFreeFullPath = Path.Combine(path, $"{fileNameWithoutExtensionAndNumber} ({++i}){extension}");
            candidateFile = StandardIoFile(candidateFreeFullPath);
        }
        return candidateFile;
    }

    public bool CanReadFromFile(IFile file, [NotNullWhen(false)] out string? reason) =>
        New<IFileVerify>().CanReadFromFile(UnwrapStandardIoFile(file), out reason);

    public bool CanWriteToFile(IFile file) => New<IFileVerify>().CanWriteToFile(UnwrapStandardIoFile(file));

    public bool CanWriteToFolder(IFolder folder) => New<IFileVerify>().CanWriteToFolder(UnwrapContainer(folder));

    private static IStandardIoDataStore UnwrapStandardIoFile(IFile file)
    {
        return file is FileWrapper { Wrapped: IStandardIoDataStore standardIoFile }
            ? standardIoFile
            : throw new ArgumentException(@"Unsupported standard IO file wrapper.", nameof(file));
    }

    private static IDataContainer UnwrapContainer(IFolder folder)
    {
        return folder is FolderWrapper { Wrapped: IDataContainer dataContainer }
            ? dataContainer
            : throw new ArgumentException(@"Unsupported container wrapper.", nameof(folder));
    }

    private const int ErrorSharingViolation = 32;

    public Task<bool> WipeAsync(string path, IProgress<Progress> progress)
    {
        try
        {
            IDataStore dataStore = New<IDataStore>(path);
            using FileLock fileLock = New<FileLocker>().Acquire(dataStore);

            // The design of the Wipe() method is unfortunate, and causes the complication with
            // the progress levels etc. Should either be rewritten in the original code base or
            // just reimplemented independently in a more flexible way.
            New<AxCryptFile>().Wipe(fileLock, new ProgressContextAdapter(progress));
            return Task.FromResult(true);
        }
        catch (IOException ioex) when ((ioex.HResult & 0xFFFF) == ErrorSharingViolation)
        {
            return Task.FromResult(false);
        }
    }

    private sealed class ProgressContextAdapter(IProgress<Progress> progress) : IProgressContext
    {
        public string Display { get; set; } = string.Empty;

        public event EventHandler<ProgressEventArgs> Progressing
        {
            add { }
            remove { }
        }

        public bool Cancel { get; set; }

        public bool AllItemsConfirmed { get; set; }

        public ProgressTotals Totals { get; } = new();

        public void RemoveCount(long totalCount, long progressCount) { }

        public void AddTotal(long count) => progress.Report(Progress.TotalAdded(count));

        public void AddCount(long count)
        {
            if (Cancel)
            {
                throw new OperationCanceledException("Operation canceled by code.");
            }

            progress.Report(Progress.CountAdded(count));
        }

        public void NotifyLevelStart() => progress.Report(Progress.LevelStarted());

        public void NotifyLevelFinished() => progress.Report(Progress.LevelFinished());

        public Task EnterSingleThread() => Task.CompletedTask;

        public void LeaveSingleThread()
        {
        }
    }

    [GeneratedRegex(@" \([\d]+\)$")]
    private static partial Regex TrailingNumberInParenthesis();
}
