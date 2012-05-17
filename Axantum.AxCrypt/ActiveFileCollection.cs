using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt
{
    [CollectionDataContract(ItemName = "FileState", KeyName = "DecryptedPath", Namespace = "http://wwww.axantum.com/Serialization/", ValueName = "ActiveFile")]
    public class ActiveFileCollection : Dictionary<string, ActiveFile>
    {
    }
}