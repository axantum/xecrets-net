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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class ImportPublicKeysViewModel : ViewModelBase
    {
        private Func<KnownPublicKeys> _createKnownPublicKeys;

        public ImportPublicKeysViewModel(Func<KnownPublicKeys> createKnownPublicKeys)
        {
            _createKnownPublicKeys = createKnownPublicKeys;

            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        public IAction ImportFiles { get; private set; }

        public IEnumerable<string> FailedFiles { get { return GetProperty<IEnumerable<string>>("FailedFiles"); } set { SetProperty("FailedFiles", value); } }

        private void InitializePropertyValues()
        {
            ImportFiles = new DelegateAction<IEnumerable<string>>((files) => ImportFilesAction(files));
        }

        private static void BindPropertyChangedEvents()
        {
        }

        private static void SubscribeToModelEvents()
        {
        }

        private void ImportFilesAction(IEnumerable<string> files)
        {
            List<string> failed = new List<string>();
            using (KnownPublicKeys knownPublicKeys = _createKnownPublicKeys())
            {
                foreach (string file in files)
                {
                    IDataStore publicKeyData = New<IDataStore>(file);
                    if (!knownPublicKeys.AddOrReplace(publicKeyData))
                    {
                        failed.Add(file);
                    }
                }
            }
            FailedFiles = failed;
        }
    }
}