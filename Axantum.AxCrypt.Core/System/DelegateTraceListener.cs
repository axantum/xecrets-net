#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
using System.Diagnostics;
using System.Text;

namespace Axantum.AxCrypt.Core.System
{
    public class DelegateTraceListener : TraceListener
    {
        private Action<string> _trace;

        private StringBuilder _buffer = new StringBuilder();

        public DelegateTraceListener(Action<string> trace)
        {
            _trace = trace;
        }

        public override void Write(string message)
        {
            int i;
            while ((i = message.IndexOf(Environment.NewLine, StringComparison.Ordinal)) >= 0)
            {
                _buffer.Append(message.Substring(0, i + Environment.NewLine.Length));
                _trace(_buffer.ToString());
                _buffer.Clear();
                message = message.Substring(i + Environment.NewLine.Length);
            }
            _buffer.Append(message);
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }
    }
}