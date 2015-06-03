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

using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.IO;
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
        private Func<KnownPublicKeys> _createKnownPublicKeys;

        public IEnumerable<UserPublicKey> SharedPublicKeys { get { return GetProperty<IEnumerable<UserPublicKey>>("SharedPublicKeys"); } private set { SetProperty("SharedPublicKeys", value); } }

        public IEnumerable<UserPublicKey> UnsharedPublicKeys { get { return GetProperty<IEnumerable<UserPublicKey>>("UnsharedPublicKeys"); } private set { SetProperty("UnsharedPublicKeys", value); } }

        public IAction AddShared { get; private set; }

        public IAction RemoveShared { get; private set; }

        public SharingListViewModel(Func<KnownPublicKeys> createKnownPublicKeys)
        {
            _createKnownPublicKeys = createKnownPublicKeys;

            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        private void InitializePropertyValues()
        {
            SharedPublicKeys = new List<UserPublicKey>();
            AddShared = new DelegateAction<IEnumerable<UserPublicKey>>((upks) => AddSharedAction(upks));
            RemoveShared = new DelegateAction<IEnumerable<UserPublicKey>>((upks) => RemoveSharedAction(upks));
            using (KnownPublicKeys knownPublicKeys = _createKnownPublicKeys())
            {
                UnsharedPublicKeys = knownPublicKeys.PublicKeys.OrderBy(upk => upk.Email.Address);
            }
        }

        private static void BindPropertyChangedEvents()
        {
        }

        private static void SubscribeToModelEvents()
        {
        }

        private void RemoveSharedAction(IEnumerable<UserPublicKey> userPublicKeysToRemove)
        {
            HashSet<UserPublicKey> fromSet = new HashSet<UserPublicKey>(SharedPublicKeys, UserPublicKey.EmailComparer);
            HashSet<UserPublicKey> toSet = new HashSet<UserPublicKey>(UnsharedPublicKeys, UserPublicKey.EmailComparer);

            MoveUserPublicKey(userPublicKeysToRemove, fromSet, toSet);

            SharedPublicKeys = fromSet;
            UnsharedPublicKeys = toSet;
        }

        private void AddSharedAction(IEnumerable<UserPublicKey> userPublicKeysToAdd)
        {
            HashSet<UserPublicKey> fromSet = new HashSet<UserPublicKey>(UnsharedPublicKeys, UserPublicKey.EmailComparer);
            HashSet<UserPublicKey> toSet = new HashSet<UserPublicKey>(SharedPublicKeys, UserPublicKey.EmailComparer);

            FectchMissingUnsharedPublicKeysFromServer(userPublicKeysToAdd, fromSet);
            MoveUserPublicKey(userPublicKeysToAdd, fromSet, toSet);

            UnsharedPublicKeys = fromSet;
            SharedPublicKeys = toSet;
        }

        private static void FectchMissingUnsharedPublicKeysFromServer(IEnumerable<UserPublicKey> userPublicKeysToAdd, HashSet<UserPublicKey> fromSet)
        {
            foreach (UserPublicKey publicKeyToAdd in userPublicKeysToAdd)
            {
                if (fromSet.Contains(publicKeyToAdd))
                {
                    continue;
                }
            }
        }

        private static void MoveUserPublicKey(IEnumerable<UserPublicKey> userPublicKeysToMove, HashSet<UserPublicKey> fromSet, HashSet<UserPublicKey> toSet)
        {
            foreach (UserPublicKey userPublicKeyToMove in userPublicKeysToMove)
            {
                if (!fromSet.Contains(userPublicKeyToMove, UserPublicKey.EmailComparer))
                {
                    continue;
                }
                fromSet.Remove(userPublicKeyToMove);
                toSet.Add(userPublicKeyToMove);
            }
        }
    }
}