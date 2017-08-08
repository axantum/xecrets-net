using System;
using System.Collections.Generic;
using System.Linq;
using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.UI;
using AxCrypt.Content;

namespace Axantum.AxCrypt
{
    internal class StatusCheck : IStatusChecker
    {
        public bool CheckStatusAndShowMessage(ErrorStatus status, string displayContext, string message)
        {
            switch (status)
            {
                case ErrorStatus.Success:
                    return true;

                case ErrorStatus.UnspecifiedError:
                    Texts.FileOperationFailed.InvariantFormat(displayContext).ShowWarning(Texts.MessageUnexpectedErrorTitle);
                    break;

                case ErrorStatus.FileAlreadyExists:
                    Texts.FileAlreadyExists.InvariantFormat(displayContext).ShowWarning(Texts.WarningTitle);
                    break;

                case ErrorStatus.FileDoesNotExist:
                    Texts.FileDoesNotExist.InvariantFormat(displayContext).ShowWarning(Texts.WarningTitle);
                    break;

                case ErrorStatus.CannotWriteDestination:
                    Texts.CannotWrite.InvariantFormat(displayContext).ShowWarning(Texts.WarningTitle);
                    break;

                case ErrorStatus.CannotStartApplication:
                    Texts.CannotStartApplication.InvariantFormat(displayContext).ShowWarning(Texts.MessageUnexpectedErrorTitle);
                    break;

                case ErrorStatus.InconsistentState:
                    Texts.InconsistentState.InvariantFormat(displayContext).ShowWarning(Texts.MessageUnexpectedErrorTitle);
                    break;

                case ErrorStatus.InvalidKey:
                    Texts.InvalidKey.InvariantFormat(displayContext).ShowWarning(Texts.MessageUnexpectedErrorTitle);
                    break;

                case ErrorStatus.Canceled:
                    break;

                case ErrorStatus.Exception:
                    string msg = Texts.Exception.InvariantFormat(displayContext);
                    if (!string.IsNullOrEmpty(message))
                    {
                        msg = "{0} [{1}]".InvariantFormat(msg, message);
                    }
                    msg.ShowWarning(Texts.MessageUnexpectedErrorTitle);
                    break;

                case ErrorStatus.InvalidPath:
                    Texts.InvalidPath.InvariantFormat(displayContext).ShowWarning(Texts.MessageUnexpectedErrorTitle);
                    break;

                case ErrorStatus.FolderAlreadyWatched:
                    Texts.FolderAlreadyWatched.InvariantFormat(displayContext).ShowWarning(Texts.MessageUnexpectedErrorTitle);
                    break;

                case ErrorStatus.FileLocked:
                    Texts.FileIsLockedWarning.InvariantFormat(displayContext).ShowWarning(Texts.WarningTitle);
                    break;

                case ErrorStatus.FileWriteProtected:
                    Texts.FileIsWriteProtectedWarning.InvariantFormat(displayContext).ShowWarning(Texts.WarningTitle);
                    break;

                case ErrorStatus.WrongFileExtensionError:
                    Texts.WrongFileExtensionWarning.InvariantFormat(displayContext, OS.Current.AxCryptExtension).ShowWarning(Texts.WarningTitle);
                    break;

                case ErrorStatus.Unknown:
                    Texts.UnknownFileStatus.InvariantFormat(displayContext).ShowWarning(Texts.MessageUnexpectedErrorTitle);
                    break;

                case ErrorStatus.Working:
                    Texts.WorkingFileStatus.InvariantFormat(displayContext).ShowWarning(Texts.MessageUnexpectedErrorTitle);
                    break;

                case ErrorStatus.Aborted:
                    Texts.AbortedFileStatus.InvariantFormat(displayContext).ShowWarning(Texts.MessageUnexpectedErrorTitle);
                    break;

                case ErrorStatus.FileAlreadyEncrypted:
                    Texts.FileAlreadyEncryptedStatus.InvariantFormat(displayContext).ShowWarning(Texts.WarningTitle);
                    break;

                case ErrorStatus.MagicGuidMissing:
                    Texts.MagicGuidMIssingFileStatus.InvariantFormat(displayContext).ShowWarning(Texts.WarningTitle);
                    break;

                default:
                    Texts.UnrecognizedError.InvariantFormat(displayContext, status).ShowWarning(Texts.MessageUnexpectedErrorTitle);
                    break;
            }
            return false;
        }
    }
}