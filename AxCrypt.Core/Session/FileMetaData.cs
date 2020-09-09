using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Session
{
    public class FileMetaData
    {
        public FileMetaData(string fileName)
        {
            FileName = fileName;
            DateTime utcNow = New<INow>().Utc;
            CreationTimeUtc = utcNow;
            LastAccessTimeUtc = utcNow;
            LastWriteTimeUtc = utcNow;
        }

        public string FileName { get; private set; }

        public DateTime CreationTimeUtc { get; set; }

        public DateTime LastAccessTimeUtc { get; set; }

        public DateTime LastWriteTimeUtc { get; set; }
    }
}
