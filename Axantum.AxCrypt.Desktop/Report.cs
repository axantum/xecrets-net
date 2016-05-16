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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Desktop
{
    public class Report : IReport
    {
        private INow _now;

        public Report()
        {
            _now = New<INow>();
        }

        private class Entry
        {
            public Exception Exception { get; set; }

            public DateTime DateTime { get; set; }
        }

        private Queue<Entry> _queue = new Queue<Entry>();

        public void Exception(Exception ex)
        {
            lock (_queue)
            {
                if (_queue.Count > 10)
                {
                    _queue.Dequeue();
                }
                _queue.Enqueue(new Entry() { Exception = ex, DateTime = _now.Utc, });
            }
        }

        public string Snapshot
        {
            get
            {
                return SnapshotInternal();
            }
        }

        private string SnapshotInternal()
        {
            Stack<Entry> stack = new Stack<Entry>();
            lock (_queue)
            {
                while (_queue.Count > 0)
                {
                    stack.Push(_queue.Dequeue());
                }
            }

            StringBuilder sb = new StringBuilder();
            while (stack.Count > 0)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                Entry entry = stack.Pop();
                sb.AppendFormat("----------- Exception at {0} -----------", entry.DateTime.ToString("u")).AppendLine();
                sb.AppendLine(entry.Exception.ToString());
            }

            return sb.ToString();
        }
    }
}