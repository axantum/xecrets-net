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

using System;

namespace Axantum.AxCrypt.Core.Session
{
    [Flags]
    public enum ActiveFileStatus
    {
        None = 0,
        AssumedOpenAndDecrypted = 1,
        NotDecrypted = 2,
        Error = 4,
        DecryptedIsPendingDelete = 8,

        /// <summary>
        /// Indicates a file operation has failed due to a sharing violation. Is only advisory, and may be ignored
        /// or reset when an event is raised that may change the situation.
        /// </summary>
        NotShareable = 16,

        IgnoreChange = 32,
        Inactive = 64,

        /// <summary>
        /// Set when an application was launched for the file, but no process could be seen to be started. This typically happens
        /// with Windows 8 apps, as well as some multi document softwares such as Word etc.
        /// </summary>
        NoProcessKnown = 128,
    }
}