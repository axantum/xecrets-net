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

        private static void RunOneCase(string testcase, string n, string e, string d, string p, string q, string dp, string dq, string qinv, string message, string seed, string encryption)
        {
            Factory.Instance.Singleton<IRandomGenerator>(() => new InjectedBytesRandomGenerator(seed.FromHex()));

            IAsymmetricKeyPair keyPair = Factory.Instance.Singleton<IAsymmetricFactory>().CreateKeyPair(PositiveFromHex(n), PositiveFromHex(e), PositiveFromHex(d), PositiveFromHex(p), PositiveFromHex(q), PositiveFromHex(dp), PositiveFromHex(dq), PositiveFromHex(qinv));

            byte[] cipher = keyPair.PublicKey.Transform(message.FromHex());

            Assert.That(cipher, Is.EquivalentTo(encryption.FromHex()), testcase);
        }
    }
}