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
        private Func<KnownPublicKeys> _knownPublicKeysFactory;

        private LogOnIdentity _logOnIdentity;

        public IEnumerable<EmailAddress> AddedKeyShares { get { return GetProperty<IEnumerable<EmailAddress>>("AddedKeyShares"); } private set { SetProperty("AddedKeyShares", value); } }

        public IEnumerable<EmailAddress> KnownKeyShares { get { return GetProperty<IEnumerable<EmailAddress>>("KnownKeyShares"); } private set { SetProperty("KnownKeyShares", value); } }

        public IAction AddKeyShares { get; private set; }

        public IAction RemoveKeyShares { get; private set; }

        public SharingListViewModel(Func<KnownPublicKeys> knownPublicKeysFactory, LogOnIdentity logOnIdentity)
        {
            _knownPublicKeysFactory = knownPublicKeysFactory;
            _logOnIdentity = logOnIdentity ?? LogOnIdentity.Empty;

            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        private void InitializePropertyValues()
        {
            AddedKeyShares = new List<EmailAddress>();
            AddKeyShares = new DelegateAction<IEnumerable<EmailAddress>>((upks) => AddKeySharesAction(upks));
            RemoveKeyShares = new DelegateAction<IEnumerable<EmailAddress>>((upks) => RemoveKeySharesAction(upks));
            using (KnownPublicKeys knownPublicKeys = _knownPublicKeysFactory())
            {
                EmailAddress userEmail = _logOnIdentity.UserKeys.UserEmail;
                KnownKeyShares = knownPublicKeys.PublicKeys.Select(upk => upk.Email).Where(upk => upk != userEmail).OrderBy(e => e.Address);
            }
        }

        private static void BindPropertyChangedEvents()
        {
        }

        private static void SubscribeToModelEvents()
        {
        }

        private void RemoveKeySharesAction(IEnumerable<EmailAddress> keySharesToRemove)
        {
            HashSet<EmailAddress> fromSet = new HashSet<EmailAddress>(AddedKeyShares);
            HashSet<EmailAddress> toSet = new HashSet<EmailAddress>(KnownKeyShares);

            MoveKeyShares(keySharesToRemove, fromSet, toSet);

            AddedKeyShares = fromSet.OrderBy(a => a.Address);
            KnownKeyShares = toSet.OrderBy(a => a.Address);
        }

        private void AddKeySharesAction(IEnumerable<EmailAddress> keySharesToAdd)
        {
            HashSet<EmailAddress> fromSet = new HashSet<EmailAddress>(KnownKeyShares);
            HashSet<EmailAddress> toSet = new HashSet<EmailAddress>(AddedKeyShares);

            FectchMissingUnsharedPublicKeysFromServer(keySharesToAdd, fromSet);
            MoveKeyShares(keySharesToAdd, fromSet, toSet);

            KnownKeyShares = fromSet.OrderBy(a => a.Address);
            AddedKeyShares = toSet.OrderBy(a => a.Address);
        }

        private static void FectchMissingUnsharedPublicKeysFromServer(IEnumerable<EmailAddress> keySharesToAdd, HashSet<EmailAddress> fromSet)
        {
            foreach (EmailAddress keyShareToAdd in keySharesToAdd)
            {
                if (fromSet.Contains(keyShareToAdd))
                {
                    continue;
                }
            }
        }

        private static void MoveKeyShares(IEnumerable<EmailAddress> keySharesToMove, HashSet<EmailAddress> fromSet, HashSet<EmailAddress> toSet)
        {
            foreach (EmailAddress keyShareToMove in keySharesToMove)
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