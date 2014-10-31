using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Mono.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Mono.Cryptography
{
    internal class AesManagedWrapper : Axantum.AxCrypt.Core.Algorithm.AesManaged
    {
        private System.Security.Cryptography.SymmetricAlgorithm _symmetricAlgorithm;

        public AesManagedWrapper()
        {
            _symmetricAlgorithm = new System.Security.Cryptography.AesManaged();
        }

        public override void Clear()
        {
            _symmetricAlgorithm.Clear();
        }

        public override int BlockSize
        {
            get
            {
                return _symmetricAlgorithm.BlockSize;
            }
            set
            {
                _symmetricAlgorithm.BlockSize = value;
            }
        }

        public override int FeedbackSize
        {
            get
            {
                return _symmetricAlgorithm.FeedbackSize;
            }
            set
            {
                _symmetricAlgorithm.FeedbackSize = value;
            }
        }

        public override byte[] IV
        {
            get
            {
                return _symmetricAlgorithm.IV;
            }
            set
            {
                _symmetricAlgorithm.IV = value;
            }
        }

        public override byte[] Key
        {
            get
            {
                return _symmetricAlgorithm.Key;
            }
            set
            {
                _symmetricAlgorithm.Key = value;
            }
        }

        public override int KeySize
        {
            get
            {
                return _symmetricAlgorithm.KeySize;
            }
            set
            {
                _symmetricAlgorithm.KeySize = value;
            }
        }

        public override KeySizes[] LegalBlockSizes
        {
            get { return _symmetricAlgorithm.LegalBlockSizes.Select(k => new KeySizes() { MaxSize = k.MaxSize, MinSize = k.MinSize, SkipSize = k.SkipSize }).ToArray(); }
        }

        public override KeySizes[] LegalKeySizes
        {
            get { return _symmetricAlgorithm.LegalKeySizes.Select(k => new KeySizes() { MaxSize = k.MaxSize, MinSize = k.MinSize, SkipSize = k.SkipSize }).ToArray(); }
        }

        public override CipherMode Mode
        {
            get
            {
                switch (_symmetricAlgorithm.Mode)
                {
                    case System.Security.Cryptography.CipherMode.CBC:
                        return CipherMode.CBC;

                    case System.Security.Cryptography.CipherMode.CFB:
                        return CipherMode.CFB;

                    case System.Security.Cryptography.CipherMode.CTS:
                        return CipherMode.CTS;

                    case System.Security.Cryptography.CipherMode.ECB:
                        return CipherMode.ECB;

                    case System.Security.Cryptography.CipherMode.OFB:
                        return CipherMode.OFB;
                }
                return CipherMode.None;
            }
            set
            {
                switch (value)
                {
                    case CipherMode.CBC:
                        _symmetricAlgorithm.Mode = System.Security.Cryptography.CipherMode.CBC;
                        break;

                    case CipherMode.ECB:
                        _symmetricAlgorithm.Mode = System.Security.Cryptography.CipherMode.ECB;
                        break;

                    case CipherMode.OFB:
                        _symmetricAlgorithm.Mode = System.Security.Cryptography.CipherMode.OFB;
                        break;

                    case CipherMode.CFB:
                        _symmetricAlgorithm.Mode = System.Security.Cryptography.CipherMode.CFB;
                        break;

                    case CipherMode.CTS:
                        _symmetricAlgorithm.Mode = System.Security.Cryptography.CipherMode.CTS;
                        break;
                }
            }
        }

        public override PaddingMode Padding
        {
            get
            {
                switch (_symmetricAlgorithm.Padding)
                {
                    case System.Security.Cryptography.PaddingMode.ANSIX923:
                        return PaddingMode.ANSIX923;

                    case System.Security.Cryptography.PaddingMode.ISO10126:
                        return PaddingMode.ISO10126;

                    case System.Security.Cryptography.PaddingMode.None:
                        return PaddingMode.None;

                    case System.Security.Cryptography.PaddingMode.PKCS7:
                        return PaddingMode.PKCS7;

                    case System.Security.Cryptography.PaddingMode.Zeros:
                        return PaddingMode.Zeros;
                };
                return PaddingMode.None;
            }
            set
            {
                switch (value)
                {
                    case PaddingMode.None:
                        _symmetricAlgorithm.Padding = System.Security.Cryptography.PaddingMode.None;
                        break;

                    case PaddingMode.PKCS7:
                        _symmetricAlgorithm.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                        break;

                    case PaddingMode.Zeros:
                        _symmetricAlgorithm.Padding = System.Security.Cryptography.PaddingMode.Zeros;
                        break;

                    case PaddingMode.ANSIX923:
                        _symmetricAlgorithm.Padding = System.Security.Cryptography.PaddingMode.ANSIX923;
                        break;

                    case PaddingMode.ISO10126:
                        _symmetricAlgorithm.Padding = System.Security.Cryptography.PaddingMode.ISO10126;
                        break;
                };
            }
        }

        public override ICryptoTransform CreateDecryptor()
        {
            return new CryptoTransformWrapper(_symmetricAlgorithm.CreateDecryptor());
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return new CryptoTransformWrapper(_symmetricAlgorithm.CreateDecryptor(rgbKey, rgbIV));
        }

        public override ICryptoTransform CreateEncryptor()
        {
            return new CryptoTransformWrapper(_symmetricAlgorithm.CreateEncryptor());
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return new CryptoTransformWrapper(_symmetricAlgorithm.CreateEncryptor(rgbKey, rgbIV));
        }

        public override void GenerateIV()
        {
            _symmetricAlgorithm.GenerateIV();
        }

        public override void GenerateKey()
        {
            _symmetricAlgorithm.GenerateKey();
        }

        public override bool ValidKeySize(int bitLength)
        {
            return _symmetricAlgorithm.ValidKeySize(bitLength);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            _symmetricAlgorithm.Dispose();
        }
    }
}