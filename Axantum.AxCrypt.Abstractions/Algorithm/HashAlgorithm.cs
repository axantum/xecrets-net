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
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Abstractions.Algorithm
{
    public abstract class HashAlgorithm : ICryptoTransform
    {
        public abstract byte[] ComputeHash(byte[] buffer);

        public abstract byte[] ComputeHash(byte[] buffer, int offset, int count);

        public abstract byte[] ComputeHash(Stream inputStream);

        public abstract byte[] Hash();

        public abstract int HashSize { get; }

        public abstract void Initialize();

        public abstract bool CanReuseTransform { get; }

        public abstract bool CanTransformMultipleBlocks { get; }

        public abstract int InputBlockSize { get; }

        public abstract int OutputBlockSize { get; }

        public abstract int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

        public abstract byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}