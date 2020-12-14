using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Rest;
using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Masterkey;
using AxCrypt.Common;
using AxCrypt.Content;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.IO;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Extensions
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

            return EncryptPrivateKey(passphrase, keys.KeyPair.PrivateKey.ToString(), null);
        }

        public static string EncryptPrivateKey(this Passphrase passphrase, string privateKey, IAsymmetricPublicKey publicKey)
        {
            byte[] privateKeyPemBytes = Encoding.UTF8.GetBytes(privateKey);

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

        public static string EncryptPrivateKey(this IAsymmetricPublicKey publicKey, string privateKey, string adminUser, Passphrase randomPassphrase)
        {
            byte[] privateKeyPemBytes = Encoding.UTF8.GetBytes(privateKey);

            if (publicKey == null)
            {
                return string.Empty;
            }

            StringBuilder encryptedPrivateKey = new StringBuilder();
            using (StringWriter writer = new StringWriter(encryptedPrivateKey))
            {
                using (Stream stream = new MemoryStream(privateKeyPemBytes))
                {
                    EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Preferred.CryptoId, randomPassphrase);
                    if (adminUser.Length > 0)
                    {
                        encryptionParameters.AddAsync(new List<UserPublicKey>() { new UserPublicKey(EmailAddress.Parse(adminUser), publicKey) });
                    }
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

        public static string DecryptPrivateKeyPem(this string privateEncryptedPem, Passphrase passphrase)
        {
            return DecryptPrivateKeyPem(privateEncryptedPem, passphrase, null);
        }

        public static string DecryptPrivateKeyPem(this string privateEncryptedPem, Passphrase passphrase, IAsymmetricPrivateKey privateKey)
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
            if (passphrase == Passphrase.Empty && privateKey == null)
            {
                return null;
            }

            using (MemoryStream encryptedPrivateKeyStream = new MemoryStream(privateKeyEncryptedPem))
            {
                using (MemoryStream decryptedPrivateKeyStream = new MemoryStream())
                {
                    DecryptionParameter decryptionParameter = new DecryptionParameter(passphrase, Resolve.CryptoFactory.Preferred.CryptoId);
                    if (privateKey != null)
                    {
                        decryptionParameter = new DecryptionParameter(privateKey, Resolve.CryptoFactory.Preferred.CryptoId);
                    }
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

        public static IEnumerable<DecryptionParameter> DecryptionParameters(this LogOnIdentity identity)
        {
            IEnumerable<DecryptionParameter> decryptionParameters = DecryptionParameter.CreateAll(new Passphrase[] { identity.Passphrase }, GetPrivateKeys(identity), Resolve.CryptoFactory.OrderedIds);
            return decryptionParameters;
        }

        public static IList<IAsymmetricPrivateKey> GetPrivateKeys(this LogOnIdentity identity)
        {
            IList<IAsymmetricPrivateKey> privateKeys = identity.PrivateKeys.ToList();
            IAsymmetricPrivateKey masterPrivateKey = identity.GetPrivateMasterKey();
            if (masterPrivateKey != null)
            {
                privateKeys.Add(masterPrivateKey);
            }

            return privateKeys;
        }

        public static IAsymmetricPrivateKey GetPrivateMasterKey(this LogOnIdentity identity)
        {
            if (identity.MasterKeyPair == null || identity.MasterKeyPair.PrivateKeys == null)
            {
                return null;
            }

            PrivateMasterKeyInfo privateKeyInfo = identity.MasterKeyPair.PrivateKeys.FirstOrDefault(pk => pk.UserEmail == identity.UserEmail.Address);
            if (string.IsNullOrEmpty(privateKeyInfo?.PrivateKey))
            {
                return null;
            }

            IAsymmetricPrivateKey privateKey = GetDecryptedMasterPrivateKey(privateKeyInfo, identity);
            if (privateKey != null)
            {
                return privateKey;
            }

            return null;
        }

        private static IAsymmetricPrivateKey GetDecryptedMasterPrivateKey(PrivateMasterKeyInfo privateKeyInfo, LogOnIdentity identity)
        {
            string masterKeyPrivateKeyPem = privateKeyInfo.PrivateKey.DecryptPrivateKeyPem(Passphrase.Empty, identity.ActiveEncryptionKeyPair.KeyPair.PrivateKey);
            if (string.IsNullOrEmpty(masterKeyPrivateKeyPem))
            {
                return null;
            }

            return New<IAsymmetricFactory>().CreatePrivateKey(masterKeyPrivateKeyPem);
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

            return highPriorityAccount.MergeWith(lowPriorityAccount.AccountKeys, lowPriorityAccount.MasterKeyPair);
        }

        public static UserAccount MergeWith(this UserAccount highPriorityAccount, IEnumerable<AccountKey> lowPriorityAccountKeys, MasterKeyPairInfo lowPriorityMasterKeyPair)
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

            MasterKeyPairInfo mergedMasterKeyPair = highPriorityAccount.MasterKeyPair;
            if (highPriorityAccount.MasterKeyPair != null)
            {
                mergedMasterKeyPair = MergeMasterKey(highPriorityAccount, lowPriorityMasterKeyPair);
            }

            UserAccount merged = new UserAccount(highPriorityAccount.UserName, highPriorityAccount.SubscriptionLevel, highPriorityAccount.LevelExpiration, highPriorityAccount.AccountStatus, highPriorityAccount.Offers, unionOfKeys)
            {
                Tag = highPriorityAccount.Tag,
                Signature = highPriorityAccount.Signature,
                AccountSource = highPriorityAccount.AccountSource,
                CanTryAppStorePremiumTrial = highPriorityAccount.CanTryAppStorePremiumTrial,
                ActiveSubscriptionFromAppStore = highPriorityAccount.ActiveSubscriptionFromAppStore,
                MasterKeyPair = highPriorityAccount.IsMasterKeyEnabled ? mergedMasterKeyPair : null,
                IsMasterKeyEnabled = highPriorityAccount.IsMasterKeyEnabled,
            };
            return merged;
        }

        private static MasterKeyPairInfo MergeMasterKey(UserAccount highPriorityAccount, MasterKeyPairInfo lowPriorityMasterKeyPair)
        {
            if (!highPriorityAccount.IsMasterKeyEnabled)
            {
                return null;
            }

            MasterKeyPairInfo mergedMasterKeyPairInfo = new MasterKeyPairInfo()
            {
                Timestamp = highPriorityAccount.MasterKeyPair.Timestamp,
                PublicKey = highPriorityAccount.MasterKeyPair.PublicKey,
                Thumbprint = highPriorityAccount.MasterKeyPair.Thumbprint,
                PrivateKeys = highPriorityAccount.MasterKeyPair.PrivateKeys,
            };

            return mergedMasterKeyPairInfo;
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
                MasterKeyPair = lowPriorityAccount.MasterKeyPair,
            };
            return highPriorityAccount.MergeWith(lowPriorityAccount.AccountKeys, lowPriorityAccount.MasterKeyPair);
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
            CustomMessageParameters invitationMessageParameters = new CustomMessageParameters(new CultureInfo(New<UserSettings>().MessageCulture), New<UserSettings>().CustomInvitationMessage);
            UserPublicKey userPublicKey = await accountStorage.GetOtherUserInvitePublicKeyAsync(email, invitationMessageParameters).Free();

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

        public static async Task<IEnumerable<UserPublicKey>> ToAvailableKnownPublicKeysAsync(this IEnumerable<EmailAddress> emails, LogOnIdentity identity)
        {
            List<UserPublicKey> availablePublicKeys = new List<UserPublicKey>();
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                foreach (EmailAddress email in emails)
                {
                    UserPublicKey key = await knownPublicKeys.GetAsync(email, identity);
                    if (key != null)
                    {
                        availablePublicKeys.Add(key);
                    }
                }
            }
            return availablePublicKeys;
        }

        public static async Task ChangeKeySharingAsync(this IEnumerable<string> files, IEnumerable<UserPublicKey> publicKeys)
        {
            LogOnIdentity logOnIdentity = New<KnownIdentities>().DefaultEncryptionIdentity;
            EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Default(New<ICryptoPolicy>()).CryptoId, logOnIdentity);
            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.Business))
            {
                encryptionParameters = await AddMasterKeyParameter(logOnIdentity, encryptionParameters, false);
            }
            await encryptionParameters.AddAsync(await publicKeys.GetKnownPublicKeysAsync(logOnIdentity));
            await ChangeEncryptionAsync(files, encryptionParameters);
        }

        public static async Task<EncryptionParameters> AddMasterKeyParameter(this LogOnIdentity logOnIdentity, EncryptionParameters encryptionParameters, bool isShowMasterKeyWarning)
        {
            if (logOnIdentity.MasterKeyPair != null && isShowMasterKeyWarning)
            {
                await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, Texts.MasterKeyInfoPopupText, DoNotShowAgainOptions.MasterKeyWarning);
            }

            if (logOnIdentity.MasterKeyPair != null)
            {
                IAsymmetricPublicKey publicKey = New<IAsymmetricFactory>().CreatePublicKey(logOnIdentity.MasterKeyPair.PublicKey);
                encryptionParameters.PublicMasterKey = new UserPublicKey(logOnIdentity.UserEmail, publicKey);
            }

            return encryptionParameters;
        }

        private static Task ChangeEncryptionAsync(IEnumerable<string> files, EncryptionParameters encryptionParameters)
        {
            return Resolve.ParallelFileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)),
                async (IDataStore file, IProgressContext progress) =>
                {
                    ActiveFile activeFile = New<FileSystemState>().FindActiveFileFromEncryptedPath(file.FullName);
                    LogOnIdentity decryptIdentity = activeFile?.Identity ?? New<KnownIdentities>().DefaultEncryptionIdentity;

                    EncryptedProperties encryptedProperties = EncryptedProperties.Create(file, decryptIdentity);

                    bool isDecryptedWithMasterKey = false;
                    if (encryptedProperties.DecryptionParameter != null && encryptedProperties.DecryptionParameter.PrivateKey != null)
                    {
                        isDecryptedWithMasterKey = encryptedProperties.DecryptionParameter.PrivateKey.Equals(New<KnownIdentities>().DefaultEncryptionIdentity.GetPrivateMasterKey());
                    }

                    if (isDecryptedWithMasterKey)
                    {
                        EncryptionParameters newEncryptionParameters = new EncryptionParameters(encryptionParameters.CryptoId, AxCryptFile.GenerateRandomPassword());
                        await newEncryptionParameters.AddAsync(encryptionParameters.PublicKeys.Where(pk => pk.Email != New<KnownIdentities>().DefaultEncryptionIdentity.UserEmail));
                        newEncryptionParameters.PublicMasterKey = encryptionParameters.PublicMasterKey;

                        encryptionParameters = newEncryptionParameters;
                    }

                    await New<AxCryptFile>().ChangeEncryptionAsync(file, decryptIdentity, encryptionParameters, progress);

                    if (activeFile != null)
                    {
                        New<FileSystemState>().Add(new ActiveFile(activeFile, encryptionParameters.CryptoId, New<KnownIdentities>().DefaultEncryptionIdentity));
                        await New<FileSystemState>().Save();
                    }

                    return new FileOperationContext(file.FullName, ErrorStatus.Success);
                },
                async (FileOperationContext foc) =>
                {
                    if (foc.ErrorStatus != ErrorStatus.Success)
                    {
                        New<IStatusChecker>().CheckStatusAndShowMessage(foc.ErrorStatus, foc.FullName, foc.InternalMessage);
                        return;
                    }
                    await Resolve.SessionNotify.NotifyAsync(new SessionNotification(SessionNotificationType.ActiveFileChange, files));
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
            if (left.IsMasterKeyShared != right.IsMasterKeyShared)
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

        public static async Task<AccountStatus> GetValidEmailAccountStatusAsync(this EmailAddress validEmail, LogOnIdentity identity)
        {
            if (validEmail == null)
            {
                throw new ArgumentNullException(nameof(validEmail));
            }

            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            IAccountService accountService = New<LogOnIdentity, IAccountService>(identity);
            if (await accountService.IsAccountSourceLocalAsync())
            {
                Texts.AccountServiceLocalExceptionDialogText.ShowWarning(Texts.WarningTitle);
                return AccountStatus.Unknown;
            }

            AccountStorage accountStorage = new AccountStorage(accountService);
            return await accountStorage.StatusAsync(validEmail).Free();
        }
    }
}