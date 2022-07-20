using System.Collections.Generic;
using System.Linq;

#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Content;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using System;
using System.IO;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Session
{
    public static class ActiveFileExtensions
    {
        /// <summary>
        /// Checks if it's time to update a decrypted file, and does it if so.
        /// </summary>
        /// <param name="activeFile">The active file.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>The possibly updated ActiveFile</returns>
        /// <exception cref="System.ArgumentNullException">activeFile</exception>
        public static async Task<ActiveFile> CheckUpdateDecrypted(this ActiveFile activeFile, FileLock encryptedFileLock, FileLock decryptedFileLock, IProgressContext progress)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }

            bool shouldUpgradeEncryption = activeFile.Properties.CryptoId.ShouldUpgradeEncryption();
            if (!shouldUpgradeEncryption && !activeFile.IsModified)
            {
                return activeFile;
            }

            if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.EditExistingFiles))
            {
                return activeFile;
            }

            if (activeFile.DecryptedFileInfo.IsLocked())
            {
                if (New<ILogging>().IsWarningEnabled)
                {
                    New<ILogging>().LogWarning("Failed exclusive open modified for '{0}'.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                return new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NotShareable);
            }

            bool wasWriteProteced = encryptedFileLock.DataStore.IsWriteProtected;
            if (wasWriteProteced)
            {
                encryptedFileLock.DataStore.IsWriteProtected = false;
            }

            try
            {
                await New<AxCryptFile>().EncryptToFileWithBackupAsync(encryptedFileLock, async (Stream destination) =>
                {
                    if (shouldUpgradeEncryption)
                    {
                        activeFile = new ActiveFile(activeFile, New<CryptoFactory>().Default(New<ICryptoPolicy>()).CryptoId);
                    }
                    if (shouldUpgradeEncryption || activeFile.Identity == LogOnIdentity.Empty)
                    {
                        activeFile = new ActiveFile(activeFile, New<KnownIdentities>().DefaultEncryptionIdentity);
                    }

                    if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.StrongerEncryption))
                    {
                        activeFile = new ActiveFile(activeFile, New<CryptoFactory>().Default(New<ICryptoPolicy>()).CryptoId, New<KnownIdentities>().DefaultEncryptionIdentity);
                    }

                    EncryptionParameters parameters = new EncryptionParameters(activeFile.Properties.CryptoId, activeFile.Identity);

                    EncryptedProperties properties = EncryptedProperties.Create(encryptedFileLock.DataStore);
                    bool isDecryptedWithMasterKey = false;
                    if (properties.DecryptionParameter != null && properties.DecryptionParameter.PrivateKey != null)
                    {
                        isDecryptedWithMasterKey = properties.DecryptionParameter.PrivateKey.Equals(activeFile.Identity.GetPrivateMasterKey());
                    }

                    if (isDecryptedWithMasterKey)
                    {
                        parameters = new EncryptionParameters(activeFile.Properties.CryptoId, AxCryptFile.GenerateRandomPassword());
                    }

                    await AddSharingParameters(parameters, activeFile, properties, isDecryptedWithMasterKey);

                    New<AxCryptFile>().Encrypt(activeFile.DecryptedFileInfo, destination, parameters, AxCryptOptions.EncryptWithCompression, progress);
                }, progress);
            }
            finally
            {
                if (wasWriteProteced)
                {
                    encryptedFileLock.DataStore.IsWriteProtected = wasWriteProteced;
                }
            }

            if (New<ILogging>().IsInfoEnabled)
            {
                New<ILogging>().LogInfo("Wrote back '{0}' to '{1}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName, activeFile.EncryptedFileInfo.FullName));
            }
            return new ActiveFile(activeFile, activeFile.DecryptedFileInfo.LastWriteTimeUtc, ActiveFileStatus.AssumedOpenAndDecrypted);
        }

        private static async Task AddSharingParameters(EncryptionParameters parameters, ActiveFile activeFile, EncryptedProperties properties, bool isDecryptedWithMasterKey)
        {
            await AddMasterKeyParameters(parameters, activeFile);

            if (!activeFile.IsShared && !isDecryptedWithMasterKey)
            {
                return;
            }

            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.KeySharing))
            {
                await parameters.AddAsync(properties.SharedKeyHolders);
                return;
            }

            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, Texts.KeySharingRemovedInFreeModeWarningText, Common.DoNotShowAgainOptions.KeySharingRemovedInFreeModeWarning);
        }

        private static async Task AddMasterKeyParameters(EncryptionParameters parameters, ActiveFile activeFile)
        {
            LogOnIdentity logOnIdentity = New<KnownIdentities>().DefaultEncryptionIdentity;
            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.Business) && logOnIdentity.MasterKeyPair != null)
            {
                IAsymmetricPublicKey publicKey = New<IAsymmetricFactory>().CreatePublicKey(logOnIdentity.MasterKeyPair.PublicKey);
                parameters.PublicMasterKey = new UserPublicKey(logOnIdentity.UserEmail, publicKey);
                return;
            }

            if (activeFile.IsMasterKeyShared)
            {
                await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, Texts.MasterKeyRemovedWarningText, Common.DoNotShowAgainOptions.MasterKeyRemovedWarning);
            }
        }

        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string Size(this ActiveFile activeFile, int decimalPlaces = 2)
        {
            if (decimalPlaces < 0)
            {
                throw new ArgumentOutOfRangeException("decimalPlaces");
            }
            long fileLength = activeFile.EncryptedFileInfo.Length();
            if (fileLength < 0)
            {
                return "-" + activeFile.Size(decimalPlaces);
            }
            if (fileLength == 0)
            {
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(fileLength, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag)
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)fileLength / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}