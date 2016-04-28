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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Runtime
{
    public interface IThreadWorker : IDisposable
    {
        /// <summary>
        /// Start the asynchronous execution of the work.
        /// </summary>
        void Run();

        /// <summary>
        /// Perform blocking wait until this thread has completed execution.
        /// </summary>
        void Join();

        /// <summary>
        /// Abort this thread - can only be called *before* Run() has been called.
        /// </summary>
        void Abort();

        /// <summary>
        /// Returns true if the thread has completed execution.
        /// </summary>
        bool HasCompleted { get; }

        /// <summary>
        /// Raised just before asynchronous execution starts. Runs on the
        /// original thread, typically the GUI thread.
        /// </summary>
        event EventHandler<ThreadWorkerEventArgs> Prepare;

        /// <summary>
        /// Raised when asynchronous execution starts. Runs on a different
        /// thread than the caller thread. Do not interact with the GUI here.
        /// </summary>
        event EventHandler<ThreadWorkerEventArgs> Work;

        /// <summary>
        /// Raised when all is about to be done. Runs on the original thread, typically
        /// the GUI thread.
        /// </summary>
        event EventHandler<ThreadWorkerEventArgs> Completing;

        /// <summary>
        /// Raised when all is done. Runs on the original thread, typically
        /// the GUI thread.
        /// </summary>
        event EventHandler<ThreadWorkerEventArgs> Completed;
    }
}