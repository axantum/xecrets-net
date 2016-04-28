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
using System.IO;

namespace Axantum.AxCrypt.Core.Header
{
    public static class AxCrypt1Guid
    {
        // c0b9072e4f93f146a015792ca1d9e821
        public static readonly Guid Value = new Guid("2e07b9c0-934f-46f1-a015-792ca1d9e821");

        private static readonly byte[] _axCrypt1GuidBytes = Value.ToByteArray();

        public static byte[] GetBytes()
        {
            return (byte[])_axCrypt1GuidBytes.Clone();
        }

        public static void Write(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            stream.Write(_axCrypt1GuidBytes, 0, _axCrypt1GuidBytes.Length);
        }

        public static int Length
        {
            get { return _axCrypt1GuidBytes.Length; }
        }
    }
}