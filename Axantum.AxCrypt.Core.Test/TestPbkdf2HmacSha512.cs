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
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pbkdf")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sha")]
    public static class TestPbkdf2HmacSha512
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

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCase1FromStackOverflow()
        {
            byte[] expected = "867f70cf1ade02cff3752599a3a53dc4af34c7a669815ae5d513554e1c8cf252c02d470a285a0501bad999bfe943c08f050235d7d68b1da55e63f73b60a57fce".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("password", Encoding.ASCII.GetBytes("salt"), 1).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCase2FromStackOverflow()
        {
            byte[] expected = "e1d9c16aa681708a45f5c7c4e215ceb66e011a2e9f0040713f18aefdb866d53cf76cab2868a39b9f7840edce4fef5a82be67335c77a6068e04112754f27ccf4e".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("password", Encoding.ASCII.GetBytes("salt"), 2).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCase3FromStackOverflow()
        {
            byte[] expected = "d197b1b33db0143e018b12f3d1d1479e6cdebdcc97c5c0f87f6902e072f457b5143f30602641b3d55cd335988cb36b84376060ecd532e039b742a239434af2d5".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("password", Encoding.ASCII.GetBytes("salt"), 4096).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCase4FromStackOverflow()
        {
            byte[] expected = "8c0511f4c6e597c6ac6315d8f0362e225f3c501495ba23b868c005174dc4ee71115b59f9e60cd9532fa33e0f75aefe30225c583a186cd82bd4daea9724a3d3b8".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passwordPASSWORDpassword", Encoding.ASCII.GetBytes("saltSALTsaltSALTsaltSALTsaltSALTsalt"), 4096).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest1AFromStackOverflow()
        {
            byte[] expected = "CBE6088AD4359AF42E603C2A33760EF9D4017A7B2AAD10AF46F992C660A0B461ECB0DC2A79C2570941BEA6A08D15D6887E79F32B132E1C134E9525EEDDD744FA".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTT", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD"), 1).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest1BFromStackOverflow()
        {
            byte[] expected = "ACCDCD8798AE5CD85804739015EF2A11E32591B7B7D16F76819B30B0D49D80E1ABEA6C9822B80A1FDFE421E26F5603ECA8A47A64C9A004FB5AF8229F762FF41F".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTT", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD"), 100000).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest2AFromStackOverflow()
        {
            byte[] expected = "8E5074A9513C1F1512C9B1DF1D8BFFA9D8B4EF9105DFC16681222839560FB63264BED6AABF761F180E912A66E0B53D65EC88F6A1519E14804EBA6DC9DF137007".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTl", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2"), 1).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest2BFromStackOverflow()
        {
            byte[] expected = "594256B0BD4D6C9F21A87F7BA5772A791A10E6110694F44365CD94670E57F1AECD797EF1D1001938719044C7F018026697845EB9AD97D97DE36AB8786AAB5096".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTl", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2"), 100000).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest3AFromStackOverflow()
        {
            byte[] expected = "A6AC8C048A7DFD7B838DA88F22C3FAB5BFF15D7CB8D83A62C6721A8FAF6903EAB6152CB7421026E36F2FFEF661EB4384DC276495C71B5CAB72E1C1A38712E56B".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlR", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2P"), 1).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest3BFromStackOverflow()
        {
            byte[] expected = "94FFC2B1A390B7B8A9E6A44922C330DB2B193ADCF082EECD06057197F35931A9D0EC0EE5C660744B50B61F23119B847E658D179A914807F4B8AB8EB9505AF065".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlR", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2P"), 100000).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest4AFromStackOverflow()
        {
            byte[] expected = "E2CCC7827F1DD7C33041A98906A8FD7BAE1920A55FCB8F831683F14F1C3979351CB868717E5AB342D9A11ACF0B12D3283931D609B06602DA33F8377D1F1F9902".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE5", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJe"), 1).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest4BFromStackOverflow()
        {
            byte[] expected = "07447401C85766E4AED583DE2E6BF5A675EABE4F3618281C95616F4FC1FDFE6ECBC1C3982789D4FD941D6584EF534A78BD37AE02555D9455E8F089FDB4DFB6BB".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE5", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJe"), 100000).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// We agree with the posted vectors and thus Python and SQL, but not OpenSSL which appears to be in the wrong.
        /// </summary>
        [Test]
        public static void TestCaseLongTest5AFromStackOverflow()
        {
            byte[] expected = "B029A551117FF36977F283F579DC7065B352266EA243BDD3F920F24D4D141ED8B6E02D96E2D3BDFB76F8D77BA8F4BB548996AD85BB6F11D01A015CE518F9A717".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJem"), 1).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest5BFromStackOverflow()
        {
            byte[] expected = "31F5CC83ED0E948C05A15735D818703AAA7BFF3F09F5169CAF5DBA6602A05A4D5CFF5553D42E82E40516D6DC157B8DAEAE61D3FEA456D964CB2F7F9A63BBBDB5".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJem"), 100000).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// We agree with the posted vectors and thus Python and SQL, but not OpenSSL which appears to be in the wrong.
        /// </summary>
        [Test]
        public static void TestCaseLongTest6AFromStackOverflow()
        {
            byte[] expected = "28B8A9F644D6800612197BB74DF460272E2276DE8CC07AC4897AC24DBC6EB77499FCAF97415244D9A29DA83FC347D09A5DBCFD6BD63FF6E410803DCA8A900AB6".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57U", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemk"), 1).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest6BFromStackOverflow()
        {
            byte[] expected = "056BC9072A356B7D4DA60DD66F5968C2CAA375C0220EDA6B47EF8E8D105ED68B44185FE9003FBBA49E2C84240C9E8FD3F5B2F4F6512FD936450253DB37D10028".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57U", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemk"), 100000).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// We agree with the posted vectors and thus Python and SQL, but not OpenSSL which appears to be in the wrong.
        /// </summary>
        [Test]
        public static void TestCaseLongTest7AFromStackOverflow()
        {
            byte[] expected = "16226C85E4F8D604573008BFE61C10B6947B53990450612DD4A3077F7DEE2116229E68EFD1DF6D73BD3C6D07567790EEA1E8B2AE9A1B046BE593847D9441A1B7".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi0", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy"), 1).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest7BFromStackOverflow()
        {
            byte[] expected = "70CF39F14C4CAF3C81FA288FB46C1DB52D19F72722F7BC84F040676D3371C89C11C50F69BCFBC3ACB0AB9E92E4EF622727A916219554B2FA121BEDDA97FF3332".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi0", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy"), 100000).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// We agree with the posted vectors and thus Python and SQL, but not OpenSSL which appears to be in the wrong.
        /// </summary>
        [Test]
        public static void TestCaseLongTest8AFromStackOverflow()
        {
            byte[] expected = "880C58C316D3A5B9F05977AB9C60C10ABEEBFAD5CE89CAE62905C1C4F80A0A098D82F95321A6220F8AECCFB45CE6107140899E8D655306AE6396553E2851376C".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6"), 1).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest8BFromStackOverflow()
        {
            byte[] expected = "2668B71B3CA56136B5E87F30E098F6B4371CB5ED95537C7A073DAC30A2D5BE52756ADF5BB2F4320CB11C4E16B24965A9C790DEF0CBC62906920B4F2EB84D1D4A".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6"), 100000).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// We agree with the posted vectors and thus Python and SQL, but not OpenSSL which appears to be in the wrong.
        /// </summary>
        [Test]
        public static void TestCaseLongTest9AFromStackOverflow()
        {
            byte[] expected = "93B9BA8283CC17D50EF3B44820828A258A996DE258225D24FB59990A6D0DE82DFB3FE2AC201952100E4CC8F06D883A9131419C0F6F5A6ECB8EC821545F14ADF1".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04U", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6P"), 1).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest9BFromStackOverflow()
        {
            byte[] expected = "2575B485AFDF37C260B8F3386D33A60ED929993C9D48AC516EC66B87E06BE54ADE7E7C8CB3417C81603B080A8EEFC56072811129737CED96236B9364E22CE3A5".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04U", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6P"), 100000).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest10AFromStackOverflow()
        {
            byte[] expected = "384BCD6914407E40C295D1037CF4F990E8F0E720AF43CB706683177016D36D1A14B3A7CF22B5DF8D5D7D44D69610B64251ADE2E7AB54A3813A89935592E391BF".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04Uz3ebEAhzZ4ve1A2wg5CnLXdZC5Y7gwfVgbEgZSTmoYQSzC5OW4dfrjqiwApTACO6xoOL1AjWj6X6f6qFfF8TVmOzU9RhOd1N4QtzWI4fP6FYttNz5FuLdtYVXWVXH2Tf7I9fieMeWCHTMkM4VcmQyQHpbcP8MEb5f1g6Ckg5xk3HQr3wMBvQcOHpCPy1K8HCM7a5wkPDhgVA0BVmwNpsRIbDQZRtHK6dT6bGyalp6gbFZBuBHwD86gTzkrFY7HkOVrgc0gJcGJZe65Ce8v4Jn5OzkuVsiU8efm2Pw2RnbpWSAr7SkVdCwXK2XSJDQ5fZ4HBEz9VTFYrG23ELuLjvx5njOLNgDAJuf5JB2tn4nMjjcnl1e8qcYVwZqFzEv2zhLyDWMkV4tzl4asLnvyAxTBkxPRZj2pRABWwb3kEofpsHYxMTAn38YSpZreoXipZWBnu6HDURaruXaIPYFPYHl9Ls9wsuD7rzaGfbOyfVgLIGK5rODphwRA7lm88bGKY8b7tWOtepyEvaLxMI7GZF5ScwpZTYeEDNUKPzvM2Im9zehIaznpguNdNXNMLWnwPu4H6zEvajkw3G3ucSiXKmh6XNe3hkdSANm3vnxzRXm4fcuzAx68IElXE2bkGFElluDLo6EsUDWZ4JIWBVaDwYdJx8uCXbQdoifzCs5kuuClaDaDqIhb5hJ2WR8mxiueFsS0aDGdIYmye5svmNmzQxFmdOkHoF7CfwuU1yy4uEEt9vPSP2wFp1dyaMvJW68vtB4kddLmI6gIgVVcT6ZX1Qm6WsusPrdisPLB2ScodXojCbL3DLj6PKG8QDVMWTrL1TpafT2wslRledWIhsTlv2mI3C066WMcTSwKLXdEDhVvFJ6ShiLKSN7gnRrlE0BnAw", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6PlBdILBOkKUB6TGTPJXh1tpdOHTG6KuIvcbQp9qWjaf1uxAKgiTtYRIHhxjJI2viVa6fDZ67QOouOaf2RXQhpsWaTtAVnff6PIFcvJhdPDFGV5nvmZWoCZQodj6yXRDHPw9PyF0iLYm9uFtEunlAAxGB5qqea4X5tZvB1OfLVwymY3a3JPjdxTdvHxCHbqqE0zip61JNqdmeWxGtlRBC6CGoCiHO4XxHCntQBRJDcG0zW7joTdgtTBarsQQhlLXBGMNBSNmmTbDf3hFtawUBCJH18IAiRMwyeQJbJ2bERsY3MVRPuYCf4Au7gN72iGh1lRktSQtEFye7pO46kMXRrEjHQWXInMzzy7X2StXUzHVTFF2VdOoKn0WUqFNvB6PF7qIsOlYKj57bi1Psa34s85WxMSbTkhrd7VHdHZkTVaWdraohXYOePdeEvIwObCGEXkETUzqM5P2yzoBOJSdjpIYaa8zzdLD3yrb1TwCZuJVxsrq0XXY6vErU4QntsW0972XmGNyumFNJiPm4ONKh1RLvS1kddY3nm8276S4TUuZfrRQO8QxZRNuSaZI8JRZp5VojB5DktuMxAQkqoPjQ5Vtb6oXeOyY591CB1MEW1fLTCs0NrL321SaNRMqza1ETogAxpEiYwZ6pIgnMmSqNMRdZnCqA4gMWw1lIVATWK83OCeicNRUNOdfzS7A8vbLcmvKPtpOFvhNzwrrUdkvuKvaYJviQgeR7snGetO9JLCwIlHIj52gMCNU18d32SJl7Xomtl3wIe02SMvq1i1BcaX7lXioqWGmgVqBWU3fsUuGwHi6RUKCCQdEOBfNo2WdpFaCflcgnn0O6jVHCqkv8cQk81AqS00rAmHGCNTwyA6Tq5TXoLlDnC8gAQjDUsZp0z"), 1).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public static void TestCaseLongTest10BFromStackOverflow()
        {
            byte[] expected = "B8674F6C0CC9F8CF1F1874534FD5AF01FC1504D76C2BC2AA0A75FE4DD5DFD1DAF60EA7C85F122BCEEB8772659D601231607726998EAC3F6AAB72EFF7BA349F7F".FromHex();
            byte[] actual = new Pbkdf2HmacSha512("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04Uz3ebEAhzZ4ve1A2wg5CnLXdZC5Y7gwfVgbEgZSTmoYQSzC5OW4dfrjqiwApTACO6xoOL1AjWj6X6f6qFfF8TVmOzU9RhOd1N4QtzWI4fP6FYttNz5FuLdtYVXWVXH2Tf7I9fieMeWCHTMkM4VcmQyQHpbcP8MEb5f1g6Ckg5xk3HQr3wMBvQcOHpCPy1K8HCM7a5wkPDhgVA0BVmwNpsRIbDQZRtHK6dT6bGyalp6gbFZBuBHwD86gTzkrFY7HkOVrgc0gJcGJZe65Ce8v4Jn5OzkuVsiU8efm2Pw2RnbpWSAr7SkVdCwXK2XSJDQ5fZ4HBEz9VTFYrG23ELuLjvx5njOLNgDAJuf5JB2tn4nMjjcnl1e8qcYVwZqFzEv2zhLyDWMkV4tzl4asLnvyAxTBkxPRZj2pRABWwb3kEofpsHYxMTAn38YSpZreoXipZWBnu6HDURaruXaIPYFPYHl9Ls9wsuD7rzaGfbOyfVgLIGK5rODphwRA7lm88bGKY8b7tWOtepyEvaLxMI7GZF5ScwpZTYeEDNUKPzvM2Im9zehIaznpguNdNXNMLWnwPu4H6zEvajkw3G3ucSiXKmh6XNe3hkdSANm3vnxzRXm4fcuzAx68IElXE2bkGFElluDLo6EsUDWZ4JIWBVaDwYdJx8uCXbQdoifzCs5kuuClaDaDqIhb5hJ2WR8mxiueFsS0aDGdIYmye5svmNmzQxFmdOkHoF7CfwuU1yy4uEEt9vPSP2wFp1dyaMvJW68vtB4kddLmI6gIgVVcT6ZX1Qm6WsusPrdisPLB2ScodXojCbL3DLj6PKG8QDVMWTrL1TpafT2wslRledWIhsTlv2mI3C066WMcTSwKLXdEDhVvFJ6ShiLKSN7gnRrlE0BnAw", Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6PlBdILBOkKUB6TGTPJXh1tpdOHTG6KuIvcbQp9qWjaf1uxAKgiTtYRIHhxjJI2viVa6fDZ67QOouOaf2RXQhpsWaTtAVnff6PIFcvJhdPDFGV5nvmZWoCZQodj6yXRDHPw9PyF0iLYm9uFtEunlAAxGB5qqea4X5tZvB1OfLVwymY3a3JPjdxTdvHxCHbqqE0zip61JNqdmeWxGtlRBC6CGoCiHO4XxHCntQBRJDcG0zW7joTdgtTBarsQQhlLXBGMNBSNmmTbDf3hFtawUBCJH18IAiRMwyeQJbJ2bERsY3MVRPuYCf4Au7gN72iGh1lRktSQtEFye7pO46kMXRrEjHQWXInMzzy7X2StXUzHVTFF2VdOoKn0WUqFNvB6PF7qIsOlYKj57bi1Psa34s85WxMSbTkhrd7VHdHZkTVaWdraohXYOePdeEvIwObCGEXkETUzqM5P2yzoBOJSdjpIYaa8zzdLD3yrb1TwCZuJVxsrq0XXY6vErU4QntsW0972XmGNyumFNJiPm4ONKh1RLvS1kddY3nm8276S4TUuZfrRQO8QxZRNuSaZI8JRZp5VojB5DktuMxAQkqoPjQ5Vtb6oXeOyY591CB1MEW1fLTCs0NrL321SaNRMqza1ETogAxpEiYwZ6pIgnMmSqNMRdZnCqA4gMWw1lIVATWK83OCeicNRUNOdfzS7A8vbLcmvKPtpOFvhNzwrrUdkvuKvaYJviQgeR7snGetO9JLCwIlHIj52gMCNU18d32SJl7Xomtl3wIe02SMvq1i1BcaX7lXioqWGmgVqBWU3fsUuGwHi6RUKCCQdEOBfNo2WdpFaCflcgnn0O6jVHCqkv8cQk81AqS00rAmHGCNTwyA6Tq5TXoLlDnC8gAQjDUsZp0z"), 100000).GetBytes();

            Assert.That(actual.IsEquivalentTo(expected));
        }

        [Test]
        public static void TestConstructorWithBadArguments()
        {
            Pbkdf2HmacSha512 pbkdf;
            Assert.Throws<ArgumentNullException>(() => pbkdf = new Pbkdf2HmacSha512("passphrase", null, 0));
            Assert.Throws<ArgumentNullException>(() => pbkdf = new Pbkdf2HmacSha512(null, new byte[0], 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => pbkdf = new Pbkdf2HmacSha512("passphrase", new byte[0], 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => pbkdf = new Pbkdf2HmacSha512("passphrase", new byte[0], -1));

            Assert.DoesNotThrow(() => pbkdf = new Pbkdf2HmacSha512("passphrase", new byte[0], 10));
        }

        [Test]
        public static void TestGetBytesTwice()
        {
            Pbkdf2HmacSha512 pbkdf = new Pbkdf2HmacSha512("passphrase", new byte[0], 10);

            byte[] bytes = pbkdf.GetBytes();
            Assert.Throws<InternalErrorException>(() => bytes = pbkdf.GetBytes());
        }
    }
}