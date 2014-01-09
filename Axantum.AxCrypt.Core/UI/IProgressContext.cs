#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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
using System.Linq;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IProgressContext
    {
        event EventHandler<ProgressEventArgs> Progressing;

        void RemoveCount(long totalCount, long progressCount);

        void AddTotal(long count);

        void AddCount(long count);

        void NotifyLevelStart();

        void NotifyLevelFinished();

        bool Cancel { get; set; }

        bool AllItemsConfirmed { get; set; }

        /// <summary>
        /// Ensure sequenced serial access to the UI thread within the scope provided by this instance.
        /// </summary>
        /// <param name="getUIThread">if set to <c>true</c> Block until serial access to UI thread is granted,  otherwise release access to the UI thread.</param>
        void SerializeOnUIThread(bool getUIThread);

        /// <summary>
        /// Gets a value indicating whether the thread this instance is assocated with has acquired serial access to the UI thread.
        /// </summary>
        /// <value>
        /// <c>true</c> if [is serialized on UI thread]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// The instantiating code must ensure that this is a per-thread value.
        /// </remarks>
        bool IsSerializedOnUIThread { get; }
    }
}