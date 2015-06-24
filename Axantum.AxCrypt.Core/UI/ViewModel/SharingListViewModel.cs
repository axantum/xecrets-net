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

        public IEnumerable<EmailAddress> SharedWith { get { return GetProperty<IEnumerable<EmailAddress>>("SharedWith"); } private set { SetProperty("SharedWith", value); } }

        public IEnumerable<EmailAddress> NotSharedWith { get { return GetProperty<IEnumerable<EmailAddress>>("NotSharedWith"); } private set { SetProperty("NotSharedWith", value); } }

        public IAction AddKeyShares { get; private set; }

        public IAction RemoveKeyShares { get; private set; }

        public SharingListViewModel(Func<KnownPublicKeys> knownPublicKeysFactory, IEnumerable<EmailAddress> sharedWith, LogOnIdentity logOnIdentity)
        {
            _knownPublicKeysFactory = knownPublicKeysFactory;
            _logOnIdentity = logOnIdentity ?? LogOnIdentity.Empty;

            InitializePropertyValues(sharedWith);
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        private void InitializePropertyValues(IEnumerable<EmailAddress> sharedWith)
        {
            EmailAddress userEmail = _logOnIdentity.UserKeys.UserEmail;
            SharedWith = sharedWith.Where(sw => sw != userEmail).ToList();

            using (KnownPublicKeys knownPublicKeys = _knownPublicKeysFactory())
            {
                NotSharedWith = knownPublicKeys.PublicKeys.Select(upk => upk.Email).Where(upk => upk != userEmail && !sharedWith.Any(sw => upk == sw)).OrderBy(e => e.Address);
            }

            AddKeyShares = new DelegateAction<IEnumerable<EmailAddress>>((upks) => AddKeySharesAction(upks));
            RemoveKeyShares = new DelegateAction<IEnumerable<EmailAddress>>((upks) => RemoveKeySharesAction(upks));
        }

        private static void BindPropertyChangedEvents()
        {
        }

        private static void SubscribeToModelEvents()
        {
        }

        private void RemoveKeySharesAction(IEnumerable<EmailAddress> keySharesToRemove)
        {
            HashSet<EmailAddress> fromSet = new HashSet<EmailAddress>(SharedWith);
            HashSet<EmailAddress> toSet = new HashSet<EmailAddress>(NotSharedWith);

            MoveKeyShares(keySharesToRemove, fromSet, toSet);

            SharedWith = fromSet.OrderBy(a => a.Address);
            NotSharedWith = toSet.OrderBy(a => a.Address);
        }

        private void AddKeySharesAction(IEnumerable<EmailAddress> keySharesToAdd)
        {
            HashSet<EmailAddress> fromSet = new HashSet<EmailAddress>(NotSharedWith);
            HashSet<EmailAddress> toSet = new HashSet<EmailAddress>(SharedWith);

            FectchMissingUnsharedPublicKeysFromServer(keySharesToAdd, fromSet);
            MoveKeyShares(keySharesToAdd, fromSet, toSet);

            NotSharedWith = fromSet.OrderBy(a => a.Address);
            SharedWith = toSet.OrderBy(a => a.Address);
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