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

using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core;
using AxCrypt.Core.Algorithm;
using AxCrypt.Core.Portable;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xecrets.Net.Core.Test.LegacyImplementation
{
    internal class BouncyCastleAesWrapper : Aes
    {
        private static KeySizes[] _legalBlockSizes = new KeySizes[]
        {
            new KeySizes() {MinSize = 128, MaxSize = 128, SkipSize = 0}
        };

        private static KeySizes[] _legalKeySizes = new KeySizes[]
        {
            new KeySizes(){MinSize = 128, MaxSize = 256, SkipSize = 64}
        };

        public BouncyCastleAesWrapper()
        {
            SetBlockSizes(_legalBlockSizes);
            SetKeySizes(_legalKeySizes);
            InitializeBlockSize(128);
            InitializeFeedbackSize(128);
            InitializeKeySize(128);
        }

        /// <summary>
        /// Creates the decryptor.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        /// <returns></returns>
        public override ICryptoTransform CreateDecryptingTransform(byte[] key, byte[] iv)
        {
            return new BouncyCastleAesTransform(key, iv, false, Mode, Padding);
        }

        /// <summary>
        /// Creates the encryptor.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        /// <returns></returns>
        public override ICryptoTransform CreateEncryptingTransform(byte[] key, byte[] iv)
        {
            return new BouncyCastleAesTransform(key, iv, true, Mode, Padding);
        }

        /// <summary>
        /// When overridden in a derived class, generates a random initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />) to use for the algorithm.
        /// </summary>
        public override void GenerateIV()
        {
            SetIV(Resolve.RandomGenerator.Generate(BlockSize / 8));
        }

        /// <summary>
        /// When overridden in a derived class, generates a random key (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" />) to use for the algorithm.
        /// </summary>
        public override void GenerateKey()
        {
            SetKey(Resolve.RandomGenerator.Generate(KeySize / 8));
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This method is part of a general pattern.")]
        public static BouncyCastleAesWrapper? Create()
        {
            return Create("AxCrypt.Core.Algorithm.Implementation.Aes");
        }

        /// <summary>
        /// Creates the specified algorithm name.
        /// </summary>
        /// <param name="algorithmName">Name of the algorithm.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">algorithmName</exception>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This method is part of a general pattern.")]
        public static BouncyCastleAesWrapper? Create(string algorithmName)
        {
            if (algorithmName == null)
            {
                throw new ArgumentNullException("algorithmName");
            }
            if (algorithmName != "AxCrypt.Core.Algorithm.Implementation.Aes")
            {
                return null;
            }

            BouncyCastleAesWrapper aes = new BouncyCastleAesWrapper();
            aes.Padding = PaddingMode.None;
            aes.Mode = CipherMode.ECB;

            return aes;
        }
    }
}
