using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    /// <summary>
    /// Increasing this value, should cause software of this level or higher to reject and clean all user settings for the user,
    /// essentially like a new installation. This will be inconvenient for users, so use with care! It must be registered as a
    /// singleton, and may be overridden in platform specific versions.
    /// </summary>
    public class UserSettingsVersion
    {
        public virtual int Current
        {
            get
            {
                return 10;
            }
        }
    }
}