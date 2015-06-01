#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IUserSettings
    {
        void Delete();

        string this[string key]
        {
            get;
            set;
        }

        T Load<T>(string key);

        T Load<T>(string key, T fallback);

        void Store<T>(string key, T value);

        string CultureName { get; set; }

        Uri AxCrypt2VersionCheckUrl { get; set; }

        Uri UpdateUrl { get; set; }

        DateTime LastUpdateCheckUtc { get; set; }

        string NewestKnownVersion { get; set; }

        bool DebugMode { get; set; }

        Uri AxCrypt2HelpUrl { get; set; }

        bool DisplayEncryptPassphrase { get; set; }

        bool DisplayDecryptPassphrase { get; set; }

        long GetKeyWrapIterations(Guid cryptoId);

        void SetKeyWrapIterations(Guid cryptoId, long keyWrapIterations);

        Salt ThumbprintSalt { get; set; }

        TimeSpan SessionNotificationMinimumIdle { get; set; }

        int SettingsVersion { get; set; }

        int CurrentSettingsVersion { get; }

        int AsymmetricKeyBits { get; set; }

        string UserEmail { get; set; }
    }
}