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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Session
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
        public static ActiveFile CheckUpdateDecrypted(this ActiveFile activeFile, FileLock encryptedFileLock, FileLock decryptedFileLock, IProgressContext progress)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }

            bool shouldConvertLegacy = activeFile.ShouldConvertLegacy();
            if (!shouldConvertLegacy && !activeFile.IsModified)
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
                New<AxCryptFile>().WriteToFileWithBackup(encryptedFileLock, (Stream destination) =>
                {
                    if (!IsLegacy(activeFile) || shouldConvertLegacy)
                    {
                        activeFile = new ActiveFile(activeFile, New<CryptoFactory>().Default(New<ICryptoPolicy>()).CryptoId);
                    }
                    if (shouldConvertLegacy)
                    {
                        activeFile = new ActiveFile(activeFile, New<KnownIdentities>().DefaultEncryptionIdentity);
                    }

                    EncryptionParameters parameters = new EncryptionParameters(activeFile.Properties.CryptoId, activeFile.Identity);
                    EncryptedProperties properties = EncryptedProperties.Create(encryptedFileLock.DataStore);
                    parameters.Add(properties.SharedKeyHolders);

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

        private static bool IsLegacy(ActiveFile activeFile)
        {
            return activeFile.Properties.CryptoId == new V1Aes128CryptoFactory().CryptoId;
        }

        public static bool ShouldConvertLegacy(this ActiveFile activeFile)
        {
            if (!IsLegacy(activeFile))
            {
                return false;
            }

            if (New<UserSettings>().LegacyConversionMode != LegacyConversionMode.AutoConvertLegacyFiles)
            {
                return false;
            }

            if (!New<KnownIdentities>().IsLoggedOn)
            {
                return false;
            }

            return true;
        }
    }
}