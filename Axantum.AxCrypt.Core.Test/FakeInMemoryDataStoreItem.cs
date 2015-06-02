using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeInMemoryDataStoreItem : IDataStore
    {
        private MemoryStream _dataStream;

        private string _fileName;

        public FakeInMemoryDataStoreItem(string fileName)
        {
            _fileName = fileName;
            _dataStream = new MemoryStream();
            IsAvailable = true;
            CreationTimeUtc = LastAccessTimeUtc = LastWriteTimeUtc = new DateTime(2015, 06, 02).ToUniversalTime();
        }

        public Stream OpenRead()
        {
            _dataStream.Position = 0;
            return new NonClosingStream(_dataStream);
        }

        public virtual Stream OpenWrite()
        {
            _dataStream.Position = 0;
            return new NonClosingStream(_dataStream);
        }

        public bool IsLocked
        {
            get;
            set;
        }

        public DateTime CreationTimeUtc
        {
            get;
            set;
        }

        public DateTime LastAccessTimeUtc
        {
            get;
            set;
        }

        public DateTime LastWriteTimeUtc
        {
            get;
            set;
        }

        public void SetFileTimes(DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc)
        {
            CreationTimeUtc = creationTimeUtc;
            LastAccessTimeUtc = lastAccessTimeUtc;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public void MoveTo(string destinationFileName)
        {
            _fileName = destinationFileName;
        }

        public IDataContainer Container
        {
            get { return null; }
        }

        public bool IsAvailable
        {
            get;
            set;
        }

        public bool IsFile
        {
            get { return true; }
        }

        public bool IsFolder
        {
            get { return false; }
        }

        public string Name
        {
            get { return _fileName; }
        }

        public string FullName
        {
            get { return _fileName; }
        }

        public void Delete()
        {
            IsAvailable = false;
        }
    }
}