using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt
{
    [CollectionDataContract(ItemName = "FileState", KeyName = "DecryptedPath", Namespace = "http://wwww.axantum.com/Serialization/", ValueName = "ActiveFile")]
    public class ActiveFileDictionary : Dictionary<string, ActiveFile>
    {
        public ActiveFileDictionary()
            : base()
        {
        }

        protected ActiveFileDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}