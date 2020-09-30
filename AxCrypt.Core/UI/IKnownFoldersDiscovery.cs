using AxCrypt.Core.UI;
using System.Collections.Generic;

namespace AxCrypt.Core.UI
{
    public interface IKnownFoldersDiscovery
    {
        IEnumerable<KnownFolder> Discover();
    }
}