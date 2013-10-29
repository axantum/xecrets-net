using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.Session
{
    public class SessionEventArgs : EventArgs
    {
        public AesKey Key { get; private set; }

        public IEnumerable<IRuntimeFileInfo> FileInfos { get; private set; }

        public SessionEvent SessionEvent { get; private set; }

        public SessionEventArgs(SessionEvent sessionEvent, AesKey key, IEnumerable<IRuntimeFileInfo> fileInfos)
        {
            Key = key;
            FileInfos = fileInfos;
            SessionEvent = sessionEvent;
        }

        public SessionEventArgs(SessionEvent sessionEvent, string path)
            : this(sessionEvent, null, path)
        {
        }

        public SessionEventArgs(SessionEvent sessionEvent, AesKey key, string path)
            :this(sessionEvent, key, new IRuntimeFileInfo[]{OS.Current.FileInfo(path)})
        {
        }

        public SessionEventArgs(SessionEvent sessionEvent)
            : this(sessionEvent, null, new IRuntimeFileInfo[0])
        {
        }
    }
}
