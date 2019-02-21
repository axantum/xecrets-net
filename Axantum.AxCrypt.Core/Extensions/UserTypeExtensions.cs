using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class UserTypeExtensions
    {
        public static string ToFileString(this PublicKeyThumbprint thumbprint)
        {
            if (thumbprint == null)
            {
                throw new ArgumentNullException("thumbprint");
            }

            string base64 = Convert.ToBase64String(thumbprint.ToByteArray());
            string fileString = base64.Substring(0, base64.Length - 2).Replace('/', '-');

            return fileString;
        }

        public static PublicKeyThumbprint ToPublicKeyThumbprint(this string thumbprint)
        {
            if (thumbprint == null)
            {
                throw new ArgumentNullException("thumbprint");
            }
            if (thumbprint.Length != 22)
            {
                throw new ArgumentException("Length must be 128 bits base 64 without padding.", "thumbprint");
            }
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(thumbprint.Replace('-', '/') + "==");
            }
            catch (FormatException fex)
            {
                throw new ArgumentException("Incorrect base64 encoding.", "thumbprint", fex);
            }

            return new PublicKeyThumbprint(bytes);
        }

        /// <summary>
        /// Convert the internal representation of a key pair to the external account key representation.
        /// </summary>
        /// <param name="keys">The key pair.</param>
        /// <param name="passphrase">The passphrase to encrypt it with.</param>
        /// <returns>A representation suitable for serialization and external storage.</returns>
        public static AccountKey ToAccountKey(this UserKeyPair keys, Passphrase passphrase)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            string encryptedPrivateKey = EncryptPrivateKey(keys, passphrase);

            KeyPair keyPair = new KeyPair(keys.KeyPair.PublicKey.ToString(), encryptedPrivateKey);
            AccountKey accountKey = new AccountKey(keys.UserEmail.Address, keys.KeyPair.PublicKey.Thumbprint.ToString(), keyPair, keys.Timestamp, PrivateKeyStatus.PassphraseKnown);

            return accountKey;
        }

        private static string EncryptPrivateKey(UserKeyPair keys, Passphrase passphrase)
        {
            if (keys.KeyPair.PrivateKey == null)
            {
                return String.Empty;
            }

            byte[] privateKeyPemBytes = Encoding.UTF8.GetBytes(keys.KeyPair.PrivateKey.ToString());

            if (passphrase == Passphrase.Empty)
            {
                byte[] encryptedPrivateKeyBytes = New<IProtectedData>().Protect(privateKeyPemBytes, null);
                return Convert.ToBase64String(encryptedPrivateKeyBytes);
            }

            StringBuilder encryptedPrivateKey = new StringBuilder();
            using (StringWriter writer = new StringWriter(encryptedPrivateKey))
            {
                using (Stream stream = new MemoryStream(privateKeyPemBytes))
                {
                    EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Preferred.CryptoId, passphrase);
                    EncryptedProperties properties = new EncryptedProperties("private-key.pem");
                    using (MemoryStream encryptedStream = new MemoryStream())
                    {
                        AxCryptFile.Encrypt(stream, encryptedStream, properties, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());
                        writer.Write(Convert.ToBase64String(encryptedStream.ToArray()));
                    }
                }
            }
            return encryptedPrivateKey.ToString();
        }

        /// <summary>
        /// Convert an external representation of a key-pair to an internal representation that is suitable for actual use.
        /// </summary>
        /// <param name="accountKey">The account key.</param>
        /// <param name="passphrase">The passphrase to decrypt the private key, if any, with.</param>
        /// <returns>A UserKeyPair or null if it was not possible to decrypt it.</returns>
        public static UserKeyPair ToUserKeyPair(this Api.Model.AccountKey accountKey, Passphrase passphrase)
        {
            if (accountKey == null)
            {
                throw new ArgumentNullException(nameof(accountKey));
            }

            string privateKeyPem = DecryptPrivateKeyPem(accountKey.KeyPair.PrivateEncryptedPem, passphrase);
            if (privateKeyPem == null)
            {
                return null;
            }

            IAsymmetricKeyPair keyPair = Resolve.AsymmetricFactory.CreateKeyPair(accountKey.KeyPair.PublicPem, privateKeyPem);
            UserKeyPair userAsymmetricKeys = new UserKeyPair(EmailAddress.Parse(accountKey.User), accountKey.Timestamp, keyPair);

            return userAsymmetricKeys;
        }

        /// <summary>
        /// Convert an external representation of a public key to an internal representation suitable for actual use.
        /// </summary>
        /// <param name="accountKey">The account key.</param>
        /// <returns>A UserPublicKey</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static UserPublicKey ToUserPublicKey(this Api.Model.AccountKey accountKey)
        {
            if (accountKey == null)
            {
                throw new ArgumentNullException(nameof(accountKey));
            }

            IAsymmetricPublicKey publicKey = Resolve.AsymmetricFactory.CreatePublicKey(accountKey.KeyPair.PublicPem);
            return new UserPublicKey(EmailAddress.Parse(accountKey.User), publicKey);
        }

        /// <summary>
        /// Convert an internal representation of a public key to a serializable external representation.
        /// </summary>
        /// <param name="userPublicKey">The user public key.</param>
        /// <returns>An AccountKey instance without a private key component.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static AccountKey ToAccountKey(this UserPublicKey userPublicKey)
        {
            if (userPublicKey == null)
            {
                throw new ArgumentNullException(nameof(userPublicKey));
            }

            AccountKey accountKey = new AccountKey(userPublicKey.Email.Address, userPublicKey.PublicKey.Thumbprint.ToString(), new KeyPair(userPublicKey.PublicKey.ToString(), String.Empty), New<INow>().Utc, PrivateKeyStatus.Empty);
            return accountKey;
        }

        private static string DecryptPrivateKeyPem(string privateEncryptedPem, Passphrase passphrase)
        {
            if (privateEncryptedPem.Length == 0)
            {
                return String.Empty;
            }

            byte[] privateKeyEncryptedPem = Convert.FromBase64String(privateEncryptedPem);

            byte[] decryptedPrivateKeyBytes = New<IProtectedData>().Unprotect(privateKeyEncryptedPem, null);
            if (decryptedPrivateKeyBytes != null)
            {
                return Encoding.UTF8.GetString(decryptedPrivateKeyBytes, 0, decryptedPrivateKeyBytes.Length);
            }
            if (passphrase == Passphrase.Empty)
            {
                return null;
            }

            using (MemoryStream encryptedPrivateKeyStream = new MemoryStream(privateKeyEncryptedPem))
            {
                using (MemoryStream decryptedPrivateKeyStream = new MemoryStream())
                {
                    DecryptionParameter decryptionParameter = new DecryptionParameter(passphrase, Resolve.CryptoFactory.Preferred.CryptoId);
                    try
                    {
                        if (!New<AxCryptFile>().Decrypt(encryptedPrivateKeyStream, decryptedPrivateKeyStream, new DecryptionParameter[] { decryptionParameter }).IsValid)
                        {
                            return null;
                        }
                    }
                    catch (FileFormatException ffex)
                    {
                        New<IReport>().Exception(ffex);
                        return null;
                    }

                    return Encoding.UTF8.GetString(decryptedPrivateKeyStream.ToArray(), 0, (int)decryptedPrivateKeyStream.Length);
                }
            }
        }

        public static RestIdentity ToRestIdentity(this LogOnIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            return new RestIdentity(identity.UserEmail.Address, identity.Passphrase.Text);
        }

        public static Task<SubscriptionLevel> ValidatedLevelAsync(this UserAccount userAccount)
        {
            return new LicenseValidation().ValidateLevelAsync(userAccount);
        }

        public static UserAccount MergeWith(this UserAccount highPriorityAccount, UserAccount lowPriorityAccount)
        {
            if (highPriorityAccount == null)
            {
                throw new ArgumentNullException(nameof(highPriorityAccount));
            }
            if (lowPriorityAccount == null)
            {
                throw new ArgumentNullException(nameof(lowPriorityAccount));
            }

            return highPriorityAccount.MergeWith(lowPriorityAccount.AccountKeys);
        }

        public static UserAccount MergeWith(this UserAccount highPriorityAccount, IEnumerable<AccountKey> lowPriorityAccountKeys)
        {
            if (highPriorityAccount == null)
            {
                throw new ArgumentNullException(nameof(highPriorityAccount));
            }
            if (lowPriorityAccountKeys == null)
            {
                throw new ArgumentNullException(nameof(lowPriorityAccountKeys));
            }

            IEnumerable<AccountKey> allKeys = new List<AccountKey>(highPriorityAccount.AccountKeys);
            IEnumerable<AccountKey> newKeys = lowPriorityAccountKeys.Where(lak => !allKeys.Any(ak => ak.KeyPair.PublicPem == lak.KeyPair.PublicPem));
            IEnumerable<AccountKey> unionOfKeys = allKeys.Union(newKeys);
            UserAccount merged = new UserAccount(highPriorityAccount.UserName, highPriorityAccount.SubscriptionLevel, highPriorityAccount.LevelExpiration, highPriorityAccount.AccountStatus, highPriorityAccount.Offers, unionOfKeys)
            {
                Tag = highPriorityAccount.Tag,
                Signature = highPriorityAccount.Signature,
                AccountSource = highPriorityAccount.AccountSource,
            };
            return merged;
        }

        public static UserAccount MergeWith(this IEnumerable<AccountKey> highPriorityAccountKeys, UserAccount lowPriorityAccount)
        {
            if (lowPriorityAccount == null)
            {
                throw new ArgumentNullException(nameof(lowPriorityAccount));
            }

            UserAccount highPriorityAccount = new UserAccount(lowPriorityAccount.UserName, lowPriorityAccount.SubscriptionLevel, lowPriorityAccount.LevelExpiration, lowPriorityAccount.AccountStatus, lowPriorityAccount.Offers, highPriorityAccountKeys)
            {
                Tag = lowPriorityAccount.Tag,
                Signature = lowPriorityAccount.Signature,
            };
            return highPriorityAccount.MergeWith(lowPriorityAccount.AccountKeys);
        }

        public static IEnumerable<WatchedFolder> ToWatchedFolders(this IEnumerable<string> folderPaths)
        {
            IEnumerable<WatchedFolder> watched = Resolve.FileSystemState.WatchedFolders.Where((wf) => folderPaths.Contains(wf.Path));

            return watched;
        }

        public static IEnumerable<EmailAddress> SharedWith(this IEnumerable<WatchedFolder> watchedFolders)
        {
            IEnumerable<EmailAddress> sharedWithEmailAddresses = watchedFolders.SelectMany(wf => wf.KeyShares).Distinct();

            return sharedWithEmailAddresses;
        }

        public static WatchedFolder FindOrDefault(this IEnumerable<WatchedFolder> watchedFolders, IDataStore fileMaybeWatched)
        {
            string fileFolderPath = fileMaybeWatched.Container.FullName;
            IEnumerable<WatchedFolder> candidates = watchedFolders.Where(watchedFolder => fileFolderPath.StartsWith(watchedFolder.Path)).OrderBy(watchedFolder => watchedFolder.Path.Length);
            if (New<UserSettings>().FolderOperationMode.Policy() == FolderOperationMode.SingleFolder)
            {
                return candidates.FirstOrDefault();
            }
            return candidates.LastOrDefault();
        }

        public static async Task ShowPopup(this AccountTip tip)
        {
            string title;
            switch (tip.Level)
            {
                case StartupTipLevel.Information:
                    title = Texts.InformationTitle;
                    break;

                case StartupTipLevel.Warning:
                    title = Texts.WarningTitle;
                    break;

                case StartupTipLevel.Critical:
                    title = Texts.WarningTitle;
                    break;

                default:
                    throw new InvalidOperationException("Unexpected tip level.");
            }

            PopupButtons clicked;
            switch (tip.ButtonStyle)
            {
                case StartupTipButtonStyle.YesNo:
                    clicked = await New<IPopup>().ShowAsync(PopupButtons.OkCancel, title, tip.Message);
                    break;

                case StartupTipButtonStyle.Ok:
                    clicked = await New<IPopup>().ShowAsync(PopupButtons.Ok, title, tip.Message);
                    break;

                default:
                    clicked = await New<IPopup>().ShowAsync(PopupButtons.Ok, title, tip.Message);
                    break;
            }

            if (clicked != PopupButtons.Ok)
            {
                return;
            }
            if (tip.Url == null)
            {
                return;
            }
            New<ILauncher>().Launch(tip.Url.ToString());
        }

        public static FolderOperationMode Policy(this FolderOperationMode operationMode)
        {
            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.IncludeSubfolders))
            {
                return operationMode;
            }
            else
            {
                return FolderOperationMode.SingleFolder;
            }
        }

        private static readonly EmailAddress _licenseAuthorityEmail = EmailAddress.Parse(New<UserSettings>().LicenseAuthorityEmail);

        public static async Task<UserPublicKey> GetAsync(this KnownPublicKeys knownPublicKeys, EmailAddress email, LogOnIdentity identity)
        {
            UserPublicKey key = knownPublicKeys.PublicKeys.FirstOrDefault(upk => upk.Email == email);
            if (key != null && New<UserPublicKeyUpdateStatus>().Status(key) == PublicKeyUpdateStatus.RecentlyUpdated)
            {
                return key;
            }

            if (New<AxCryptOnlineState>().IsOffline)
            {
                return key;
            }

            if (identity == LogOnIdentity.Empty || identity.UserEmail == EmailAddress.Empty)
            {
                return key;
            }

            IAccountService accountService = New<LogOnIdentity, IAccountService>(identity);
            if (await accountService.IsAccountSourceLocalAsync())
            {
                return key;
            }

            if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.KeySharing) && email != _licenseAuthorityEmail)
            {
                return key;
            }

            AccountStorage accountStorage = new AccountStorage(New<LogOnIdentity, IAccountService>(identity));
            UserPublicKey userPublicKey = await accountStorage.GetOtherUserPublicKeyAsync(email).Free();

            if (userPublicKey != null)
            {
                knownPublicKeys.AddOrReplace(userPublicKey);
                New<UserPublicKeyUpdateStatus>().SetStatus(userPublicKey, PublicKeyUpdateStatus.RecentlyUpdated);
            }
            return userPublicKey;
        }

        public static async Task<IEnumerable<UserPublicKey>> GetKnownPublicKeysAsync(this IEnumerable<UserPublicKey> publicKeys, LogOnIdentity identity)
        {
            List<UserPublicKey> knownKeys = new List<UserPublicKey>();
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                foreach (UserPublicKey publicKey in publicKeys)
                {
                    UserPublicKey key = await knownPublicKeys.GetAsync(publicKey.Email, identity);
                    if (key == null)
                    {
                        knownKeys.Add(publicKey);
                        continue;
                    }

                    knownKeys.Add(key);
                }
            }
            return knownKeys;
        }

        public static async Task<IEnumerable<UserPublicKey>> ToKnownPublicKeysAsync(this IEnumerable<EmailAddress> emails, LogOnIdentity identity)
        {
            List<UserPublicKey> knownKeys = new List<UserPublicKey>();
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                foreach (EmailAddress email in emails)
                {
                    UserPublicKey key = await knownPublicKeys.GetAsync(email, identity);
                    if (key == null)
                    {
                        continue;
                    }

                    knownKeys.Add(key);
                }
            }
            return knownKeys;
        }

        public static async Task ChangeKeySharingAsync(this IEnumerable<string> files, IEnumerable<UserPublicKey> publicKeys)
        {
            EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default(New<ICryptoPolicy>()).CryptoId, New<KnownIdentities>().DefaultEncryptionIdentity);
            await encryptionParameters.AddAsync(await publicKeys.GetKnownPublicKeysAsync(New<KnownIdentities>().DefaultEncryptionIdentity));
            await ChangeEncryptionAsync(files, encryptionParameters);
        }

        private static Task ChangeEncryptionAsync(IEnumerable<string> files, EncryptionParameters encryptionParameters)
        {
            return Resolve.ParallelFileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)),
                async (IDataStore file, IProgressContext progress) =>
                {
                    await New<AxCryptFile>().ChangeEncryptionAsync(file, Resolve.KnownIdentities.DefaultEncryptionIdentity, encryptionParameters, progress);
                    return new FileOperationContext(file.FullName, ErrorStatus.Success);
                },
                async (FileOperationContext foc) =>
                {
                    if (foc.ErrorStatus != ErrorStatus.Success)
                    {
                        New<IStatusChecker>().CheckStatusAndShowMessage(foc.ErrorStatus, foc.FullName, foc.InternalMessage);
                        return;
                    }

                    await Resolve.SessionNotify.NotifyAsync(new SessionNotification(SessionNotificationType.ActiveFileChange));
                });
        }

        public static bool IsDisplayEquivalentTo(this IEnumerable<ActiveFile> left, IEnumerable<ActiveFile> right)
        {
            if (left.Count() != right.Count())
            {
                return false;
            }
            IEnumerator<ActiveFile> rightEnumerator = right.GetEnumerator();
            foreach (ActiveFile leftActiveFile in left)
            {
                rightEnumerator.MoveNext();
                ActiveFile rightActiveFile = rightEnumerator.Current;

                if (!leftActiveFile.IsDisplayEquivalentTo(rightActiveFile))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsDisplayEquivalentTo(this ActiveFile left, ActiveFile right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return true;
            }
            if (left.Properties != right.Properties)
            {
                return false;
            }
            if (left.Status != right.Status)
            {
                return false;
            }
            if (left.DecryptedFileInfo.FullName != right.DecryptedFileInfo.FullName)
            {
                return false;
            }
            if (left.EncryptedFileInfo.FullName != right.EncryptedFileInfo.FullName)
            {
                return false;
            }
            if (left.IsShared != right.IsShared)
            {
                return false;
            }

            return true;
        }

        public static void ShowNotification(this ProgressTotals progressTotals)
        {
            if (New<UserSettings>().LongOperationThreshold == TimeSpan.Zero)
            {
                return;
            }

            if (progressTotals.Elapsed >= New<UserSettings>().LongOperationThreshold)
            {
                TimeSpan wholeSeconds = TimeSpan.FromSeconds(Math.Round(progressTotals.Elapsed.TotalSeconds));
                string formattedTime = wholeSeconds.ToString("g", CultureInfo.CurrentCulture);
                New<IGlobalNotification>().ShowTransient(Texts.AxCryptFileEncryption, string.Format(Texts.ProgressTotalsInformationText, progressTotals.NumberOfFiles, formattedTime));
            }
        }
    }
}