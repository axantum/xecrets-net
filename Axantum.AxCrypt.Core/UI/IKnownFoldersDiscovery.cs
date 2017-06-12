using Axantum.AxCrypt.Core.UI;
using System.Collections.Generic;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IKnownFoldersDiscovery
    {
        IEnumerable<KnownFolder> Discover();
    }
}