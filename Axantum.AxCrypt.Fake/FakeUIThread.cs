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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Fake
{
    public class FakeUIThread : IUIThread
    {
        private SynchronizationContext _context;

        public FakeUIThread()
        {
            _context = new SynchronizationContext();
        }

        public bool Blocked { get; set; }

        public bool IsOn { get; set; }

        public void SendTo(Action action)
        {
            _context.Send((state) => action(), null);
        }

        public Task SendToAsync(Func<Task> action)
        {
            _context.Send(async (state) => await action(), null);
            return Task.FromResult<object>(null);
        }

        public void PostTo(Action action)
        {
            _context.Send((state) => action(), null);
        }

        public void Yield()
        {
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}