using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Fake
{
    public class FakeKnownFoldersDiscovery : IKnownFoldersDiscovery
    {
        public IEnumerable<KnownFolder> Discover()
        {
            return new KnownFolder[] { new KnownFolder(new FakeDataContainer(@"C:\Users\AxCrypt\My Documents"), "My FakeAxCrypt", KnownFolderKind.WindowsMyDocuments, new Uri("http://localhost")), };
        }
    }
}
