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
using System.IO;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeRuntimeEnvironment : IRuntimeEnvironment
    {
        private byte _randomForTest = 0;

        private bool _isLittleEndian = BitConverter.IsLittleEndian;

        public FakeRuntimeEnvironment()
        {
            AxCryptExtension = ".axx";
        }

        public FakeRuntimeEnvironment(Endian endianness)
            : this()
        {
            _isLittleEndian = endianness == Endian.Reverse ? !_isLittleEndian : _isLittleEndian;
        }

        public bool IsLittleEndian
        {
            get { return _isLittleEndian; }
        }

        public byte[] GetRandomBytes(int count)
        {
            byte[] bytes = new byte[count];
            for (int i = 0; i < count; ++i)
            {
                bytes[i] = _randomForTest++;
            }
            return bytes;
        }

        public IRuntimeFileInfo FileInfo(string path)
        {
            return new FakeRuntimeFileInfo(path);
        }

        public string AxCryptExtension
        {
            get;
            set;
        }

        public bool IsDesktopWindows
        {
            get { return true; }
        }

        public int StreamBufferSize
        {
            get { return 512; }
        }
    }
}