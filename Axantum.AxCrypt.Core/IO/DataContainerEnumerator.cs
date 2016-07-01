using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.IO
{
    public class DataContainerEnumerator : IEnumerable<IDataStore>
    {
        private IEnumerable<IDataContainer> _containers;

        public bool Cancel { get; set; }

        public DataContainerEnumerator(IEnumerable<IDataContainer> containers)
        {
            _containers = containers;
        }

        public IEnumerator<IDataStore> GetEnumerator()
        {
            foreach (IDataStore store in EnumerateFolders(_containers))
            {
                yield return store;
            }
        }

        private IEnumerable<IDataStore> EnumerateFolders(IEnumerable<IDataContainer> containers)
        {
            foreach (IDataContainer container in containers)
            {
                foreach (IDataStore store in container.Files)
                {
                    if (Cancel)
                    {
                        yield break;
                    }
                    yield return store;
                }
                foreach (IDataStore store in EnumerateFolders(container.Folders))
                {
                    yield return store;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}