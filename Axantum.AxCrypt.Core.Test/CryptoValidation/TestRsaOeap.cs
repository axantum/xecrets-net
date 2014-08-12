using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test.CryptoValidation
{
    /// <summary>
    /// ====================
    /// pkcs-1v2-1-vec.zip
    /// ====================
    ///
    /// This directory contains test vectors for RSAES-OAEP and
    /// RSASSA-PSS as defined in PKCS #1 v2.1.
    ///
    /// The files:
    ///
    /// readme.txt              This file.
    ///
    /// oaep-vect.txt           Test vectors for RSAES-OAEP encryption.
    ///
    /// oaep-int.txt            Intermediate values for RSAES-OAEP
    ///                         encryption and RSA decryption with CRT.
    ///                         Also, DER-encoded RSAPrivateKey and
    ///                         RSAPublicKey types.
    ///
    /// pss-vect.txt            Test vectors for RSASSA-PSS signing.
    ///
    /// pss-int.txt             Intermediate values for RSASSA-PSS
    ///                         signing.
    ///
    /// =========================
    /// TEST VECTORS FOR RSA-OAEP
    /// =========================
    ///
    /// # This file contains test vectors for the
    /// # RSAES-OAEP encryption scheme as defined in
    /// # PKCS #1 v2.1. 10 RSA keys of different sizes
    /// # have been generated. For each key, 6 random
    /// # messages of length between 1 and 64 octets
    /// # have been RSAES-OAEP encrypted via a random
    /// # seed of length 20 octets.
    /// #
    /// # The underlying hash function is SHA-1; the
    /// # mask generation function is MGF1 with SHA-1
    /// # as specified in PKCS #1 v2.1.
    /// #
    /// # Integers are represented by strings of octets
    /// # with the leftmost octet being the most
    /// # significant octet. For example,
    /// #
    /// #           9,202,000 = (0x)8c 69 50.
    /// #
    /// # Key lengths:
    /// #
    /// # Key  1: 1024 bits
    /// # Key  2: 1025 bits
    /// # Key  3: 1026 bits
    /// # Key  4: 1027 bits
    /// # Key  5: 1028 bits
    /// # Key  6: 1029 bits
    /// # Key  7: 1030 bits
    /// # Key  8: 1031 bits
    /// # Key  9: 1536 bits
    /// # Key 10: 2048 bits
    /// # =============================================
    /// </summary>
    [TestFixture]
    public static class TestRsaOeap
    {
        internal class InjectedBytesRandomGenerator : IRandomGenerator
        {
            private byte[] _bytes;

            private int _offset = 0;

            public InjectedBytesRandomGenerator(byte[] bytes)
            {
                _bytes = bytes;
            }

            public byte[] Generate(int count)
            {
                byte[] bytes = new byte[count];
                for (int i = 0; i < bytes.Length; ++i)
                {
                    bytes[i] = _bytes[_offset++];
                    _offset %= _bytes.Length;
                }

                return bytes;
            }
        }

        [SetUp]
        public static void Setup()
        {
            Factory.Instance.Singleton<IAsymmetricFactory>(() => new FakeAsymmetricFactory("SHA1"));
        }

        [TearDown]
        public static void Teardown()
        {
            Factory.Instance.Clear();
        }

        private static byte[] PositiveFromHex(string hex)
        {
            hex = "00" + hex.Replace(" ", String.Empty).Replace("\r", String.Empty).Replace("\n", String.Empty);
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Odd number of characters is not allowed in a hex string.");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = Byte.Parse(hex.Substring(i + i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return bytes;
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            66 28 19 4e 12 07 3d b0 3b a9 4c da 9e f9 53 23
            97 d5 0d ba 79 b9 87 00 4a fe fe 34
            ";

            //# Seed:
            seed = @"
            18 b7 76 ea 21 06 9d 69 77 6a 33 e9 6b ad 48 e1
            dd a0 a5 ef
            ";

            //# Encryption:
            encryption = @"
            35 4f e6 7b 4a 12 6d 5d 35 fe 36 c7 77 79 1a 3f
            7b a1 3d ef 48 4e 2d 39 08 af f7 22 fa d4 68 fb
            21 69 6d e9 5d 0b e9 11 c2 d3 17 4f 8a fc c2 01
            03 5f 7b 6d 8e 69 40 2d e5 45 16 18 c2 1a 53 5f
            a9 d7 bf c5 b8 dd 9f c2 43 f8 cf 92 7d b3 13 22
            d6 e8 81 ea a9 1a 99 61 70 e6 57 a0 5a 26 64 26
            d9 8c 88 00 3f 84 77 c1 22 70 94 a0 d9 fa 1e 8c
            40 24 30 9c e1 ec cc b5 21 00 35 d4 7a c7 2e 8a
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            75 0c 40 47 f5 47 e8 e4 14 11 85 65 23 29 8a c9
            ba e2 45 ef af 13 97 fb e5 6f 9d d5
            ";

            //# Seed:
            seed = @"
            0c c7 42 ce 4a 9b 7f 32 f9 51 bc b2 51 ef d9 25
            fe 4f e3 5f
            ";

            //# Encryption:
            encryption = @"
            64 0d b1 ac c5 8e 05 68 fe 54 07 e5 f9 b7 01 df
            f8 c3 c9 1e 71 6c 53 6f c7 fc ec 6c b5 b7 1c 11
            65 98 8d 4a 27 9e 15 77 d7 30 fc 7a 29 93 2e 3f
            00 c8 15 15 23 6d 8d 8e 31 01 7a 7a 09 df 43 52
            d9 04 cd eb 79 aa 58 3a dc c3 1e a6 98 a4 c0 52
            83 da ba 90 89 be 54 91 f6 7c 1a 4e e4 8d c7 4b
            bb e6 64 3a ef 84 66 79 b4 cb 39 5a 35 2d 5e d1
            15 91 2d f6 96 ff e0 70 29 32 94 6d 71 49 2b 44
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            d9 4a e0 83 2e 64 45 ce 42 33 1c b0 6d 53 1a 82
            b1 db 4b aa d3 0f 74 6d c9 16 df 24 d4 e3 c2 45
            1f ff 59 a6 42 3e b0 e1 d0 2d 4f e6 46 cf 69 9d
            fd 81 8c 6e 97 b0 51
            ";

            //# Seed:
            seed = @"
            25 14 df 46 95 75 5a 67 b2 88 ea f4 90 5c 36 ee
            c6 6f d2 fd
            ";

            //# Encryption:
            encryption = @"
            42 37 36 ed 03 5f 60 26 af 27 6c 35 c0 b3 74 1b
            36 5e 5f 76 ca 09 1b 4e 8c 29 e2 f0 be fe e6 03
            59 5a a8 32 2d 60 2d 2e 62 5e 95 eb 81 b2 f1 c9
            72 4e 82 2e ca 76 db 86 18 cf 09 c5 34 35 03 a4
            36 08 35 b5 90 3b c6 37 e3 87 9f b0 5e 0e f3 26
            85 d5 ae c5 06 7c d7 cc 96 fe 4b 26 70 b6 ea c3
            06 6b 1f cf 56 86 b6 85 89 aa fb 7d 62 9b 02 d8
            f8 62 5c a3 83 36 24 d4 80 0f b0 81 b1 cf 94 eb
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            52 e6 50 d9 8e 7f 2a 04 8b 4f 86 85 21 53 b9 7e
            01 dd 31 6f 34 6a 19 f6 7a 85
            ";

            //# Seed:
            seed = @"
            c4 43 5a 3e 1a 18 a6 8b 68 20 43 62 90 a3 7c ef
            b8 5d b3 fb
            ";

            //# Encryption:
            encryption = @"
            45 ea d4 ca 55 1e 66 2c 98 00 f1 ac a8 28 3b 05
            25 e6 ab ae 30 be 4b 4a ba 76 2f a4 0f d3 d3 8e
            22 ab ef c6 97 94 f6 eb bb c0 5d db b1 12 16 24
            7d 2f 41 2f d0 fb a8 7c 6e 3a cd 88 88 13 64 6f
            d0 e4 8e 78 52 04 f9 c3 f7 3d 6d 82 39 56 27 22
            dd dd 87 71 fe c4 8b 83 a3 1e e6 f5 92 c4 cf d4
            bc 88 17 4f 3b 13 a1 12 aa e3 b9 f7 b8 0e 0f c6
            f7 25 5b a8 80 dc 7d 80 21 e2 2a d6 a8 5f 07 55
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            8d a8 9f d9 e5 f9 74 a2 9f ef fb 46 2b 49 18 0f
            6c f9 e8 02
            ";

            //# Seed:
            seed = @"
            b3 18 c4 2d f3 be 0f 83 fe a8 23 f5 a7 b4 7e d5
            e4 25 a3 b5
            ";

            //# Encryption:
            encryption = @"
            36 f6 e3 4d 94 a8 d3 4d aa cb a3 3a 21 39 d0 0a
            d8 5a 93 45 a8 60 51 e7 30 71 62 00 56 b9 20 e2
            19 00 58 55 a2 13 a0 f2 38 97 cd cd 73 1b 45 25
            7c 77 7f e9 08 20 2b ef dd 0b 58 38 6b 12 44 ea
            0c f5 39 a0 5d 5d 10 32 9d a4 4e 13 03 0f d7 60
            dc d6 44 cf ef 20 94 d1 91 0d 3f 43 3e 1c 7c 6d
            d1 8b c1 f2 df 7f 64 3d 66 2f b9 dd 37 ea d9 05
            91 90 f4 fa 66 ca 39 e8 69 c4 eb 44 9c bd c4 39
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            26 52 10 50 84 42 71
            ";

            //# Seed:
            seed = @"
            e4 ec 09 82 c2 33 6f 3a 67 7f 6a 35 61 74 eb 0c
            e8 87 ab c2
            ";

            //# Encryption:
            encryption = @"
            42 ce e2 61 7b 1e ce a4 db 3f 48 29 38 6f bd 61
            da fb f0 38 e1 80 d8 37 c9 63 66 df 24 c0 97 b4
            ab 0f ac 6b df 59 0d 82 1c 9f 10 64 2e 68 1a d0
            5b 8d 78 b3 78 c0 f4 6c e2 fa d6 3f 74 e0 ad 3d
            f0 6b 07 5d 7e b5 f5 63 6f 8d 40 3b 90 59 ca 76
            1b 5c 62 bb 52 aa 45 00 2e a7 0b aa ce 08 de d2
            43 b9 d8 cb d6 2a 68 ad e2 65 83 2b 56 56 4e 43
            a6 fa 42 ed 19 9a 09 97 69 74 2d f1 53 9e 82 55
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            8f f0 0c aa 60 5c 70 28 30 63 4d 9a 6c 3d 42 c6
            52 b5 8c f1 d9 2f ec 57 0b ee e7
            ";

            //# Seed:
            seed = @"
            8c 40 7b 5e c2 89 9e 50 99 c5 3e 8c e7 93 bf 94
            e7 1b 17 82
            ";

            //# Encryption:
            encryption = @"
            01 81 af 89 22 b9 fc b4 d7 9d 92 eb e1 98 15 99
            2f c0 c1 43 9d 8b cd 49 13 98 a0 f4 ad 3a 32 9a
            5b d9 38 55 60 db 53 26 83 c8 b7 da 04 e4 b1 2a
            ed 6a ac df 47 1c 34 c9 cd a8 91 ad dc c2 df 34
            56 65 3a a6 38 2e 9a e5 9b 54 45 52 57 eb 09 9d
            56 2b be 10 45 3f 2b 6d 13 c5 9c 02 e1 0f 1f 8a
            bb 5d a0 d0 57 09 32 da cf 2d 09 01 db 72 9d 0f
            ef cc 05 4e 70 96 8e a5 40 c8 1b 04 bc ae fe 72
            0e
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            2d
            ";

            //# Seed:
            seed = @"
            b6 00 cf 3c 2e 50 6d 7f 16 77 8c 91 0d 3a 8b 00
            3e ee 61 d5
            ";

            //# Encryption:
            encryption = @"
            01 87 59 ff 1d f6 3b 27 92 41 05 62 31 44 16 a8
            ae af 2a c6 34 b4 6f 94 0a b8 2d 64 db f1 65 ee
            e3 30 11 da 74 9d 4b ab 6e 2f cd 18 12 9c 9e 49
            27 7d 84 53 11 2b 42 9a 22 2a 84 71 b0 70 99 39
            98 e7 58 86 1c 4d 3f 6d 74 9d 91 c4 29 0d 33 2c
            7a 4a b3 f7 ea 35 ff 3a 07 d4 97 c9 55 ff 0f fc
            95 00 6b 62 c6 d2 96 81 0d 9b fa b0 24 19 6c 79
            34 01 2c 2d f9 78 ef 29 9a ba 23 99 40 cb a1 02
            45
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            74 fc 88 c5 1b c9 0f 77 af 9d 5e 9a 4a 70 13 3d
            4b 4e 0b 34 da 3c 37 c7 ef 8e
            ";

            //# Seed:
            seed = @"
            a7 37 68 ae ea a9 1f 9d 8c 1e d6 f9 d2 b6 34 67
            f0 7c ca e3
            ";

            //# Encryption:
            encryption = @"
            01 88 02 ba b0 4c 60 32 5e 81 c4 96 23 11 f2 be
            7c 2a dc e9 30 41 a0 07 19 c8 8f 95 75 75 f2 c7
            9f 1b 7b c8 ce d1 15 c7 06 b3 11 c0 8a 2d 98 6c
            a3 b6 a9 33 6b 14 7c 29 c6 f2 29 40 9d de c6 51
            bd 1f dd 5a 0b 7f 61 0c 99 37 fd b4 a3 a7 62 36
            4b 8b 32 06 b4 ea 48 5f d0 98 d0 8f 63 d4 aa 8b
            b2 69 7d 02 7b 75 0c 32 d7 f7 4e af 51 80 d2 e9
            b6 6b 17 cb 2f a5 55 23 bc 28 0d a1 0d 14 be 20
            53
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            a7 eb 2a 50 36 93 1d 27 d4 e8 91 32 6d 99 69 2f
            fa dd a9 bf 7e fd 3e 34 e6 22 c4 ad c0 85 f7 21
            df e8 85 07 2c 78 a2 03 b1 51 73 9b e5 40 fa 8c
            15 3a 10 f0 0a
            ";

            //# Seed:
            seed = @"
            9a 7b 3b 0e 70 8b d9 6f 81 90 ec ab 4f b9 b2 b3
            80 5a 81 56
            ";

            //# Encryption:
            encryption = @"
            00 a4 57 8c bc 17 63 18 a6 38 fb a7 d0 1d f1 57
            46 af 44 d4 f6 cd 96 d7 e7 c4 95 cb f4 25 b0 9c
            64 9d 32 bf 88 6d a4 8f ba f9 89 a2 11 71 87 ca
            fb 1f b5 80 31 76 90 e3 cc d4 46 92 0b 7a f8 2b
            31 db 58 04 d8 7d 01 51 4a cb fa 91 56 e7 82 f8
            67 f6 be d9 44 9e 0e 9a 2c 09 bc ec c6 aa 08 76
            36 96 5e 34 b3 ec 76 6f 2f e2 e4 30 18 a2 fd de
            b1 40 61 6a 0e 9d 82 e5 33 10 24 ee 06 52 fc 76
            41
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            2e f2 b0 66 f8 54 c3 3f 3b dc bb 59 94 a4 35 e7
            3d 6c 6c
            ";

            //# Seed:
            seed = @"
            eb 3c eb bc 4a dc 16 bb 48 e8 8c 8a ec 0e 34 af
            7f 42 7f d3
            ";

            //# Encryption:
            encryption = @"
            00 eb c5 f5 fd a7 7c fd ad 3c 83 64 1a 90 25 e7
            7d 72 d8 a6 fb 33 a8 10 f5 95 0f 8d 74 c7 3e 8d
            93 1e 86 34 d8 6a b1 24 62 56 ae 07 b6 00 5b 71
            b7 f2 fb 98 35 12 18 33 1c e6 9b 8f fb dc 9d a0
            8b bc 9c 70 4f 87 6d eb 9d f9 fc 2e c0 65 ca d8
            7f 90 90 b0 7a cc 17 aa 7f 99 7b 27 ac a4 88 06
            e8 97 f7 71 d9 51 41 fe 45 26 d8 a5 30 1b 67 86
            27 ef ab 70 7f d4 0f be bd 6e 79 2a 25 61 3e 7a
            ec
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            8a 7f b3 44 c8 b6 cb 2c f2 ef 1f 64 3f 9a 32 18
            f6 e1 9b ba 89 c0
            ";

            //# Seed:
            seed = @"
            4c 45 cf 4d 57 c9 8e 3d 6d 20 95 ad c5 1c 48 9e
            b5 0d ff 84
            ";

            //# Encryption:
            encryption = @"
            01 08 39 ec 20 c2 7b 90 52 e5 5b ef b9 b7 7e 6f
            c2 6e 90 75 d7 a5 43 78 c6 46 ab df 51 e4 45 bd
            57 15 de 81 78 9f 56 f1 80 3d 91 70 76 4a 9e 93
            cb 78 79 86 94 02 3e e7 39 3c e0 4b c5 d8 f8 c5
            a5 2c 17 1d 43 83 7e 3a ca 62 f6 09 eb 0a a5 ff
            b0 96 0e f0 41 98 dd 75 4f 57 f7 fb e6 ab f7 65
            cf 11 8b 4c a4 43 b2 3b 5a ab 26 6f 95 23 26 ac
            45 81 10 06 44 32 5f 8b 72 1a cd 5d 04 ff 14 ef
            3a
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example3_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
08 78 20 b5 69 e8 fa 8d
            ";

            //# Seed:
            seed = @"
8c ed 6b 19 62 90 80 57 90 e9 09 07 40 15 e6 a2
0b 0c 48 94
            ";

            //# Encryption:
            encryption = @"
02 6a 04 85 d9 6a eb d9 6b 43 82 08 50 99 b9 62
e6 a2 bd ec 3d 90 c8 db 62 5e 14 37 2d e8 5e 2d
5b 7b aa b6 5c 8f af 91 bb 55 04 fb 49 5a fc e5
c9 88 b3 f6 a5 2e 20 e1 d6 cb d3 56 6c 5c d1 f2
b8 31 8b b5 42 cc 0e a2 5c 4a ab 99 32 af a2 07
60 ea dd ec 78 43 96 a0 7e a0 ef 24 d4 e6 f4 d3
7e 50 52 a7 a3 1e 14 6a a4 80 a1 11 bb e9 26 40
13 07 e0 0f 41 00 33 84 2b 6d 82 fe 5c e4 df ae
80
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example3_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
46 53 ac af 17 19 60 b0 1f 52 a7 be 63 a3 ab 21
dc 36 8e c4 3b 50 d8 2e c3 78 1e 04
            ";

            //# Seed:
            seed = @"
b4 29 1d 65 67 55 08 48 cc 15 69 67 c8 09 ba ab
6c a5 07 f0
            ";

            //# Encryption:
            encryption = @"
02 4d b8 9c 78 02 98 9b e0 78 38 47 86 30 84 94
1b f2 09 d7 61 98 7e 38 f9 7c b5 f6 f1 bc 88 da
72 a5 0b 73 eb af 11 c8 79 c4 f9 5d f3 7b 85 0b
8f 65 d7 62 2e 25 b1 b8 89 e8 0f e8 0b ac a2 06
9d 6e 0e 1d 82 99 53 fc 45 90 69 de 98 ea 97 98
b4 51 e5 57 e9 9a bf 8f e3 d9 cc f9 09 6e bb f3
e5 25 5d 3b 4e 1c 6d 2e ca df 06 7a 35 9e ea 86
40 5a cd 47 d5 e1 65 51 7c ca fd 47 d6 db ee 4b
f5
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example3_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
d9 4c d0 e0 8f a4 04 ed 89
            ";

            //# Seed:
            seed = @"
ce 89 28 f6 05 95 58 25 40 08 ba dd 97 94 fa dc
d2 fd 1f 65
            ";

            //# Encryption:
            encryption = @"
02 39 bc e6 81 03 24 41 52 88 77 d6 d1 c8 bb 28
aa 3b c9 7f 1d f5 84 56 36 18 99 57 97 68 38 44
ca 86 66 47 32 f4 be d7 a0 aa b0 83 aa ab fb 72
38 f5 82 e3 09 58 c2 02 4e 44 e5 70 43 b9 79 50
fd 54 3d a9 77 c9 0c dd e5 33 7d 61 84 42 f9 9e
60 d7 78 3a b5 9c e6 dd 9d 69 c4 7a d1 e9 62 be
c2 2d 05 89 5c ff 8d 3f 64 ed 52 61 d9 2b 26 78
51 03 93 48 49 90 ba 3f 7f 06 81 8a e6 ff ce 8a
3a
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        [Ignore("This test currently fails. Why is to be determined.")]
        public static void TestRSAES_OAEP_Encryption_Example3_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
6c c6 41 b6 b6 1e 6f 96 39 74 da d2 3a 90 13 28
4e f1
            ";

            //# Seed:
            seed = @"
ce 89 28 f6 05 95 58 25 40 08 ba dd 97 94 fa dc
d2 fd 1f 65
            ";

            //# Encryption:
            encryption = @"
02 99 4c 62 af d7 6f 49 8b a1 fd 2c f6 42 85 7f
ca 81 f4 37 3c b0 8f 1c ba ee 6f 02 5c 3b 51 2b
42 c3 e8 77 91 13 47 66 48 03 9d be 04 93 f9 24
62 92 fa c2 89 50 60 0e 7c 0f 32 ed f9 c8 1b 9d
ec 45 c3 bd e0 cc 8d 88 47 59 01 69 90 7b 7d c5
99 1c eb 29 bb 07 14 d6 13 d9 6d f0 f1 2e c5 d8
d3 50 7c 8e e7 ae 78 dd 83 f2 16 fa 61 de 10 03
63 ac a4 8a 7e 91 4a e9 f4 2d df be 94 3b 09 d9
a0
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example3_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
df 51 51 83 2b 61 f4 f2 58 91 fb 41 72 f3 28 d2
ed df 83 71 ff cf db e9 97 93 92 95 f3 0e ca 69
18 01 7c fd a1 15 3b f7 a6 af 87 59 32 23
            ";

            //# Seed:
            seed = @"
2d 76 0b fe 38 c5 9d e3 4c dc 8b 8c 78 a3 8e 66
28 4a 2d 27
            ";

            //# Encryption:
            encryption = @"
01 62 04 2f f6 96 95 92 a6 16 70 31 81 1a 23 98
34 ce 63 8a bf 54 fe c8 b9 94 78 12 2a fe 2e e6
7f 8c 5b 18 b0 33 98 05 bf db c5 a4 e6 72 0b 37
c5 9c fb a9 42 46 4c 59 7f f5 32 a1 19 82 15 45
fd 2e 59 b1 14 e6 1d af 71 82 05 29 f5 02 9c f5
24 95 43 27 c3 4e c5 e6 f5 ba 7e fc c4 de 94 3a
b8 ad 4e d7 87 b1 45 43 29 f7 0d b7 98 a3 a8 f4
d9 2f 82 74 e2 b2 94 8a de 62 7c e8 ee 33 e4 3c
60
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example3_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
3c 3b ad 89 3c 54 4a 6d 52 0a b0 22 31 91 88 c8
d5 04 b7 a7 88 b8 50 90 3b 85 97 2e aa 18 55 2e
11 34 a7 ad 60 98 82 62 54 ff 7a b6 72 b3 d8 eb
31 58 fa c6 d4 cb ae f1
            ";

            //# Seed:
            seed = @"
f1 74 77 9c 5f d3 cf e0 07 ba dc b7 a3 6c 9b 55
bf cf bf 0e
            ";

            //# Encryption:
            encryption = @"
00 11 20 51 e7 5d 06 49 43 bc 44 78 07 5e 43 48
2f d5 9c ee 06 79 de 68 93 ee c3 a9 43 da a4 90
b9 69 1c 93 df c0 46 4b 66 23 b9 f3 db d3 e7 00
83 26 4f 03 4b 37 4f 74 16 4e 1a 00 76 37 25 e5
74 74 4b a0 b9 db 83 43 4f 31 df 96 f6 e2 a2 6f
6d 8e ba 34 8b d4 68 6c 22 38 ac 07 c3 7a ac 37
85 d1 c7 ee a2 f8 19 fd 91 49 17 98 ed 8e 9c ef
5e 43 b7 81 b0 e0 27 6e 37 c4 3f f9 49 2d 00 57
30
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
4a 86 60 95 34 ee 43 4a 6c bc a3 f7 e9 62 e7 6d
45 5e 32 64 c1 9f 60 5f 6e 5f f6 13 7c 65 c5 6d
7f b3 44 cd 52 bc 93 37 4f 3d 16 6c 9f 0c 6f 9c
50 6b ad 19 33 09 72 d2
            ";

            //# Seed:
            seed = @"
1c ac 19 ce 99 3d ef 55 f9 82 03 f6 85 28 96 c9
5c cc a1 f3
            ";

            //# Encryption:
            encryption = @"
04 cc e1 96 14 84 5e 09 41 52 a3 fe 18 e5 4e 33
30 c4 4e 5e fb c6 4a e1 68 86 cb 18 69 01 4c c5
78 1b 1f 8f 9e 04 53 84 d0 11 2a 13 5c a0 d1 2e
9c 88 a8 e4 06 34 16 de aa e3 84 4f 60 d6 e9 6f
e1 55 14 5f 45 25 b9 a3 44 31 ca 37 66 18 0f 70
e1 5a 5e 5d 8e 8b 1a 51 6f f8 70 60 9f 13 f8 96
93 5c ed 18 82 79 a5 8e d1 3d 07 11 42 77 d7 5c
65 68 60 7e 0a b0 92 fd 80 3a 22 3e 4a 8e e0 b1
a8
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
b0 ad c4 f3 fe 11 da 59 ce 99 27 73 d9 05 99 43
c0 30 46 49 7e e9 d9 f9 a0 6d f1 16 6d b4 6d 98
f5 8d 27 ec 07 4c 02 ee e6 cb e2 44 9c 8b 9f c5
08 0c 5c 3f 44 33 09 25 12 ec 46 aa 79 37 43 c8
            ";

            //# Seed:
            seed = @"
f5 45 d5 89 75 85 e3 db 71 aa 0c b8 da 76 c5 1d
03 2a e9 63
            ";

            //# Encryption:
            encryption = @"
00 97 b6 98 c6 16 56 45 b3 03 48 6f bf 5a 2a 44
79 c0 ee 85 88 9b 54 1a 6f 0b 85 8d 6b 65 97 b1
3b 85 4e b4 f8 39 af 03 39 9a 80 d7 9b da 65 78
c8 41 f9 0d 64 57 15 b2 80 d3 71 43 99 2d d1 86
c8 0b 94 9b 77 5c ae 97 37 0e 4e c9 74 43 13 6c
6d a4 84 e9 70 ff db 13 23 a2 08 47 82 1d 3b 18
38 1d e1 3b b4 9a ae a6 65 30 c4 a4 b8 27 1f 3e
ae 17 2c d3 66 e0 7e 66 36 f1 01 9d 2a 28 ae d1
5e
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
bf 6d 42 e7 01 70 7b 1d 02 06 b0 c8 b4 5a 1c 72
64 1f f1 28 89 21 9a 82 bd ea 96 5b 5e 79 a9 6b
0d 01 63 ed 9d 57 8e c9 ad a2 0f 2f bc f1 ea 3c
40 89 d8 34 19 ba 81 b0 c6 0f 36 06 da 99
            ";

            //# Seed:
            seed = @"
ad 99 7f ee f7 30 d6 ea 7b e6 0d 0d c5 2e 72 ea
cb fd d2 75
            ";

            //# Encryption:
            encryption = @"
03 01 f9 35 e9 c4 7a bc b4 8a cb be 09 89 5d 9f
59 71 af 14 83 9d a4 ff 95 41 7e e4 53 d1 fd 77
31 90 72 bb 72 97 e1 b5 5d 75 61 cd 9d 1b b2 4c
1a 9a 37 c6 19 86 43 08 24 28 04 87 9d 86 eb d0
01 dc e5 18 39 75 e1 50 69 89 b7 0e 5a 83 43 41
54 d5 cb fd 6a 24 78 7e 60 eb 0c 65 8d 2a c1 93
30 2d 11 92 c6 e6 22 d4 a1 2a d4 b5 39 23 bc a2
46 df 31 c6 39 5e 37 70 2c 6a 78 ae 08 1f b9 d0
65
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
fb 2e f1 12 f5 e7 66 eb 94 01 92 97 93 47 94 f7
be 2f 6f c1 c5 8e
            ";

            //# Seed:
            seed = @"
13 64 54 df 57 30 f7 3c 80 7a 7e 40 d8 c1 a3 12
ac 5b 9d d3
            ";

            //# Encryption:
            encryption = @"
02 d1 10 ad 30 af b7 27 be b6 91 dd 0c f1 7d 0a
f1 a1 e7 fa 0c c0 40 ec 1a 4b a2 6a 42 c5 9d 0a
79 6a 2e 22 c8 f3 57 cc c9 8b 65 19 ac eb 68 2e
94 5e 62 cb 73 46 14 a5 29 40 7c d4 52 be e3 e4
4f ec e8 42 3c c1 9e 55 54 8b 8b 99 4b 84 9c 7e
cd e4 93 3e 76 03 7e 1d 0c e4 42 75 b0 87 10 c6
8e 43 01 30 b9 29 73 0e d7 7e 09 b0 15 64 2c 55
93 f0 4e 4f fb 94 10 79 81 02 a8 e9 6f fd fe 11
e4
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
28 cc d4 47 bb 9e 85 16 6d ab b9 e5 b7 d1 ad ad
c4 b9 d3 9f 20 4e 96 d5 e4 40 ce 9a d9 28 bc 1c
22 84
            ";

            //# Seed:
            seed = @"
bc a8 05 7f 82 4b 2e a2 57 f2 86 14 07 ee f6 3d
33 20 86 81
            ";

            //# Encryption:
            encryption = @"
00 db b8 a7 43 9d 90 ef d9 19 a3 77 c5 4f ae 8f
e1 1e c5 8c 3b 85 83 62 e2 3a d1 b8 a4 43 10 79
90 66 b9 93 47 aa 52 56 91 d2 ad c5 8d 9b 06 e3
4f 28 8c 17 03 90 c5 f0 e1 1c 0a a3 64 59 59 f1
8e e7 9e 8f 2b e8 d7 ac 5c 23 d0 61 f1 8d d7 4b
8c 5f 2a 58 fc b5 eb 0c 54 f9 9f 01 a8 32 47 56
82 92 53 65 83 34 09 48 d7 a8 c9 7c 4a cd 1e 98
d1 e2 9d c3 20 e9 7a 26 05 32 a8 aa 7a 75 8a 1e
c2
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
f2 22 42 75 1e c6 b1
            ";

            //# Seed:
            seed = @"
2e 7e 1e 17 f6 47 b5 dd d0 33 e1 54 72 f9 0f 68
12 f3 ac 4e
            ";

            //# Encryption:
            encryption = @"
00 a5 ff a4 76 8c 8b be ca ee 2d b7 7e 8f 2e ec
99 59 59 33 54 55 20 83 5e 5b a7 db 94 93 d3 e1
7c dd ef e6 a5 f5 67 62 44 71 90 8d b4 e2 d8 3a
0f be e6 06 08 fc 84 04 95 03 b2 23 4a 07 dc 83
b2 7b 22 84 7a d8 92 0f f4 2f 67 4e f7 9b 76 28
0b 00 23 3d 2b 51 b8 cb 27 03 a9 d4 2b fb c8 25
0c 96 ec 32 c0 51 e5 7f 1b 4b a5 28 db 89 c3 7e
4c 54 e2 7e 6e 64 ac 69 63 5a e8 87 d9 54 16 19
a9
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        private static void SetupKeysExample1(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# RSA modulus n:
            n = @"
            a8 b3 b2 84 af 8e b5 0b 38 70 34 a8 60 f1 46 c4
            91 9f 31 87 63 cd 6c 55 98 c8 ae 48 11 a1 e0 ab
            c4 c7 e0 b0 82 d6 93 a5 e7 fc ed 67 5c f4 66 85
            12 77 2c 0c bc 64 a7 42 c6 c6 30 f5 33 c8 cc 72
            f6 2a e8 33 c4 0b f2 58 42 e9 84 bb 78 bd bf 97
            c0 10 7d 55 bd b6 62 f5 c4 e0 fa b9 84 5c b5 14
            8e f7 39 2d d3 aa ff 93 ae 1e 6b 66 7b b3 d4 24
            76 16 d4 f5 ba 10 d4 cf d2 26 de 88 d3 9f 16 fb";

            //# RSA public exponent e:
            e = @"01 00 01";

            //# RSA private exponent d:
            d = @"
            53 33 9c fd b7 9f c8 46 6a 65 5c 73 16 ac a8 5c
            55 fd 8f 6d d8 98 fd af 11 95 17 ef 4f 52 e8 fd
            8e 25 8d f9 3f ee 18 0f a0 e4 ab 29 69 3c d8 3b
            15 2a 55 3d 4a c4 d1 81 2b 8b 9f a5 af 0e 7f 55
            fe 73 04 df 41 57 09 26 f3 31 1f 15 c4 d6 5a 73
            2c 48 31 16 ee 3d 3d 2d 0a f3 54 9a d9 bf 7c bf
            b7 8a d8 84 f8 4d 5b eb 04 72 4d c7 36 9b 31 de
            f3 7d 0c f5 39 e9 cf cd d3 de 65 37 29 ea d5 d1";

            //# Prime p:
            p = @"
            d3 27 37 e7 26 7f fe 13 41 b2 d5 c0 d1 50 a8 1b
            58 6f b3 13 2b ed 2f 8d 52 62 86 4a 9c b9 f3 0a
            f3 8b e4 48 59 8d 41 3a 17 2e fb 80 2c 21 ac f1
            c1 1c 52 0c 2f 26 a4 71 dc ad 21 2e ac 7c a3 9d";

            //# Prime q:
            q = @"
            cc 88 53 d1 d5 4d a6 30 fa c0 04 f4 71 f2 81 c7
            b8 98 2d 82 24 a4 90 ed be b3 3d 3e 3d 5c c9 3c
            47 65 70 3d 1d d7 91 64 2f 1f 11 6a 0d d8 52 be
            24 19 b2 af 72 bf e9 a0 30 e8 60 b0 28 8b 5d 77";

            //# p's CRT exponent dP:
            dp = @"
            0e 12 bf 17 18 e9 ce f5 59 9b a1 c3 88 2f e8 04
            6a 90 87 4e ef ce 8f 2c cc 20 e4 f2 74 1f b0 a3
            3a 38 48 ae c9 c9 30 5f be cb d2 d7 68 19 96 7d
            46 71 ac c6 43 1e 40 37 96 8d b3 78 78 e6 95 c1";

            //# q's CRT exponent dQ:
            dq = @"
            95 29 7b 0f 95 a2 fa 67 d0 07 07 d6 09 df d4 fc
            05 c8 9d af c2 ef 6d 6e a5 5b ec 77 1e a3 33 73
            4d 92 51 e7 90 82 ec da 86 6e fe f1 3c 45 9e 1a
            63 13 86 b7 e3 54 c8 99 f5 f1 12 ca 85 d7 15 83";

            //# CRT coefficient qInv:
            qinv = @"
            4f 45 6c 50 24 93 bd c0 ed 2a b7 56 a3 a6 ed 4d
            67 35 2a 69 7d 42 16 e9 32 12 b1 27 a6 3d 54 11
            ce 6f a9 8d 5d be fd 73 26 3e 37 28 14 27 43 81
            81 66 ed 7d d6 36 87 dd 2a 8c a1 d2 f4 fb d8 e1";
        }

        private static void SetupKeysExample2(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# RSA modulus n:
            n = @"
            01 94 7c 7f ce 90 42 5f 47 27 9e 70 85 1f 25 d5
            e6 23 16 fe 8a 1d f1 93 71 e3 e6 28 e2 60 54 3e
            49 01 ef 60 81 f6 8c 0b 81 41 19 0d 2a e8 da ba
            7d 12 50 ec 6d b6 36 e9 44 ec 37 22 87 7c 7c 1d
            0a 67 f1 4b 16 94 c5 f0 37 94 51 a4 3e 49 a3 2d
            de 83 67 0b 73 da 91 a1 c9 9b c2 3b 43 6a 60 05
            5c 61 0f 0b af 99 c1 a0 79 56 5b 95 a3 f1 52 66
            32 d1 d4 da 60 f2 0e da 25 e6 53 c4 f0 02 76 6f
            45
            ";

            //# RSA public exponent e:
            e = @"
            01 00 01
            ";

            //# RSA private exponent d:
            d = @"
            08 23 f2 0f ad b5 da 89 08 8a 9d 00 89 3e 21 fa
            4a 1b 11 fb c9 3c 64 a3 be 0b aa ea 97 fb 3b 93
            c3 ff 71 37 04 c1 9c 96 3c 1d 10 7a ae 99 05 47
            39 f7 9e 02 e1 86 de 86 f8 7a 6d de fe a6 d8 cc
            d1 d3 c8 1a 47 bf a7 25 5b e2 06 01 a4 a4 b2 f0
            8a 16 7b 5e 27 9d 71 5b 1b 45 5b dd 7e ab 24 59
            41 d9 76 8b 9a ce fb 3c cd a5 95 2d a3 ce e7 25
            25 b4 50 16 63 a8 ee 15 c9 e9 92 d9 24 62 fe 39
            ";

            //# Prime p:
            p = @"
            01 59 db de 04 a3 3e f0 6f b6 08 b8 0b 19 0f 4d
            3e 22 bc c1 3a c8 e4 a0 81 03 3a bf a4 16 ed b0
            b3 38 aa 08 b5 73 09 ea 5a 52 40 e7 dc 6e 54 37
            8c 69 41 4c 31 d9 7d db 1f 40 6d b3 76 9c c4 1a
            43
            ";

            //# Prime q:
            q = @"
            01 2b 65 2f 30 40 3b 38 b4 09 95 fd 6f f4 1a 1a
            cc 8a da 70 37 32 36 b7 20 2d 39 b2 ee 30 cf b4
            6d b0 95 11 f6 f3 07 cc 61 cc 21 60 6c 18 a7 5b
            8a 62 f8 22 df 03 1b a0 df 0d af d5 50 6f 56 8b
            d7
            ";

            //# p's CRT exponent dP:
            dp = @"
            43 6e f5 08 de 73 65 19 c2 da 4c 58 0d 98 c8 2c
            b7 45 2a 3f b5 ef ad c3 b9 c7 78 9a 1b c6 58 4f
            79 5a dd bb d3 24 39 c7 46 86 55 2e cb 6c 2c 30
            7a 4d 3a f7 f5 39 ee c1 57 24 8c 7b 31 f1 a2 55
            ";

            //# q's CRT exponent dQ:
            dq = @"
            01 2b 15 a8 9f 3d fb 2b 39 07 3e 73 f0 2b dd 0c
            1a 7b 37 9d d4 35 f0 5c dd e2 ef f9 e4 62 94 8b
            7c ec 62 ee 90 50 d5 e0 81 6e 07 85 a8 56 b4 91
            08 dc b7 5f 36 83 87 4d 1c a6 32 9a 19 01 30 66
            ff
            ";

            //# CRT coefficient qInv:
            qinv = @"
            02 70 db 17 d5 91 4b 01 8d 76 11 8b 24 38 9a 73
            50 ec 83 6b 00 63 a2 17 21 23 6f d8 ed b6 d8 9b
            51 e7 ee b8 7b 61 1b 71 32 cb 7e a7 35 6c 23 15
            1c 1e 77 51 50 7c 78 6d 9e e1 79 41 70 a8 c8 e8
            ";
        }

        private static void SetupKeysExample3(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# ==================================
            //# Example 3: A 1026-bit RSA Key Pair
            //# ==================================

            //# ------------------------------
            //# Components of the RSA Key Pair
            //# ------------------------------

            //# RSA modulus n:
            n = @"
02 b5 8f ec 03 9a 86 07 00 a4 d7 b6 46 2f 93 e6
cd d4 91 16 1d dd 74 f4 e8 10 b4 0e 3c 16 52 00
6a 5c 27 7b 27 74 c1 13 05 a4 cb ab 5a 78 ef a5
7e 17 a8 6d f7 a3 fa 36 fc 4b 1d 22 49 f2 2e c7
c2 dd 6a 46 32 32 ac ce a9 06 d6 6e be 80 b5 70
4b 10 72 9d a6 f8 33 23 4a bb 5e fd d4 a2 92 cb
fa d3 3b 4d 33 fa 7a 14 b8 c3 97 b5 6e 3a cd 21
20 34 28 b7 7c df a3 3a 6d a7 06 b3 d8 b0 fc 43
e9
            ";

            //# RSA public exponent e:
            e = @"
01 00 01
            ";

            //# RSA private exponent d:
            d = @"
15 b4 8a 5b 56 83 a9 46 70 e2 3b 57 18 f8 14 fa
0e 13 f8 50 38 f5 07 11 18 2c ba 61 51 05 81 f3
d2 2c 7e 23 2e f9 37 e2 2e 55 1d 68 b8 6e 2f 8c
b1 aa d8 be 2e 48 8f 5d f7 ef d2 79 e3 f5 68 d4
ea f3 6f 80 cf 71 41 ac e6 0f cc 91 13 fb 6c 4a
84 1f d5 0b bc 7c 51 2f fc be ff 21 48 7a a8 11
eb 3c a8 c6 20 05 34 6a 86 de 86 bf a1 d8 a9 48
fd 3f 34 8c 22 ea ad f3 33 c3 ce 6c e1 32 08 fd
            ";

            //# Prime p:
            p = @"
01 bf 01 d2 16 d7 35 95 cf 02 70 c2 be b7 8d 40
a0 d8 44 7d 31 da 91 9a 98 3f 7e ea 78 1b 77 d8
5f e3 71 b3 e9 37 3e 7b 69 21 7d 31 50 a0 2d 89
58 de 7f ad 9d 55 51 60 95 8b 44 54 12 7e 0e 7e
af
            ";

            //# Prime q:
            q = @"
01 8d 33 99 65 81 66 db 38 29 81 6d 7b 29 54 16
75 9e 9c 91 98 7f 5b 2d 8a ec d6 3b 04 b4 8b d7
b2 fc f2 29 bb 7f 8a 6d c8 8b a1 3d d2 e3 9a d5
5b 6d 1a 06 16 07 08 f9 70 0b e8 0b 8f d3 74 4c
e7
            ";

            //# p's CRT exponent dP:
            dp = @"
06 c0 a2 49 d2 0a 6f 2e e7 5c 88 b4 94 d5 3f 6a
ae 99 aa 42 7c 88 c2 8b 16 3a 76 94 45 e5 f3 90
cf 40 c2 74 fd 6e a6 32 9a 5c e7 c7 ce 03 a2 15
83 96 ee 2a 78 45 78 6e 09 e2 88 5a 97 28 e4 e5
            ";

            //# q's CRT exponent dQ:
            dq = @"
d1 d2 7c 29 fe dd 92 d8 6c 34 8e dd 0c cb fa c1
4f 74 6e 05 1c e1 d1 81 1d f3 5d 61 f2 ee 1c 97
d4 bf 28 04 80 2f 64 27 18 7b a8 e9 0a 8a f4 42
43 b4 07 9b 03 44 5e 60 2e 29 fa 51 93 e6 4f e9
            ";

            //# CRT coefficient qInv:
            qinv = @"
8c b2 f7 56 bd 89 41 b1 d3 b7 70 e5 ad 31 ee 37
3b 28 ac da 69 ff 9b 6f 40 fe 57 8b 9f 1a fb 85
83 6f 96 27 d3 7a cf f7 3c 27 79 e6 34 bb 26 01
1c 2c 8f 7f 33 61 ae 2a 9e a6 5e d6 89 e3 63 9a
            ";
        }

        private static void SetupKeysExample4(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# ==================================
            //# Example 4: A 1027-bit RSA Key Pair
            //# ==================================

            //# ------------------------------
            //# Components of the RSA Key Pair
            //# ------------------------------

            //# RSA modulus n:
            n = @"
05 12 40 b6 cc 00 04 fa 48 d0 13 46 71 c0 78 c7
c8 de c3 b3 e2 f2 5b c2 56 44 67 33 9d b3 88 53
d0 6b 85 ee a5 b2 de 35 3b ff 42 ac 2e 46 bc 97
fa e6 ac 96 18 da 95 37 a5 c8 f5 53 c1 e3 57 62
59 91 d6 10 8d cd 78 85 fb 3a 25 41 3f 53 ef ca
d9 48 cb 35 cd 9b 9a e9 c1 c6 76 26 d1 13 d5 7d
de 4c 5b ea 76 bb 5b b7 de 96 c0 0d 07 37 2e 96
85 a6 d7 5c f9 d2 39 fa 14 8d 70 93 1b 5f 3f b0
39
            ";

            //# RSA public exponent e:
            e = @"
01 00 01
            ";

            //# RSA private exponent d:
            d = @"
04 11 ff ca 3b 7c a5 e9 e9 be 7f e3 8a 85 10 5e
35 38 96 db 05 c5 79 6a ec d2 a7 25 16 1e b3 65
1c 86 29 a9 b8 62 b9 04 d7 b0 c7 b3 7f 8c b5 a1
c2 b5 40 01 01 8a 00 a1 eb 2c af e4 ee 4e 94 92
c3 48 bc 2b ed ab 4b 9e bb f0 64 e8 ef f3 22 b9
00 9f 8e ec 65 39 05 f4 0d f8 8a 3c dc 49 d4 56
7f 75 62 7d 41 ac a6 24 12 9b 46 a0 b7 c6 98 e5
e6 5f 2b 7b a1 02 c7 49 a1 01 35 b6 54 0d 04 01
            ";

            //# Prime p:
            p = @"
02 74 58 c1 9e c1 63 69 19 e7 36 c9 af 25 d6 09
a5 1b 8f 56 1d 19 c6 bf 69 43 dd 1e e1 ab 8a 4a
3f 23 21 00 bd 40 b8 8d ec c6 ba 23 55 48 b6 ef
79 2a 11 c9 de 82 3d 0a 79 22 c7 09 5b 6e ba 57
01
            ";

            //# Prime q:
            q = @"
02 10 ee 9b 33 ab 61 71 6e 27 d2 51 bd 46 5f 4b
35 a1 a2 32 e2 da 00 90 1c 29 4b f2 23 50 ce 49
0d 09 9f 64 2b 53 75 61 2d b6 3b a1 f2 03 86 49
2b f0 4d 34 b3 c2 2b ce b9 09 d1 34 41 b5 3b 51
39
            ";

            //# p's CRT exponent dP:
            dp = @"
39 fa 02 8b 82 6e 88 c1 12 1b 75 0a 8b 24 2f a9
a3 5c 5b 66 bd fd 1f a6 37 d3 cc 48 a8 4a 4f 45
7a 19 4e 77 27 e4 9f 7b cc 6e 5a 5a 41 26 57 fc
47 0c 73 22 eb c3 74 16 ef 45 8c 30 7a 8c 09 01
            ";

            //# q's CRT exponent dQ:
            dq = @"
01 5d 99 a8 41 95 94 39 79 fa 9e 1b e2 c3 c1 b6
9f 43 2f 46 fd 03 e4 7d 5b ef bb bf d6 b1 d1 37
1d 83 ef b3 30 a3 e0 20 94 2b 2f ed 11 5e 5d 02
be 24 fd 92 c9 01 9d 1c ec d6 dd 4c f1 e5 4c c8
99
            ";

            //# CRT coefficient qInv:
            qinv = @"
01 f0 b7 01 51 70 b3 f5 e4 22 23 ba 30 30 1c 41
a6 d8 7c bb 70 e3 0c b7 d3 c6 7d 25 47 3d b1 f6
cb f0 3e 3f 91 26 e3 e9 79 68 27 9a 86 5b 2c 2b
42 65 24 cf c5 2a 68 3d 31 ed 30 eb 98 4b e4 12
ba
            ";
        }

        private static void RunOneCase(string testcase, string n, string e, string d, string p, string q, string dp, string dq, string qinv, string message, string seed, string encryption)
        {
            Factory.Instance.Singleton<IRandomGenerator>(() => new InjectedBytesRandomGenerator(seed.FromHex()));

            IAsymmetricKeyPair keyPair = Factory.Instance.Singleton<IAsymmetricFactory>().CreateKeyPair(PositiveFromHex(n), PositiveFromHex(e), PositiveFromHex(d), PositiveFromHex(p), PositiveFromHex(q), PositiveFromHex(dp), PositiveFromHex(dq), PositiveFromHex(qinv));

            byte[] cipher = keyPair.PublicKey.Transform(message.FromHex());

            Assert.That(cipher, Is.EquivalentTo(encryption.FromHex()), testcase);
        }
    }
}