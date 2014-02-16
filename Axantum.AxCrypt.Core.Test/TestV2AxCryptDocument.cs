#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestV2AxCryptDocument
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestWriteWithHmac()
        {
            byte[] output;
            byte[] hmacKey;
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Encoding.UTF8.GetBytes("Just a few bytes testing!");
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    V2AxCryptDocument document = new V2AxCryptDocument(new V2AesCrypto(new AesKey(256), new AesIV()), 100);
                    document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithoutCompression, new ProgressContext());
                    output = outputStream.ToArray();
                    hmacKey = document.DocumentHeaders.GetHmacKey();
                }
            }

            byte[] hmacBytesFromHeaders = new byte[V2Hmac.RequiredLength];
            Array.Copy(output, output.Length - V2Hmac.RequiredLength, hmacBytesFromHeaders, 0, V2Hmac.RequiredLength);
            V2Hmac hmacFromHeaders = new V2Hmac(hmacBytesFromHeaders);

            byte[] dataToHmac = new byte[output.Length - (V2Hmac.RequiredLength + 5)];
            Array.Copy(output, 0, dataToHmac, 0, dataToHmac.Length);

            HMACSHA512 hmac = new HMACSHA512(hmacKey);
            hmac.TransformFinalBlock(dataToHmac, 0, dataToHmac.Length);
            V2Hmac hmacFromCalculation = new V2Hmac(hmac.Hash);

            Assert.That(hmacFromHeaders, Is.EqualTo(hmacFromCalculation));
        }

        [Test]
        public static void TestDecryptWithHmac()
        {
            AesKey key = new AesKey(256);
            AesIV iv = new AesIV();
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Encoding.UTF8.GetBytes("Just a few more bytes testing!");
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    V2AxCryptDocument document = new V2AxCryptDocument(new V2AesCrypto(key, iv), 113);
                    document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithoutCompression, new ProgressContext());

                    outputStream.Position = 0;
                    document = new V2AxCryptDocument(new V2AesCrypto(key, iv));
                    Assert.That(document.Load(outputStream), Is.True);
                    string plain;
                    using (MemoryStream decryptedStream = new MemoryStream())
                    {
                        document.DecryptTo(decryptedStream, new ProgressContext());
                        plain = Encoding.UTF8.GetString(decryptedStream.ToArray());
                    }

                    Assert.That(plain, Is.EqualTo("Just a few more bytes testing!"));
                }
            }
        }
    }
}