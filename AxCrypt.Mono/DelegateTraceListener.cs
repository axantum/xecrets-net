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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Diagnostics;
using System.Text;

namespace AxCrypt.Mono
{
    public class DelegateTraceListener : TraceListener
    {
        private Action<string> _trace;

        private StringBuilder _buffer = new StringBuilder();

        public DelegateTraceListener(string name, Action<string> trace)
            : base(name)
        {
            _trace = trace;
        }

        public override void Write(string? message)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));
            int i;
            while ((i = message.IndexOf(Environment.NewLine, StringComparison.Ordinal)) >= 0)
            {
                _ = _buffer.Append(message.AsSpan(0, i + Environment.NewLine.Length));
                _trace(_buffer.ToString());
                _buffer.Length = 0;
                message = message.Substring(i + Environment.NewLine.Length);
            }
            _ = _buffer.Append(message);
        }

        public override void WriteLine(string? message)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            Write(message + Environment.NewLine);
        }
    }
}
