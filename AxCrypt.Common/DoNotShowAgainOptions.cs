using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Common
{
    [Flags]
    public enum DoNotShowAgainOptions
    {
        None = 0x0,
        FileAssociationBrokenWarning = 0x1,
        LavasoftWebCompanionExistenceWarning = 0x2,
        TryPremium = 0x4,
        SignedInSoNoPasswordRequired = 0x8,
        WillNotForgetPassword = 0x10,
        IgnoreFileWarning = 0x20,
        UnopenableFileWarning = 0x40,
        KeySharingRemovedInFreeModeWarning = 0x80,
        MasterKeyWarning = 0x100,
        MasterKeyRemovedWarning = 0x200,
    }
}