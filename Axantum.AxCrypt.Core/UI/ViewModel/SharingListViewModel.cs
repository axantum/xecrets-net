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

using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    /// <summary>
    /// Edit a list of public sharing keys, adding and removing
    /// </summary>
    public class SharingListViewModel : ViewModelBase
    {
        private LogOnIdentity _logOnIdentity;

        public IEnumerable<UserPublicKey> SharedWith { get { return GetProperty<IEnumerable<UserPublicKey>>(nameof(SharedWith)); } private set { SetProperty(nameof(SharedWith), value); } }

        public IEnumerable<UserPublicKey> NotSharedWith { get { return GetProperty<IEnumerable<UserPublicKey>>(nameof(NotSharedWith)); } private set { SetProperty(nameof(NotSharedWith), value); } }

        public string NewKeyShare { get { return GetProperty<string>(nameof(NewKeyShare)); } set { SetProperty(nameof(NewKeyShare), value); } }

        public bool CanAddNewKey { get { return GetProperty<bool>(nameof(CanAddNewKey)); } set { SetProperty(nameof(CanAddNewKey), value); } }

        public IAsyncAction AsyncAddKeyShares { get; private set; }

        public IAsyncAction AsyncRemoveKeyShares { get; private set; }

        public IAsyncAction AsyncAddNewKeyShare { get; private set; }

        private Task _missingKeysLoader;

        public SharingListViewModel(IEnumerable<UserPublicKey> sharedWith, LogOnIdentity logOnIdentity)
        {
            _logOnIdentity = logOnIdentity ?? LogOnIdentity.Empty;

            _missingKeysLoader = Task.Run(() => TryAddMissingUnsharedPublicKeysFromServerAsync(sharedWith.Select(sw => sw.Email), sharedWith));

            InitializePropertyValues(sharedWith);
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        public async Task ReadyAsync()
        {
            await _missingKeysLoader;
        }

        private void InitializePropertyValues(IEnumerable<UserPublicKey> sharedWith)
        {
            EmailAddress userEmail = _logOnIdentity.UserKeys.UserEmail;
            SharedWith = sharedWith.Where(sw => sw.Email != userEmail).OrderBy(e => e.Email.Address).ToList();

            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                NotSharedWith = knownPublicKeys.PublicKeys.Where(upk => upk.Email != userEmail && !sharedWith.Any(sw => upk.Email == sw.Email)).OrderBy(e => e.Email.Address);
            }
            NewKeyShare = String.Empty;
            CanAddNewKey = New<AxCryptOnlineState>().IsOnline;

            AsyncAddKeyShares = new AsyncDelegateAction<IEnumerable<EmailAddress>>(async (upks) => await AddKeySharesActionAsync(upks));
            AsyncRemoveKeyShares = new AsyncDelegateAction<IEnumerable<UserPublicKey>>(async (upks) => await RemoveKeySharesActionAsync(upks));
            AsyncAddNewKeyShare = new AsyncDelegateAction<string>(async (email) => await AddNewKeyShareActionAsync(email), (email) => Task.FromResult(this[nameof(NewKeyShare)].Length == 0));
        }

        private static void BindPropertyChangedEvents()
        {
        }

        private static void SubscribeToModelEvents()
        {
        }

        private async Task RemoveKeySharesActionAsync(IEnumerable<UserPublicKey> keySharesToRemove)
        {
            await ReadyAsync();

            HashSet<UserPublicKey> fromSet = new HashSet<UserPublicKey>(SharedWith, UserPublicKey.EmailComparer);
            HashSet<UserPublicKey> toSet = new HashSet<UserPublicKey>(NotSharedWith, UserPublicKey.EmailComparer);

            MoveKeyShares(keySharesToRemove, fromSet, toSet);

            SharedWith = fromSet.OrderBy(a => a.Email.Address);
            NotSharedWith = toSet.OrderBy(a => a.Email.Address);
        }

        private async Task AddKeySharesActionAsync(IEnumerable<EmailAddress> keySharesToAdd)
        {
            await ReadyAsync();

            IEnumerable<UserPublicKey> publicKeysToAdd = await TryAddMissingUnsharedPublicKeysFromServerAsync(keySharesToAdd, NotSharedWith).Free();

            HashSet<UserPublicKey> fromSet = new HashSet<UserPublicKey>(NotSharedWith, UserPublicKey.EmailComparer);
            HashSet<UserPublicKey> toSet = new HashSet<UserPublicKey>(SharedWith, UserPublicKey.EmailComparer);

            MoveKeyShares(publicKeysToAdd, fromSet, toSet);

            NotSharedWith = fromSet.OrderBy(a => a.Email.Address);
            SharedWith = toSet.OrderBy(a => a.Email.Address);
        }

        private async Task AddNewKeyShareActionAsync(string email)
        {
            await ReadyAsync();
            await AddKeySharesActionAsync(new EmailAddress[] { EmailAddress.Parse(email), }).Free();
        }

        private async Task<IEnumerable<UserPublicKey>> TryAddMissingUnsharedPublicKeysFromServerAsync(IEnumerable<EmailAddress> keySharesToAdd, IEnumerable<UserPublicKey> fileShares)
        {
            List<UserPublicKey> publicKeys = new List<UserPublicKey>();
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                foreach (EmailAddress keyShareToAdd in keySharesToAdd)
                {
                    UserPublicKey key = await knownPublicKeys.GetAsync(keyShareToAdd);
                    if (key != null)
                    {
                        publicKeys.Add(key);
                    }
                }
            }
            return publicKeys;
        }

        private static void MoveKeyShares(IEnumerable<UserPublicKey> keySharesToMove, HashSet<UserPublicKey> fromSet, HashSet<UserPublicKey> toSet)
        {
            foreach (UserPublicKey keyShareToMove in keySharesToMove)
            {
                if (fromSet.Contains(keyShareToMove))
                {
                    fromSet.Remove(keyShareToMove);
                }
                toSet.Add(keyShareToMove);
            }
        }

        protected override Task<bool> ValidateAsync(string columnName)
        {
            return Task.FromResult(ValidateInternal(columnName));
        }

        private bool ValidateInternal(string columnName)
        {
            switch (columnName)
            {
                case nameof(NewKeyShare):
                    if (!NewKeyShare.IsValidEmail())
                    {
                        ValidationError = (int)ViewModel.ValidationError.InvalidEmail;
                        return false;
                    }
                    return true;
            }
            return false;
        }
    }
}