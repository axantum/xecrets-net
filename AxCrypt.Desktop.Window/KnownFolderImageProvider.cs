using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using AxCrypt.Desktop.Window.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Desktop.Window
{
    public class KnownFolderImageProvider : IKnownFolderImageProvider
    {
        public object GetImage(KnownFolderKind folderKind)
        {
            switch (folderKind)
            {
                case KnownFolderKind.OneDrive:
                    return Resources.skydrive_40px;

                case KnownFolderKind.GoogleDrive:
                    return Resources.google_drive_40px;

                case KnownFolderKind.Dropbox:
                    return Resources.dropbox_40px;

                case KnownFolderKind.WindowsMyDocuments:
                    return Resources.documents_white_40px;

                case KnownFolderKind.None:
                case KnownFolderKind.ICloud:
                case KnownFolderKind.MacDocumentsLibrary:
                default:
                    throw new InternalErrorException("Unexpected known folder kind.", folderKind.ToString());
            }
        }
    }
}