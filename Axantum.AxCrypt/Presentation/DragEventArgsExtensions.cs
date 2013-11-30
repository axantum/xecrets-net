using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Presentation
{
    public static class DragEventArgsExtensions
    {
        public static IEnumerable<string> GetDragged(this DragEventArgs e)
        {
            IList<string> dropped = e.Data.GetData(DataFormats.FileDrop) as IList<string>;
            if (dropped == null)
            {
                return new string[0];
            }

            return dropped;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable")]
        public static IEnumerable<IRuntimeFileInfo> Encryptable(this IEnumerable<IRuntimeFileInfo> files)
        {
            return files.Where(fileInfo => Instance.KnownKeys.DefaultEncryptionKey != null && fileInfo.Type() == FileInfoTypes.EncryptableFile);
        }

        public static IEnumerable<IRuntimeFileInfo> Encrypted(this IEnumerable<IRuntimeFileInfo> files)
        {
            return files.Where(fileInfo => fileInfo.Type() == FileInfoTypes.EncryptedFile);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable")]
        public static bool HasEncryptable(this IEnumerable<IRuntimeFileInfo> files)
        {
            return files.Any(fileInfo => fileInfo.Type() == FileInfoTypes.EncryptableFile);
        }

        public static bool HasEncrypted(this IEnumerable<IRuntimeFileInfo> files)
        {
            return files.Any(fileInfo => fileInfo.Type() == FileInfoTypes.EncryptedFile);
        }
    }
}