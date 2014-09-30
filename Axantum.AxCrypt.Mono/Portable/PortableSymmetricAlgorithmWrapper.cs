using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono.Portable
{
    internal class PortableSymmetricAlgorithmWrapper : Axantum.AxCrypt.Core.Portable.SymmetricAlgorithm
    {
        private System.Security.Cryptography.SymmetricAlgorithm _symmetricAlgorithm;

        public PortableSymmetricAlgorithmWrapper(System.Security.Cryptography.SymmetricAlgorithm symmetricAlgorithm)
        {
            _symmetricAlgorithm = symmetricAlgorithm;
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

        public override Core.Portable.KeySizes[] LegalBlockSizes
        {
            get { return _symmetricAlgorithm.LegalBlockSizes.Select(k => new Core.Portable.KeySizes() { MaxSize = k.MaxSize, MinSize = k.MinSize, SkipSize = k.SkipSize }).ToArray(); }
        }

        public override Core.Portable.KeySizes[] LegalKeySizes
        {
            get { return _symmetricAlgorithm.LegalKeySizes.Select(k => new Core.Portable.KeySizes() { MaxSize = k.MaxSize, MinSize = k.MinSize, SkipSize = k.SkipSize }).ToArray(); }
        }

        public override Core.Portable.CipherMode Mode
        {
            get
            {
                switch (_symmetricAlgorithm.Mode)
                {
                    case System.Security.Cryptography.CipherMode.CBC:
                        return Core.Portable.CipherMode.CBC;

                    case System.Security.Cryptography.CipherMode.CFB:
                        return Core.Portable.CipherMode.CFB;

                    case System.Security.Cryptography.CipherMode.CTS:
                        return Core.Portable.CipherMode.CTS;

                    case System.Security.Cryptography.CipherMode.ECB:
                        return Core.Portable.CipherMode.ECB;

                    case System.Security.Cryptography.CipherMode.OFB:
                        return Core.Portable.CipherMode.OFB;
                }
                return Core.Portable.CipherMode.None;
            }
            set
            {
                switch (value)
                {
                    case Axantum.AxCrypt.Core.Portable.CipherMode.CBC:
                        _symmetricAlgorithm.Mode = System.Security.Cryptography.CipherMode.CBC;
                        break;

                    case Axantum.AxCrypt.Core.Portable.CipherMode.ECB:
                        _symmetricAlgorithm.Mode = System.Security.Cryptography.CipherMode.ECB;
                        break;

                    case Axantum.AxCrypt.Core.Portable.CipherMode.OFB:
                        _symmetricAlgorithm.Mode = System.Security.Cryptography.CipherMode.OFB;
                        break;

                    case Axantum.AxCrypt.Core.Portable.CipherMode.CFB:
                        _symmetricAlgorithm.Mode = System.Security.Cryptography.CipherMode.CFB;
                        break;

                    case Axantum.AxCrypt.Core.Portable.CipherMode.CTS:
                        _symmetricAlgorithm.Mode = System.Security.Cryptography.CipherMode.CTS;
                        break;
                }
            }
        }

        public override Core.Portable.PaddingMode Padding
        {
            get
            {
                switch (_symmetricAlgorithm.Padding)
                {
                    case System.Security.Cryptography.PaddingMode.ANSIX923:
                        return Core.Portable.PaddingMode.ANSIX923;

                    case System.Security.Cryptography.PaddingMode.ISO10126:
                        return Core.Portable.PaddingMode.ISO10126;

                    case System.Security.Cryptography.PaddingMode.None:
                        return Core.Portable.PaddingMode.None;

                    case System.Security.Cryptography.PaddingMode.PKCS7:
                        return Core.Portable.PaddingMode.PKCS7;

                    case System.Security.Cryptography.PaddingMode.Zeros:
                        return Core.Portable.PaddingMode.Zeros;
                };
                return Core.Portable.PaddingMode.None;
            }
            set
            {
                switch (value)
                {
                    case Axantum.AxCrypt.Core.Portable.PaddingMode.None:
                        _symmetricAlgorithm.Padding = System.Security.Cryptography.PaddingMode.None;
                        break;

                    case Axantum.AxCrypt.Core.Portable.PaddingMode.PKCS7:
                        _symmetricAlgorithm.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                        break;

                    case Axantum.AxCrypt.Core.Portable.PaddingMode.Zeros:
                        _symmetricAlgorithm.Padding = System.Security.Cryptography.PaddingMode.Zeros;
                        break;

                    case Axantum.AxCrypt.Core.Portable.PaddingMode.ANSIX923:
                        _symmetricAlgorithm.Padding = System.Security.Cryptography.PaddingMode.ANSIX923;
                        break;

                    case Axantum.AxCrypt.Core.Portable.PaddingMode.ISO10126:
                        _symmetricAlgorithm.Padding = System.Security.Cryptography.PaddingMode.ISO10126;
                        break;
                };
            }
        }

        public override Core.Portable.ICryptoTransform CreateDecryptor()
        {
            return new PortableCryptoTransformWrapper(_symmetricAlgorithm.CreateDecryptor());
        }

        public override Core.Portable.ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return new PortableCryptoTransformWrapper(_symmetricAlgorithm.CreateDecryptor(rgbKey, rgbIV));
        }

        public override Core.Portable.ICryptoTransform CreateEncryptor()
        {
            return new PortableCryptoTransformWrapper(_symmetricAlgorithm.CreateEncryptor());
        }

        public override Core.Portable.ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return new PortableCryptoTransformWrapper(_symmetricAlgorithm.CreateEncryptor(rgbKey, rgbIV));
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