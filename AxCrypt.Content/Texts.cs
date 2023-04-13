using Xecrets.File.Content.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Content
{
    public class Texts : Resources
    {
        private static readonly Lazy<ResourceManager> _resourceManager = new Lazy<ResourceManager>(() => new ContentResourceManager());

        public static new ResourceManager ResourceManager { get { return _resourceManager.Value; } }
    }
}
