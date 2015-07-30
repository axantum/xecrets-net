#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    /// <summary>
    /// Edit a list of public sharing keys, adding and removing
    /// </summary>
    public class SharingListViewModel : ViewModelBase
    {
        private Func<KnownPublicKeys> _knownPublicKeysFactory;

        private LogOnIdentity _logOnIdentity;

        public IEnumerable<UserPublicKey> SharedWith { get { return GetProperty<IEnumerable<UserPublicKey>>("SharedWith"); } private set { SetProperty("SharedWith", value); } }

        public IEnumerable<UserPublicKey> NotSharedWith { get { return GetProperty<IEnumerable<UserPublicKey>>("NotSharedWith"); } private set { SetProperty("NotSharedWith", value); } }

        public IAction AddKeyShares { get; private set; }

        public IAction RemoveKeyShares { get; private set; }

        public SharingListViewModel(Func<KnownPublicKeys> knownPublicKeysFactory, IEnumerable<UserPublicKey> sharedWith, LogOnIdentity logOnIdentity)
        {
            _knownPublicKeysFactory = knownPublicKeysFactory;
            _logOnIdentity = logOnIdentity ?? LogOnIdentity.Empty;

            InitializePropertyValues(sharedWith);
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        private void InitializePropertyValues(IEnumerable<UserPublicKey> sharedWith)
        {
            EmailAddress userEmail = _logOnIdentity.UserKeys.UserEmail;
            SharedWith = sharedWith.Where(sw => sw.Email != userEmail).ToList();

            using (KnownPublicKeys knownPublicKeys = _knownPublicKeysFactory())
            {
                NotSharedWith = knownPublicKeys.PublicKeys.Where(upk => upk.Email != userEmail && !sharedWith.Any(sw => upk.Email == sw.Email)).OrderBy(e => e.Email.Address);
            }

            AddKeyShares = new DelegateAction<IEnumerable<EmailAddress>>((upks) => AddKeySharesAction(upks));
            RemoveKeyShares = new DelegateAction<IEnumerable<UserPublicKey>>((upks) => RemoveKeySharesAction(upks));
        }

        private static void BindPropertyChangedEvents()
        {
        }

        private static void SubscribeToModelEvents()
        {
        }

        private void RemoveKeySharesAction(IEnumerable<UserPublicKey> keySharesToRemove)
        {
            HashSet<UserPublicKey> fromSet = new HashSet<UserPublicKey>(SharedWith, UserPublicKey.EmailComparer);
            HashSet<UserPublicKey> toSet = new HashSet<UserPublicKey>(NotSharedWith, UserPublicKey.EmailComparer);

            MoveKeyShares(keySharesToRemove, fromSet, toSet);

            SharedWith = fromSet.OrderBy(a => a.Email.Address);
            NotSharedWith = toSet.OrderBy(a => a.Email.Address);
        }

        private void AddKeySharesAction(IEnumerable<EmailAddress> keySharesToAdd)
        {
            IEnumerable<UserPublicKey> publicKeysToAdd = TryAddMissingUnsharedPublicKeysFromServer(keySharesToAdd);

            HashSet<UserPublicKey> fromSet = new HashSet<UserPublicKey>(NotSharedWith, UserPublicKey.EmailComparer);
            HashSet<UserPublicKey> toSet = new HashSet<UserPublicKey>(SharedWith, UserPublicKey.EmailComparer);

            MoveKeyShares(publicKeysToAdd, fromSet, toSet);

            NotSharedWith = fromSet.OrderBy(a => a.Email.Address);
            SharedWith = toSet.OrderBy(a => a.Email.Address);
        }

        private IEnumerable<UserPublicKey> TryAddMissingUnsharedPublicKeysFromServer(IEnumerable<EmailAddress> keySharesToAdd)
        {
            List<UserPublicKey> publicKeys = new List<UserPublicKey>();
            using (KnownPublicKeys knownPublicKeys = _knownPublicKeysFactory())
            {
                foreach (EmailAddress keyShareToAdd in keySharesToAdd)
                {
                    UserPublicKey userPublicKey = knownPublicKeys.PublicKeys.FirstOrDefault(pk => pk.Email == keyShareToAdd);
                    if (userPublicKey == null)
                    {
                        continue;
                    }
                    publicKeys.Add(userPublicKey);
                }
            }
            return publicKeys;
        }

        private static void MoveKeyShares(IEnumerable<UserPublicKey> keySharesToMove, HashSet<UserPublicKey> fromSet, HashSet<UserPublicKey> toSet)
        {
            foreach (UserPublicKey keyShareToMove in keySharesToMove)
            {
                if (!fromSet.Contains(keyShareToMove))
                {
                    continue;
                }
                fromSet.Remove(keyShareToMove);
                toSet.Add(keyShareToMove);
            }
        }
    }
}