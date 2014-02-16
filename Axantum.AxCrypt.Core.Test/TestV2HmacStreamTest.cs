using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2HmacStreamTest
    {
        [SetUp]
        public static void Setup()
        {
        }

        [TearDown]
        public static void Teardown()
        {
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc", Justification = "This is well-known acronymn")]
        public static void Rfc4231TestCase1()
        {
            byte[] key = "0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b".FromHex();
            byte[] data = "4869205468657265".FromHex();
            byte[] hmac_sha_512 = "87aa7cdea5ef619d4ff0b4241a1d6cb02379f4e2ce4ec2787ad0b30545e17cdedaa833b7d6b8a702038b274eaea3f4e4be9d914eeb61f1702e696c203a126854".FromHex();

            byte[] result;
            using (V2HmacStream stream = new V2HmacStream(key))
            {
                stream.Write(data, 0, data.Length);
                result = stream.Hmac.GetBytes();
            }

            Assert.That(result, Is.EquivalentTo(hmac_sha_512));
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc", Justification = "This is well-known acronymn")]
        public static void Rfc4231TestCase2()
        {
            byte[] key = "4a656665".FromHex();
            byte[] data = "7768617420646f2079612077616e7420666f72206e6f7468696e673f".FromHex();
            byte[] hmac_sha_512 = "164b7a7bfcf819e2e395fbe73b56e0a387bd64222e831fd610270cd7ea2505549758bf75c05a994a6d034f65f8f0e6fdcaeab1a34d4a6b4b636e070a38bce737".FromHex();

            byte[] result;
            using (V2HmacStream stream = new V2HmacStream(key))
            {
                stream.Write(data, 0, data.Length);
                result = stream.Hmac.GetBytes();
            }

            Assert.That(result, Is.EquivalentTo(hmac_sha_512));
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc", Justification = "This is well-known acronymn")]
        public static void Rfc4231TestCase3()
        {
            byte[] key = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa".FromHex();
            byte[] data = "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd".FromHex();
            byte[] hmac_sha_512 = "fa73b0089d56a284efb0f0756c890be9b1b5dbdd8ee81a3655f83e33b2279d39bf3e848279a722c806b485a47e67c807b946a337bee8942674278859e13292fb".FromHex();

            byte[] result;
            using (V2HmacStream stream = new V2HmacStream(key))
            {
                stream.Write(data, 0, data.Length);
                result = stream.Hmac.GetBytes();
            }

            Assert.That(result, Is.EquivalentTo(hmac_sha_512));
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc", Justification = "This is well-known acronymn")]
        public static void Rfc4231TestCase4()
        {
            byte[] key = "0102030405060708090a0b0c0d0e0f10 111213141516171819".FromHex();
            byte[] data = "cdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcd cdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcd cdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcd cdcd".FromHex();
            byte[] hmac_sha_512 = "b0ba465637458c6990e5a8c5f61d4af7 e576d97ff94b872de76f8050361ee3db a91ca5c11aa25eb4d679275cc5788063 a5f19741120c4f2de2adebeb10a298dd".FromHex();

            byte[] result;
            using (V2HmacStream stream = new V2HmacStream(key))
            {
                stream.Write(data, 0, data.Length);
                result = stream.Hmac.GetBytes();
            }

            Assert.That(result, Is.EquivalentTo(hmac_sha_512));
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc", Justification = "This is well-known acronymn")]
        public static void Rfc4231TestCase5()
        {
            byte[] key = "0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c 0c0c0c0c".FromHex();
            byte[] data = "546573742057697468205472756e6361 74696f6e".FromHex();
            byte[] hmac_sha_512 = "415fad6271580a531d4179bc891d87a6".FromHex();

            byte[] result;
            using (V2HmacStream stream = new V2HmacStream(key))
            {
                stream.Write(data, 0, data.Length);
                result = new byte[16];
                Array.Copy(stream.Hmac.GetBytes(), 0, result, 0, 16);
            }

            Assert.That(result, Is.EquivalentTo(hmac_sha_512));
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc", Justification = "This is well-known acronymn")]
        public static void Rfc4231TestCase6()
        {
            byte[] key = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaa".FromHex();
            byte[] data = "54657374205573696e67204c61726765 72205468616e20426c6f636b2d53697a 65204b6579202d2048617368204b6579 204669727374".FromHex();
            byte[] hmac_sha_512 = "80b24263c7c1a3ebb71493c1dd7be8b4 9b46d1f41b4aeec1121b013783f8f352 6b56d037e05f2598bd0fd2215d6a1e52 95e64f73f63f0aec8b915a985d786598".FromHex();

            byte[] result;
            using (V2HmacStream stream = new V2HmacStream(key))
            {
                stream.Write(data, 0, data.Length);
                result = stream.Hmac.GetBytes();
            }

            Assert.That(result, Is.EquivalentTo(hmac_sha_512));
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc", Justification = "This is well-known acronymn")]
        public static void Rfc4231TestCase7()
        {
            byte[] key = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaa".FromHex();
            byte[] data = "54686973206973206120746573742075 73696e672061206c6172676572207468 616e20626c6f636b2d73697a65206b65 7920616e642061206c61726765722074 68616e20626c6f636b2d73697a652064 6174612e20546865206b6579206e6565 647320746f2062652068617368656420 6265666f7265206265696e6720757365 642062792074686520484d414320616c 676f726974686d2e".FromHex();
            byte[] hmac_sha_512 = "e37b6a775dc87dbaa4dfa9f96e5e3ffd debd71f8867289865df5a32d20cdc944 b6022cac3c4982b10d5eeb55c3e4de15 134676fb6de0446065c97440fa8c6a58".FromHex();

            byte[] result;
            using (V2HmacStream stream = new V2HmacStream(key))
            {
                stream.Write(data, 0, data.Length);
                result = stream.Hmac.GetBytes();
            }

            Assert.That(result, Is.EquivalentTo(hmac_sha_512));
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc", Justification = "This is well-known acronymn")]
        public static void Rfc4231TestCase7WithChaining()
        {
            byte[] key = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaa".FromHex();
            byte[] data = "54686973206973206120746573742075 73696e672061206c6172676572207468 616e20626c6f636b2d73697a65206b65 7920616e642061206c61726765722074 68616e20626c6f636b2d73697a652064 6174612e20546865206b6579206e6565 647320746f2062652068617368656420 6265666f7265206265696e6720757365 642062792074686520484d414320616c 676f726974686d2e".FromHex();
            byte[] hmac_sha_512 = "e37b6a775dc87dbaa4dfa9f96e5e3ffd debd71f8867289865df5a32d20cdc944 b6022cac3c4982b10d5eeb55c3e4de15 134676fb6de0446065c97440fa8c6a58".FromHex();

            byte[] result;
            using (MemoryStream output = new MemoryStream())
            {
                using (V2HmacStream stream = new V2HmacStream(key, output))
                {
                    stream.Write(data, 0, data.Length);
                    result = stream.Hmac.GetBytes();
                }
                byte[] chained = output.ToArray();
                Assert.That(chained, Is.EquivalentTo(data));
            }

            Assert.That(result, Is.EquivalentTo(hmac_sha_512));
        }
    }
}