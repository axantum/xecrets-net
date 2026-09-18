#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Runtime;
using AxCrypt.Fake;
using Xecrets.Net.Cryptography;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    [TestFixture(CryptoImplementation.Xecrets)]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pbkdf")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sha")]
    public class TestPbkdf2HmacSha512
    {
        private CryptoImplementation _cryptoImplementation;

        public TestPbkdf2HmacSha512(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(_cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        private byte[] DeriveBytes(string password, Salt salt, int derivationIterations)
        {
            return _cryptoImplementation == CryptoImplementation.Xecrets
                ? new XecretsPbkdf2(password, salt.GetBytes(), derivationIterations).GetBytes()
                : new Pbkdf2HmacSha512(password, salt, derivationIterations).GetBytes();
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCase1FromStackOverflow()
        {
            byte[] expected = "867f70cf1ade02cff3752599a3a53dc4af34c7a669815ae5d513554e1c8cf252c02d470a285a0501bad999bfe943c08f050235d7d68b1da55e63f73b60a57fce".FromHex();
            byte[] actual = DeriveBytes("password", new Salt(Encoding.ASCII.GetBytes("salt")), 1);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCase2FromStackOverflow()
        {
            byte[] expected = "e1d9c16aa681708a45f5c7c4e215ceb66e011a2e9f0040713f18aefdb866d53cf76cab2868a39b9f7840edce4fef5a82be67335c77a6068e04112754f27ccf4e".FromHex();
            byte[] actual = DeriveBytes("password", new Salt(Encoding.ASCII.GetBytes("salt")), 2);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCase3FromStackOverflow()
        {
            byte[] expected = "d197b1b33db0143e018b12f3d1d1479e6cdebdcc97c5c0f87f6902e072f457b5143f30602641b3d55cd335988cb36b84376060ecd532e039b742a239434af2d5".FromHex();
            byte[] actual = DeriveBytes("password", new Salt(Encoding.ASCII.GetBytes("salt")), 4096);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCase4FromStackOverflow()
        {
            byte[] expected = "8c0511f4c6e597c6ac6315d8f0362e225f3c501495ba23b868c005174dc4ee71115b59f9e60cd9532fa33e0f75aefe30225c583a186cd82bd4daea9724a3d3b8".FromHex();
            byte[] actual = DeriveBytes("passwordPASSWORDpassword", new Salt(Encoding.ASCII.GetBytes("saltSALTsaltSALTsaltSALTsaltSALTsalt")), 4096);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest1AFromStackOverflow()
        {
            byte[] expected = "CBE6088AD4359AF42E603C2A33760EF9D4017A7B2AAD10AF46F992C660A0B461ECB0DC2A79C2570941BEA6A08D15D6887E79F32B132E1C134E9525EEDDD744FA".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTT", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD")), 1);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest1BFromStackOverflow()
        {
            byte[] expected = "ACCDCD8798AE5CD85804739015EF2A11E32591B7B7D16F76819B30B0D49D80E1ABEA6C9822B80A1FDFE421E26F5603ECA8A47A64C9A004FB5AF8229F762FF41F".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTT", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD")), 100000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest2AFromStackOverflow()
        {
            byte[] expected = "8E5074A9513C1F1512C9B1DF1D8BFFA9D8B4EF9105DFC16681222839560FB63264BED6AABF761F180E912A66E0B53D65EC88F6A1519E14804EBA6DC9DF137007".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTl", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2")), 1);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest2BFromStackOverflow()
        {
            byte[] expected = "594256B0BD4D6C9F21A87F7BA5772A791A10E6110694F44365CD94670E57F1AECD797EF1D1001938719044C7F018026697845EB9AD97D97DE36AB8786AAB5096".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTl", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2")), 100000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest3AFromStackOverflow()
        {
            byte[] expected = "A6AC8C048A7DFD7B838DA88F22C3FAB5BFF15D7CB8D83A62C6721A8FAF6903EAB6152CB7421026E36F2FFEF661EB4384DC276495C71B5CAB72E1C1A38712E56B".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlR", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2P")), 1);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest3BFromStackOverflow()
        {
            byte[] expected = "94FFC2B1A390B7B8A9E6A44922C330DB2B193ADCF082EECD06057197F35931A9D0EC0EE5C660744B50B61F23119B847E658D179A914807F4B8AB8EB9505AF065".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlR", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2P")), 100000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest4AFromStackOverflow()
        {
            byte[] expected = "E2CCC7827F1DD7C33041A98906A8FD7BAE1920A55FCB8F831683F14F1C3979351CB868717E5AB342D9A11ACF0B12D3283931D609B06602DA33F8377D1F1F9902".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE5", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJe")), 1);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest4BFromStackOverflow()
        {
            byte[] expected = "07447401C85766E4AED583DE2E6BF5A675EABE4F3618281C95616F4FC1FDFE6ECBC1C3982789D4FD941D6584EF534A78BD37AE02555D9455E8F089FDB4DFB6BB".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE5", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJe")), 100000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// We agree with the posted vectors and thus Python and SQL, but not OpenSSL which appears to be in the wrong.
        /// </summary>
        [Test]
        public void TestCaseLongTest5AFromStackOverflow()
        {
            byte[] expected = "B029A551117FF36977F283F579DC7065B352266EA243BDD3F920F24D4D141ED8B6E02D96E2D3BDFB76F8D77BA8F4BB548996AD85BB6F11D01A015CE518F9A717".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJem")), 1);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest5BFromStackOverflow()
        {
            byte[] expected = "31F5CC83ED0E948C05A15735D818703AAA7BFF3F09F5169CAF5DBA6602A05A4D5CFF5553D42E82E40516D6DC157B8DAEAE61D3FEA456D964CB2F7F9A63BBBDB5".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJem")), 100000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// We agree with the posted vectors and thus Python and SQL, but not OpenSSL which appears to be in the wrong.
        /// </summary>
        [Test]
        public void TestCaseLongTest6AFromStackOverflow()
        {
            byte[] expected = "28B8A9F644D6800612197BB74DF460272E2276DE8CC07AC4897AC24DBC6EB77499FCAF97415244D9A29DA83FC347D09A5DBCFD6BD63FF6E410803DCA8A900AB6".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57U", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemk")), 1);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest6BFromStackOverflow()
        {
            byte[] expected = "056BC9072A356B7D4DA60DD66F5968C2CAA375C0220EDA6B47EF8E8D105ED68B44185FE9003FBBA49E2C84240C9E8FD3F5B2F4F6512FD936450253DB37D10028".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57U", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemk")), 100000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// We agree with the posted vectors and thus Python and SQL, but not OpenSSL which appears to be in the wrong.
        /// </summary>
        [Test]
        public void TestCaseLongTest7AFromStackOverflow()
        {
            byte[] expected = "16226C85E4F8D604573008BFE61C10B6947B53990450612DD4A3077F7DEE2116229E68EFD1DF6D73BD3C6D07567790EEA1E8B2AE9A1B046BE593847D9441A1B7".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi0", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy")), 1);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest7BFromStackOverflow()
        {
            byte[] expected = "70CF39F14C4CAF3C81FA288FB46C1DB52D19F72722F7BC84F040676D3371C89C11C50F69BCFBC3ACB0AB9E92E4EF622727A916219554B2FA121BEDDA97FF3332".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi0", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy")), 100000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// We agree with the posted vectors and thus Python and SQL, but not OpenSSL which appears to be in the wrong.
        /// </summary>
        [Test]
        public void TestCaseLongTest8AFromStackOverflow()
        {
            byte[] expected = "880C58C316D3A5B9F05977AB9C60C10ABEEBFAD5CE89CAE62905C1C4F80A0A098D82F95321A6220F8AECCFB45CE6107140899E8D655306AE6396553E2851376C".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6")), 1);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest8BFromStackOverflow()
        {
            byte[] expected = "2668B71B3CA56136B5E87F30E098F6B4371CB5ED95537C7A073DAC30A2D5BE52756ADF5BB2F4320CB11C4E16B24965A9C790DEF0CBC62906920B4F2EB84D1D4A".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6")), 100000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// We agree with the posted vectors and thus Python and SQL, but not OpenSSL which appears to be in the wrong.
        /// </summary>
        [Test]
        public void TestCaseLongTest9AFromStackOverflow()
        {
            byte[] expected = "93B9BA8283CC17D50EF3B44820828A258A996DE258225D24FB59990A6D0DE82DFB3FE2AC201952100E4CC8F06D883A9131419C0F6F5A6ECB8EC821545F14ADF1".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04U", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6P")), 1);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest9BFromStackOverflow()
        {
            byte[] expected = "2575B485AFDF37C260B8F3386D33A60ED929993C9D48AC516EC66B87E06BE54ADE7E7C8CB3417C81603B080A8EEFC56072811129737CED96236B9364E22CE3A5".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04U", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6P")), 100000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest10AFromStackOverflow()
        {
            byte[] expected = "384BCD6914407E40C295D1037CF4F990E8F0E720AF43CB706683177016D36D1A14B3A7CF22B5DF8D5D7D44D69610B64251ADE2E7AB54A3813A89935592E391BF".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04Uz3ebEAhzZ4ve1A2wg5CnLXdZC5Y7gwfVgbEgZSTmoYQSzC5OW4dfrjqiwApTACO6xoOL1AjWj6X6f6qFfF8TVmOzU9RhOd1N4QtzWI4fP6FYttNz5FuLdtYVXWVXH2Tf7I9fieMeWCHTMkM4VcmQyQHpbcP8MEb5f1g6Ckg5xk3HQr3wMBvQcOHpCPy1K8HCM7a5wkPDhgVA0BVmwNpsRIbDQZRtHK6dT6bGyalp6gbFZBuBHwD86gTzkrFY7HkOVrgc0gJcGJZe65Ce8v4Jn5OzkuVsiU8efm2Pw2RnbpWSAr7SkVdCwXK2XSJDQ5fZ4HBEz9VTFYrG23ELuLjvx5njOLNgDAJuf5JB2tn4nMjjcnl1e8qcYVwZqFzEv2zhLyDWMkV4tzl4asLnvyAxTBkxPRZj2pRABWwb3kEofpsHYxMTAn38YSpZreoXipZWBnu6HDURaruXaIPYFPYHl9Ls9wsuD7rzaGfbOyfVgLIGK5rODphwRA7lm88bGKY8b7tWOtepyEvaLxMI7GZF5ScwpZTYeEDNUKPzvM2Im9zehIaznpguNdNXNMLWnwPu4H6zEvajkw3G3ucSiXKmh6XNe3hkdSANm3vnxzRXm4fcuzAx68IElXE2bkGFElluDLo6EsUDWZ4JIWBVaDwYdJx8uCXbQdoifzCs5kuuClaDaDqIhb5hJ2WR8mxiueFsS0aDGdIYmye5svmNmzQxFmdOkHoF7CfwuU1yy4uEEt9vPSP2wFp1dyaMvJW68vtB4kddLmI6gIgVVcT6ZX1Qm6WsusPrdisPLB2ScodXojCbL3DLj6PKG8QDVMWTrL1TpafT2wslRledWIhsTlv2mI3C066WMcTSwKLXdEDhVvFJ6ShiLKSN7gnRrlE0BnAw", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6PlBdILBOkKUB6TGTPJXh1tpdOHTG6KuIvcbQp9qWjaf1uxAKgiTtYRIHhxjJI2viVa6fDZ67QOouOaf2RXQhpsWaTtAVnff6PIFcvJhdPDFGV5nvmZWoCZQodj6yXRDHPw9PyF0iLYm9uFtEunlAAxGB5qqea4X5tZvB1OfLVwymY3a3JPjdxTdvHxCHbqqE0zip61JNqdmeWxGtlRBC6CGoCiHO4XxHCntQBRJDcG0zW7joTdgtTBarsQQhlLXBGMNBSNmmTbDf3hFtawUBCJH18IAiRMwyeQJbJ2bERsY3MVRPuYCf4Au7gN72iGh1lRktSQtEFye7pO46kMXRrEjHQWXInMzzy7X2StXUzHVTFF2VdOoKn0WUqFNvB6PF7qIsOlYKj57bi1Psa34s85WxMSbTkhrd7VHdHZkTVaWdraohXYOePdeEvIwObCGEXkETUzqM5P2yzoBOJSdjpIYaa8zzdLD3yrb1TwCZuJVxsrq0XXY6vErU4QntsW0972XmGNyumFNJiPm4ONKh1RLvS1kddY3nm8276S4TUuZfrRQO8QxZRNuSaZI8JRZp5VojB5DktuMxAQkqoPjQ5Vtb6oXeOyY591CB1MEW1fLTCs0NrL321SaNRMqza1ETogAxpEiYwZ6pIgnMmSqNMRdZnCqA4gMWw1lIVATWK83OCeicNRUNOdfzS7A8vbLcmvKPtpOFvhNzwrrUdkvuKvaYJviQgeR7snGetO9JLCwIlHIj52gMCNU18d32SJl7Xomtl3wIe02SMvq1i1BcaX7lXioqWGmgVqBWU3fsUuGwHi6RUKCCQdEOBfNo2WdpFaCflcgnn0O6jVHCqkv8cQk81AqS00rAmHGCNTwyA6Tq5TXoLlDnC8gAQjDUsZp0z")), 1);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors .
        /// </summary>
        [Test]
        public void TestCaseLongTest10BFromStackOverflow()
        {
            byte[] expected = "B8674F6C0CC9F8CF1F1874534FD5AF01FC1504D76C2BC2AA0A75FE4DD5DFD1DAF60EA7C85F122BCEEB8772659D601231607726998EAC3F6AAB72EFF7BA349F7F".FromHex();
            byte[] actual = DeriveBytes("passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04Uz3ebEAhzZ4ve1A2wg5CnLXdZC5Y7gwfVgbEgZSTmoYQSzC5OW4dfrjqiwApTACO6xoOL1AjWj6X6f6qFfF8TVmOzU9RhOd1N4QtzWI4fP6FYttNz5FuLdtYVXWVXH2Tf7I9fieMeWCHTMkM4VcmQyQHpbcP8MEb5f1g6Ckg5xk3HQr3wMBvQcOHpCPy1K8HCM7a5wkPDhgVA0BVmwNpsRIbDQZRtHK6dT6bGyalp6gbFZBuBHwD86gTzkrFY7HkOVrgc0gJcGJZe65Ce8v4Jn5OzkuVsiU8efm2Pw2RnbpWSAr7SkVdCwXK2XSJDQ5fZ4HBEz9VTFYrG23ELuLjvx5njOLNgDAJuf5JB2tn4nMjjcnl1e8qcYVwZqFzEv2zhLyDWMkV4tzl4asLnvyAxTBkxPRZj2pRABWwb3kEofpsHYxMTAn38YSpZreoXipZWBnu6HDURaruXaIPYFPYHl9Ls9wsuD7rzaGfbOyfVgLIGK5rODphwRA7lm88bGKY8b7tWOtepyEvaLxMI7GZF5ScwpZTYeEDNUKPzvM2Im9zehIaznpguNdNXNMLWnwPu4H6zEvajkw3G3ucSiXKmh6XNe3hkdSANm3vnxzRXm4fcuzAx68IElXE2bkGFElluDLo6EsUDWZ4JIWBVaDwYdJx8uCXbQdoifzCs5kuuClaDaDqIhb5hJ2WR8mxiueFsS0aDGdIYmye5svmNmzQxFmdOkHoF7CfwuU1yy4uEEt9vPSP2wFp1dyaMvJW68vtB4kddLmI6gIgVVcT6ZX1Qm6WsusPrdisPLB2ScodXojCbL3DLj6PKG8QDVMWTrL1TpafT2wslRledWIhsTlv2mI3C066WMcTSwKLXdEDhVvFJ6ShiLKSN7gnRrlE0BnAw", new Salt(Encoding.ASCII.GetBytes("saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6PlBdILBOkKUB6TGTPJXh1tpdOHTG6KuIvcbQp9qWjaf1uxAKgiTtYRIHhxjJI2viVa6fDZ67QOouOaf2RXQhpsWaTtAVnff6PIFcvJhdPDFGV5nvmZWoCZQodj6yXRDHPw9PyF0iLYm9uFtEunlAAxGB5qqea4X5tZvB1OfLVwymY3a3JPjdxTdvHxCHbqqE0zip61JNqdmeWxGtlRBC6CGoCiHO4XxHCntQBRJDcG0zW7joTdgtTBarsQQhlLXBGMNBSNmmTbDf3hFtawUBCJH18IAiRMwyeQJbJ2bERsY3MVRPuYCf4Au7gN72iGh1lRktSQtEFye7pO46kMXRrEjHQWXInMzzy7X2StXUzHVTFF2VdOoKn0WUqFNvB6PF7qIsOlYKj57bi1Psa34s85WxMSbTkhrd7VHdHZkTVaWdraohXYOePdeEvIwObCGEXkETUzqM5P2yzoBOJSdjpIYaa8zzdLD3yrb1TwCZuJVxsrq0XXY6vErU4QntsW0972XmGNyumFNJiPm4ONKh1RLvS1kddY3nm8276S4TUuZfrRQO8QxZRNuSaZI8JRZp5VojB5DktuMxAQkqoPjQ5Vtb6oXeOyY591CB1MEW1fLTCs0NrL321SaNRMqza1ETogAxpEiYwZ6pIgnMmSqNMRdZnCqA4gMWw1lIVATWK83OCeicNRUNOdfzS7A8vbLcmvKPtpOFvhNzwrrUdkvuKvaYJviQgeR7snGetO9JLCwIlHIj52gMCNU18d32SJl7Xomtl3wIe02SMvq1i1BcaX7lXioqWGmgVqBWU3fsUuGwHi6RUKCCQdEOBfNo2WdpFaCflcgnn0O6jVHCqkv8cQk81AqS00rAmHGCNTwyA6Tq5TXoLlDnC8gAQjDUsZp0z")), 100000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        [Test]
        public void TestConstructorWithBadArguments()
        {
            Pbkdf2HmacSha512 pbkdf = null;
            Assert.Throws<ArgumentNullException>(() => pbkdf = new Pbkdf2HmacSha512("passphrase", null, 0));
            Assert.Throws<ArgumentNullException>(() => pbkdf = new Pbkdf2HmacSha512(null, Salt.Zero, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => pbkdf = new Pbkdf2HmacSha512("passphrase", Salt.Zero, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => pbkdf = new Pbkdf2HmacSha512("passphrase", Salt.Zero, -1));

            Assert.DoesNotThrow(() => pbkdf = new Pbkdf2HmacSha512("passphrase", Salt.Zero, 10));
            Assert.That(pbkdf, Is.Not.Null);
        }

        [Test]
        public void TestGetBytesTwice()
        {
            Pbkdf2HmacSha512 pbkdf = new Pbkdf2HmacSha512("passphrase", Salt.Zero, 10);

            byte[] bytes = pbkdf.GetBytes();
            Assert.Throws<InternalErrorException>(() => bytes = pbkdf.GetBytes());
            Assert.That(bytes, Is.Not.Null);
        }

        private static Salt PatternSalt(int length)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; ++i)
            {
                bytes[i] = (byte)((i * 7) + 1);
            }

            return new Salt(bytes);
        }

        /// <summary>
        /// A zero length salt, as Salt.Zero provides, must be accepted.
        /// </summary>
        [Test]
        public void TestEmptySalt()
        {
            byte[] expected = "037e94baa9506c5ba26bd3bbaa3684b933192040620ebce309ff1e8442d5463ce406cee23465ca5f93c3210754b7ad6005cafd3fcf1a75fdd6f654586767d1f5".FromHex();
            byte[] actual = DeriveBytes("password", Salt.Zero, 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        [Test]
        public void TestOneByteSalt()
        {
            byte[] expected = "c993dcc02222234bda32c9e1c57d663833072a43c19d583202e3e19b533530e8bde7188ec4cb9413e05f632e8c3a90ee82f61effc3454076695e4e1258be3cc3".FromHex();
            byte[] actual = DeriveBytes("password", PatternSalt(1), 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// The SHA-512 block size is 128 bytes, so check either side of that boundary.
        /// </summary>
        [Test]
        public void TestSaltJustBelowBlockSize()
        {
            byte[] expected = "83e0dcaedb8d35b69f6195df4d411083cfa08b5d39b1ad0523790f654df533b1fc99c40c4e6d83539364f693e91bbd1b8696c21c98474be509839d6e3c11e3ed".FromHex();
            byte[] actual = DeriveBytes("password", PatternSalt(127), 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        [Test]
        public void TestSaltExactlyBlockSize()
        {
            byte[] expected = "3a3e59006a1400306f8237c2134ce21512f6de18ffba6f124475bf19c3bc43f7d601f3a5dbd25f5316697dce7ffecd493289a1f24fab13986129e98609d4296e".FromHex();
            byte[] actual = DeriveBytes("password", PatternSalt(128), 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        [Test]
        public void TestSaltJustAboveBlockSize()
        {
            byte[] expected = "b8c73a2c93d751e77f88ce34d8c349f5203bd9f56e05bb56c74b3055f6d3a6ad106b29071281852ad7025bd13fb0beb0b4be36ecd8ddff0e045e0731775ef8e9".FromHex();
            byte[] actual = DeriveBytes("password", PatternSalt(129), 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// 32 bytes is the salt size actually used, via new Salt(256).
        /// </summary>
        [Test]
        public void TestSaltOfTheSizeActuallyUsed()
        {
            byte[] expected = "4cec1a7570fb28cf0fe399308f7f8fec92c647dc7b1f75deec01cbefb5e3eab26f21973598200784ed8dc0793871013364e1ef5440c85d75af7c2d8d10afc73a".FromHex();
            byte[] actual = DeriveBytes("password", PatternSalt(32), 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// A password longer than the HMAC block size is hashed down to a key first.
        /// </summary>
        [Test]
        public void TestPasswordLongerThanBlockSize()
        {
            byte[] expected = "1769eeb10efc00454f329c6f31ca76e28737930b496e98d5bfc1e569cd2b5f640e3b3353a23e34ebb6e6bc2c4a69a616031d25ff65a94054a5eef41d1abcc46f".FromHex();
            byte[] actual = DeriveBytes(new string('P', 129), PatternSalt(32), 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        [Test]
        public void TestEmptyPassword()
        {
            byte[] expected = "83325b0922b23d3b98235958706302cd9ad1cf39813fc5d535b32a6293a64fd5f84b97e870891093d32255b8c35075c6b8dfbc76cb432df288c54931453c1a5e".FromHex();
            byte[] actual = DeriveBytes(string.Empty, PatternSalt(32), 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// The password is encoded as UTF-8 without a byte order mark, so check multi byte sequences.
        /// </summary>
        [Test]
        public void TestLatinAccentedPassword()
        {
            byte[] expected = "f86962a6725288959ba5169cca3039db86bbbf9193c5e0bdc93025fafdd8adf84ff4e04cec7bd771dd6eb31082dcb6e191ffa0756f1563816bf2710b6ba64cb7".FromHex();
            byte[] actual = DeriveBytes("räksmörgås", PatternSalt(32), 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        [Test]
        public void TestCjkPassword()
        {
            byte[] expected = "f4b99a7dfc65515838efe2b5751cbdd813fb4844d448d2a0c31609c3a1d8ccfbc1d9c34af87b98a8656a14245c7d856b741ff2e57fb54f5e120c1ec9b71b2773".FromHex();
            byte[] actual = DeriveBytes("密码密碼", PatternSalt(32), 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// Characters outside the BMP are surrogate pairs in UTF-16, four bytes in UTF-8.
        /// </summary>
        [Test]
        public void TestSurrogatePairPassword()
        {
            byte[] expected = "21fa818d911a7ce7c9dad8318e370639a725d8afb18f6ce6064bf1dd416c52285269e998f176c3ba4d75f916f1ba5813aa49db0c93fb3e4c1f086934bdf51779".FromHex();
            byte[] actual = DeriveBytes("🔐🗝", PatternSalt(32), 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// A combining mark is a separate code point, and must not be normalized away.
        /// </summary>
        [Test]
        public void TestCombiningMarkPassword()
        {
            byte[] expected = "f402b53d3755d526c584e4f8b4ac7ef1e438416d6a77cb1e71593fb00551ce25203bb966e6a9d0e983331a65ffe21b1e72ce67bfb4d3ba33c09aa5a6cb58a9b1".FromHex();
            byte[] actual = DeriveBytes("égal", PatternSalt(32), 1000);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        /// <summary>
        /// One and two iterations are covered by the vectors above; three is the first fully general case.
        /// </summary>
        [Test]
        public void TestThreeIterations()
        {
            byte[] expected = "b6b07cb2cebf4ad84468391a543824fccffe0e0769dbe6bddf10a65673c4b648e612d44918f9ce9a19a1294cf5140628084ba994c3b21a4ef4741220b811c633".FromHex();
            byte[] actual = DeriveBytes("password", new Salt(Encoding.ASCII.GetBytes("salt")), 3);

            Assert.That(actual.IsEquivalentTo(expected));
        }

        [Test]
        public void TestXecretsPbkdf2ConstructorWithBadArguments()
        {
            XecretsPbkdf2 pbkdf = null;
            Assert.Throws<ArgumentNullException>(() => pbkdf = new XecretsPbkdf2(null, Salt.Zero.GetBytes(), 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => pbkdf = new XecretsPbkdf2("passphrase", Salt.Zero.GetBytes(), 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => pbkdf = new XecretsPbkdf2("passphrase", Salt.Zero.GetBytes(), -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => pbkdf = new XecretsPbkdf2("passphrase", Salt.Zero.GetBytes(), 10, 0));

            Assert.DoesNotThrow(() => pbkdf = new XecretsPbkdf2("passphrase", Salt.Zero.GetBytes(), 10));
            Assert.That(pbkdf, Is.Not.Null);
        }

        [Test]
        public void TestXecretsPbkdf2GetBytesTwice()
        {
            XecretsPbkdf2 pbkdf = new XecretsPbkdf2("passphrase", Salt.Zero.GetBytes(), 10);

            byte[] bytes = pbkdf.GetBytes();
            Assert.Throws<InvalidOperationException>(() => bytes = pbkdf.GetBytes());
            Assert.That(bytes, Is.Not.Null);
        }

        [Test]
        public void TestXecretsPbkdf2NonDefaultOutputLength()
        {
            byte[] expected = "4cec1a7570fb28cf0fe399308f7f8fec92c647dc7b1f75deec01cbefb5e3eab26f21973598200784ed8dc0793871013364e1ef5440c85d75af7c2d8d10afc73a".FromHex();
            byte[] actual = new XecretsPbkdf2("password", PatternSalt(32).GetBytes(), 1000, 16).GetBytes();

            Assert.That(actual.Length, Is.EqualTo(16));
            Assert.That(actual.IsEquivalentTo(expected.Take(16).ToArray()));
        }
    }
}
