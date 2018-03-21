using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using AxCrypt.Content;
using System;
using System.IO;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class ExceptionExtensions
    {
        public static bool IsFileOrDirectoryNotFound(this Exception ex)
        {
            if (ex is FileNotFoundException)
            {
                return true;
            }

            return IsDirectoryNotFound(ex);
        }

        private static bool IsDirectoryNotFound(this Exception ex)
        {
            string typeName = ex.GetType().FullName;
            if (typeName == "System.IO.DirectoryNotFoundException")
            {
                return true;
            }

            return false;
        }

        public static void ReportAndDisplay(this Exception ex)
        {
            New<IReport>().Exception(ex);
            AxCryptException aex = ex as AxCryptException;
            if (aex != null)
            {
                New<IStatusChecker>().CheckStatusAndShowMessage(aex.ErrorStatus, aex.DisplayContext, aex.Message);
                return;
            }
            New<IStatusChecker>().CheckStatusAndShowMessage(ErrorStatus.Exception, ex?.Message ?? "(null)", Texts.Exception.InvariantFormat("unknown"));
        }

        public static void RethrowFileOperation(this Exception ex, string displayContext)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            AxCryptException aex = ex as AxCryptException;
            if (aex != null)
            {
                throw new FileOperationException(aex.Message, displayContext, aex.ErrorStatus, ex);
            }
            throw new FileOperationException(ex.Message, displayContext, ErrorStatus.Exception, ex);
        }

        public static void HandleApiExceptions(this ApiException aex)
        {
            New<IReport>().Exception(aex);
            New<AxCryptOnlineState>().IsOffline = true;
            if (aex is OfflineApiException)
            {
                Texts.OfflineApiExceptionDialogText.ShowWarning(Texts.WarningTitle);
                throw aex;
            }

            return;
        }
    }
}