using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono
{
    public abstract class DataItem : IDataItem
    {
        public static IDataItem Create(string location)
        {
            if (File.GetAttributes(location).HasFlag(FileAttributes.Directory))
            {
                return new DataContainer(location);
            }
            return new DataStore(location);
        }

        protected virtual string Location { get; set; }

        public abstract bool IsAvailable
        {
            get;
        }

        public abstract bool IsFile
        {
            get;
        }

        public abstract bool IsFolder
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public abstract string FullName
        {
            get;
        }

        public abstract void Delete();

        public IDataContainer Container
        {
            get
            {
                return new DataContainer(Path.GetDirectoryName(Location));
            }
        }
    }
}