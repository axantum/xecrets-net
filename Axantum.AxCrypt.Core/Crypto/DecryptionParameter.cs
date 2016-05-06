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

using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class DecryptionParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptionParameter"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="cryptoId">The crypto identifier.</param>
        /// <exception cref="System.ArgumentNullException">passphrase</exception>
        public DecryptionParameter(Passphrase passphrase, Guid cryptoId)
        {
            if (passphrase == null)
            {
                throw new ArgumentNullException("passphrase");
            }

            Passphrase = passphrase;
            CryptoId = cryptoId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptionParameter"/> class.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="cryptoId">The crypto identifier.</param>
        /// <exception cref="System.ArgumentNullException">privateKey</exception>
        public DecryptionParameter(IAsymmetricPrivateKey privateKey, Guid cryptoId)
        {
            if (privateKey == null)
            {
                throw new ArgumentNullException("privateKey");
            }

            PrivateKey = privateKey;
            CryptoId = cryptoId;
        }

        /// <summary>
        /// Creates an enumeration of all relevant DecryptionParameters from the provided arguments.
        /// </summary>
        /// <param name="passphrases">The passphrases or null.</param>
        /// <param name="privateKeys">The private keys or null.</param>
        /// <param name="cryptoIds">The crypto ids.</param>
        /// <returns></returns>
        public static IEnumerable<DecryptionParameter> CreateAll(IEnumerable<Passphrase> passphrases, IEnumerable<IAsymmetricPrivateKey> privateKeys, IEnumerable<Guid> cryptoIds)
        {
            if (cryptoIds == null)
            {
                throw new ArgumentNullException("cryptoIds");
            }

            List<DecryptionParameter> all = new List<DecryptionParameter>();

            foreach (Passphrase passphrase in passphrases ?? new Passphrase[0])
            {
                foreach (Guid cryptoId in cryptoIds)
                {
                    all.Add(new DecryptionParameter(passphrase, cryptoId));
                    if (cryptoId != new V1Aes128CryptoFactory().CryptoId)
                    {
                        continue;
                    }
                    string filtered = FilterV1Disallowed(passphrase.Text);
                    if (string.CompareOrdinal(filtered, passphrase.Text) == 0)
                    {
                        continue;
                    }
                    all.Add(new DecryptionParameter(Passphrase.Create(filtered), cryptoId));
                }
            }

            foreach (IAsymmetricPrivateKey privateKey in privateKeys ?? new IAsymmetricPrivateKey[0])
            {
                foreach (Guid cryptoId in cryptoIds)
                {
                    if (cryptoId == new V1Aes128CryptoFactory().CryptoId)
                    {
                        continue;
                    }
                    all.Add(new DecryptionParameter(privateKey, cryptoId));
                }
            }

            return all;
        }

        private static string _legacyV1AllowedCharacters = @" !""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]_abcdefghijklmnopqrstuvwxyz{|}€ŠŒŽšœžŸ¡¢£¤¥§±¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿ";

        private static string FilterV1Disallowed(string password)
        {
            StringBuilder filtered = new StringBuilder();
            foreach (char c in password)
            {
                if (_legacyV1AllowedCharacters.IndexOf(c) >= 0)
                {
                    filtered.Append(c);
                }
            }

            return filtered.ToString();
        }

        /// <summary>
        /// Gets passphrase. A passphrase is optional.
        /// </summary>
        /// <value>
        /// The passphrase.
        /// </value>
        public Passphrase Passphrase { get; private set; }

        /// <summary>
        /// Gets the private key.
        /// </summary>
        /// <value>
        /// The private key.
        /// </value>
        public IAsymmetricPrivateKey PrivateKey { get; private set; }

        /// <summary>
        /// Gets the crypto identifier.
        /// </summary>
        /// <value>
        /// The crypto identifier.
        /// </value>
        public Guid CryptoId { get; private set; }
    }
}