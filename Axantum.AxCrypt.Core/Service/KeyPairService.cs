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
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Service
{
    /// <summary>
    /// Prepare keys for use in the background, in order to make them available as quickly as possible. This class is thread safe.
    /// </summary>
    public class KeyPairService : IDisposable
    {
        private Task _running;

        private bool _disposed = false;

        private int _firstBatch;

        private int _preGenerationTargetCount;

        private int _keyBits;

        private Queue<IAsymmetricKeyPair> _keyPairs = new Queue<IAsymmetricKeyPair>();

        public KeyPairService()
        {
        }

        public KeyPairService(int firstBatch, int preGenerationTargetCount, int keyBits)
        {
            if (firstBatch < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(firstBatch));
            }
            if (preGenerationTargetCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(preGenerationTargetCount));
            }
#if DEBUG
            if (keyBits != 768 && keyBits != 4096)
#else
            if (keyBits != 4096)
#endif
            {
                throw new ArgumentOutOfRangeException(nameof(keyBits));
            }

            Initialize(firstBatch, preGenerationTargetCount, keyBits);
        }

        protected void Initialize(int firstBatch, int preGenerationTargetCount, int keyBits)
        {
            _firstBatch = firstBatch;
            _preGenerationTargetCount = preGenerationTargetCount;
            _keyBits = keyBits;
        }

        /// <summary>
        /// Gets a new key pair, either from the pre-generated queue or at worst by synchronously
        /// generating one.
        /// </summary>
        /// <returns></returns>
        public IAsymmetricKeyPair New()
        {
            IAsymmetricKeyPair keyPair;
            lock (_keyPairs)
            {
                if (_keyPairs.Count > 0)
                {
                    keyPair = _keyPairs.Dequeue();
                }
                else
                {
                    keyPair = CreateKeyPair();
                }
            }
            Start();
            return keyPair;
        }

        /// <summary>
        /// Starts a background process to fill up to the appropriate level of key pairs.
        /// </summary>
        public void Start()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(KeyPairService));
            }

            if (!(_running?.IsCompleted).GetValueOrDefault(true))
            {
                return;
            }
            _running = Task.Run(() => { while (!_disposed && KeyPairsNeeded()) { AddOneKeyPair(); } });
        }

        private void AddOneKeyPair()
        {
            IAsymmetricKeyPair keyPair = CreateKeyPair();
            lock (_keyPairs)
            {
                _keyPairs.Enqueue(keyPair);
            }
            return;
        }

        private bool KeyPairsNeeded()
        {
            lock (_keyPairs)
            {
                if (_firstBatch > 0)
                {
                    --_firstBatch;
                    return true;
                }
                return _keyPairs.Count < _preGenerationTargetCount;
            }
        }

        public bool IsAnyAvailable
        {
            get
            {
                lock (_keyPairs)
                {
                    return _keyPairs.Count > 0;
                }
            }
        }

        private IAsymmetricKeyPair CreateKeyPair()
        {
            IAsymmetricKeyPair keyPair = Resolve.AsymmetricFactory.CreateKeyPair(_keyBits);
            return keyPair;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_running != null && !_disposed)
            {
                _disposed = true;
                _running.Wait();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}