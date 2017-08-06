using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    [Flags]
    public enum DontShowAgain
    {
        None = 0x0,
        FileAssociationBrokenWarning = 0x1,
        LavasoftWebCompanionExistenceWarning = 0x2,
        TryPremium = 0x4,
        SignedInSoNoPasswordRequired = 0x8,
    }
}