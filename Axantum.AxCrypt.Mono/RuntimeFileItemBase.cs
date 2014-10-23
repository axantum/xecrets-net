using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono
{
    public abstract class RuntimeFileItemBase
    {
        public RuntimeFileItemBase()
        {
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

        public virtual string FullName
        {
            get { return Location; }
        }

        public abstract void Delete();
    }
}