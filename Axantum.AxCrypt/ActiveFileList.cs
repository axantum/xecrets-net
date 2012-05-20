using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt
{
    [CollectionDataContract(ItemName = "FileState", Namespace = "http://wwww.axantum.com/Serialization/")]
    public class ActiveFileList : List<ActiveFile>
    {
        public ActiveFileList()
            : base()
        {
        }

        public ActiveFileList(IEnumerable<ActiveFile> collection)
            : base(collection)
        {
        }
    }
}