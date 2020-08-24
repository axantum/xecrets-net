using System;
using System.Collections.Generic;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    public enum SKVerifyReceiptStatus
    {
        ValidReceipt = 0,
        RequestNotMade = 21000,

        //21001 This status code is no longer sent by the App Store
        ReceiptDataMalformedOrMissing = 21002,

        ReceiptNotAuthenticated = 21003,
        SharedSecretDoesNotMatch = 21004,
        ReceiptServerNotAvailableNow = 21005,
        ValidReceiptSubscriptionExpired = 21006,
        SandboxReceiptInProductionForVerification = 21007,
        ProductionReceiptInSandboxForVerification = 21008,
        InternalDataAccessError = 21009,
        UserAccountDoesNotExists = 21010,
    }
}