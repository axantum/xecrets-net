using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Presentation
{
    public static class DragEventArgsExtensions
    {
        public static IEnumerable<IRuntimeFileInfo> GetDragged(this DragEventArgs e)
        {
            IList<string> dropped = e.Data.GetData(DataFormats.FileDrop) as IList<string>;
            if (dropped == null)
            {
                return new IRuntimeFileInfo[0];
            }

            return dropped.Select(path => OS.Current.FileInfo(path));
        }

        public static IEnumerable<IRuntimeFileInfo> Encryptable(this IEnumerable<IRuntimeFileInfo> files)
        {
            return files.Where(fileInfo => Instance.KnownKeys.DefaultEncryptionKey != null && fileInfo.Type() == FileInfoType.EncryptableFile);
        }

        public static IEnumerable<IRuntimeFileInfo> Encrypted(this IEnumerable<IRuntimeFileInfo> files)
        {
            return files.Where(fileInfo => fileInfo.Type() == FileInfoType.EncryptedFile);
        }

        public static bool HasEncryptable(this IEnumerable<IRuntimeFileInfo> files)
        {
            return files.Any(fileInfo => fileInfo.Type() == FileInfoType.EncryptableFile);
        }

        public static bool HasEncrypted(this IEnumerable<IRuntimeFileInfo> files)
        {
            return files.Any(fileInfo => fileInfo.Type() == FileInfoType.EncryptedFile);
        }
    }
}