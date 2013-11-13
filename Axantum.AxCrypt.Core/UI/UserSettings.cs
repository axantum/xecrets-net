using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class UserSettings
    {
        public UserSettings()
        {
            Defaults(new StreamingContext());
        }

        [OnDeserializing]
        private void Defaults(StreamingContext context)
        {
            CultureName = "en-US";
            AxCrypt2VersionCheckUrl = new Uri("https://www.axantum.com/Xecrets/RestApi.ashx/axcrypt2version/windows");
            UpdateUrl = new Uri("http://www.axantum.com/");
            AxCrypt2HelpUrl = new Uri("http://www.axantum.com/AxCrypt/AxCryptNetHelp.html");
            DisplayEncryptPassphrase = true;
            RecentFilesMaxNumber = 250;
            RecentFilesAscending = true;
            NewestKnownVersion = String.Empty;
        }

        [DataMember(Name = "CultureName")]
        public string CultureName { get; set; }

        [DataMember(Name = "AxCrypt2VersionCheckUrl")]
        public Uri AxCrypt2VersionCheckUrl { get; set; }

        [DataMember(Name = "UpdateUrl")]
        public Uri UpdateUrl { get; set; }

        [DataMember(Name = "LastUpdateCheckUtc")]
        public DateTime LastUpdateCheckUtc { get; set; }

        [DataMember(Name = "NewestKnownVersion")]
        public string NewestKnownVersion { get; set; }

        [DataMember(Name = "DebugMode")]
        public bool DebugMode { get; set; }

        [DataMember(Name = "AxCrypt2HelpUrl")]
        public Uri AxCrypt2HelpUrl { get; set; }

        [DataMember(Name = "DisplayEncryptPassphrase")]
        public bool DisplayEncryptPassphrase { get; set; }

        [DataMember(Name = "DisplayDecryptPassphrase")]
        public bool DisplayDecryptPassphrase { get; set; }

        [DataMember(Name = "MainWindowWidth")]
        public int MainWindowWidth { get; set; }

        [DataMember(Name = "MainWindowHeight")]
        public int MainWindowHeight { get; set; }

        [DataMember(Name = "MainWindowLocationX")]
        public int MainWindowLocationX { get; set; }

        [DataMember(Name = "MainWindowLocationY")]
        public int MainWindowLocationY { get; set; }

        [DataMember(Name = "RecentFilesMaxNumber")]
        public int RecentFilesMaxNumber { get; set; }

        [DataMember(Name = "RecentFilesDocumentWidth")]
        public int RecentFilesDocumentWidth { get; set; }

        [DataMember(Name = "RecentFilesDocumentHeight")]
        public int RecentFilesDocumentHeight { get; set; }

        [DataMember(Name = "RecentFilesDateTimeWidth")]
        public int RecentFilesDateTimeWidth { get; set; }

        [DataMember(Name = "RecentFilesEncryptedPathWidth")]
        public int RecentFilesEncryptedPathWidth { get; set; }

        [DataMember(Name = "RecentFilesAscending")]
        public bool RecentFilesAscending { get; set; }

        [DataMember(Name = "RecentFilesSortColumn")]
        public int RecentFilesSortColumn { get; set; }
    }
}