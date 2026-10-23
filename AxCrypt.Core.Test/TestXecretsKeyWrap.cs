#region Coypright and GPL License

/*
 * Xecrets.Net - Copyright © 2022-2026, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets.Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets.Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets.Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Coypright and GPL License

using AxCrypt.Core.Extensions;

using NUnit.Framework;

using Xecrets.Net.Cryptography;

namespace Xecrets.Net.Core.Test;

[TestFixture]
public class TestXecretsKeyWrap
{
    // RFC 3394 section 4.
    private static TestCaseData[] SpecificationVectors =>
    [
        new TestCaseData(
                "000102030405060708090A0B0C0D0E0F".FromHex(),
                "00112233445566778899AABBCCDDEEFF".FromHex(),
                "1FA68B0A8112B447AEF34BD8FB5A7B829D3E862371D2CFE5".FromHex())
            .SetName("RFC 3394 4.1 128-bit KEK, 128-bit key data"),
        new TestCaseData(
                "000102030405060708090A0B0C0D0E0F1011121314151617".FromHex(),
                "00112233445566778899AABBCCDDEEFF".FromHex(),
                "96778B25AE6CA435F92B5B97C050AED2468AB8A17AD84E5D".FromHex())
            .SetName("RFC 3394 4.2 192-bit KEK, 128-bit key data"),
        new TestCaseData(
                "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F".FromHex(),
                "00112233445566778899AABBCCDDEEFF".FromHex(),
                "64E8C3F9CE0F5BA263E9777905818A2A93C8191E7D6E8AE7".FromHex())
            .SetName("RFC 3394 4.3 256-bit KEK, 128-bit key data"),
        new TestCaseData(
                "000102030405060708090A0B0C0D0E0F1011121314151617".FromHex(),
                "00112233445566778899AABBCCDDEEFF0001020304050607".FromHex(),
                "031D33264E15D33268F24EC260743EDCE1C6C7DDEE725A936BA814915C6762D2".FromHex())
            .SetName("RFC 3394 4.4 192-bit KEK, 192-bit key data"),
        new TestCaseData(
                "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F".FromHex(),
                "00112233445566778899AABBCCDDEEFF0001020304050607".FromHex(),
                "A8F9BC1612C68B3FF6E6F4FBE30E71E4769C8B80A32CB8958CD5D17D6B254DA1".FromHex())
            .SetName("RFC 3394 4.5 256-bit KEK, 192-bit key data"),
        new TestCaseData(
                "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F".FromHex(),
                "00112233445566778899AABBCCDDEEFF000102030405060708090A0B0C0D0E0F".FromHex(),
                "28C9F404C4B810F4CBCCB35CFB87F8263F5786E2D80ED326CBC7F0E71A99F43BFB988B9B7A02DD21".FromHex())
            .SetName("RFC 3394 4.6 256-bit KEK, 256-bit key data"),
    ];

    [TestCaseSource(nameof(SpecificationVectors))]
    public void TestSpecificationVectors(byte[] kek, byte[] keyData, byte[] wrappedData)
    {
        XecretsKeyWrap keyWrap = new(6, littleEndianCounter: false);

        Assert.That(keyWrap.Wrap(kek, keyData), Is.EqualTo(wrappedData), "The wrapped data should match the RFC 3394 vector.");
        Assert.That(keyWrap.Unwrap(kek, wrappedData), Is.EqualTo(keyData), "The unwrapped data should match the RFC 3394 vector.");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void TestWrapAndUnwrapWithManyIterations(bool littleEndianCounter)
    {
        byte[] kek = "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F".FromHex();
        byte[] keyData = "00112233445566778899AABBCCDDEEFF000102030405060708090A0B0C0D0E0F".FromHex();
        XecretsKeyWrap keyWrap = new(12345, littleEndianCounter);

        byte[] wrapped = keyWrap.Wrap(kek, keyData);

        Assert.That(wrapped, Has.Length.EqualTo(keyData.Length + 8));
        Assert.That(keyWrap.Unwrap(kek, wrapped), Is.EqualTo(keyData));
    }

    [Test]
    public void TestCounterByteOrderMatters()
    {
        byte[] kek = "000102030405060708090A0B0C0D0E0F".FromHex();
        byte[] keyData = "00112233445566778899AABBCCDDEEFF".FromHex();

        byte[] bigEndian = new XecretsKeyWrap(6, littleEndianCounter: false).Wrap(kek, keyData);
        byte[] littleEndian = new XecretsKeyWrap(6, littleEndianCounter: true).Wrap(kek, keyData);

        Assert.That(littleEndian, Is.Not.EqualTo(bigEndian));
        Assert.That(new XecretsKeyWrap(6, littleEndianCounter: false).Unwrap(kek, littleEndian), Is.Empty);
    }

    [Test]
    public void TestUnwrapWithWrongKeyReturnsEmpty()
    {
        byte[] wrongKek = "0F0E0D0C0B0A09080706050403020100".FromHex();
        byte[] wrapped = "1FA68B0A8112B447AEF34BD8FB5A7B829D3E862371D2CFE5".FromHex();

        Assert.That(new XecretsKeyWrap(6, littleEndianCounter: false).Unwrap(wrongKek, wrapped), Is.Empty);
    }

    [Test]
    public void TestConstructorWithBadArguments()
    {
        XecretsKeyWrap keyWrap = null;
        Assert.Throws<ArgumentOutOfRangeException>(() => keyWrap = new XecretsKeyWrap(5, false));
        Assert.Throws<ArgumentOutOfRangeException>(() => keyWrap = new XecretsKeyWrap(0, false));
        Assert.Throws<ArgumentOutOfRangeException>(() => keyWrap = new XecretsKeyWrap(-100, true));

        Assert.DoesNotThrow(() => keyWrap = new XecretsKeyWrap(6, false));
    }

    [Test]
    public void TestWrapWithBadArguments()
    {
        XecretsKeyWrap keyWrap = new(6, false);
        byte[] kek = new byte[16];

        Assert.Throws<ArgumentException>(() => keyWrap.Wrap(new byte[15], new byte[16]), "Bad key encrypting key length.");
        Assert.Throws<ArgumentException>(() => keyWrap.Wrap(kek, []), "Empty key material.");
        Assert.Throws<ArgumentException>(() => keyWrap.Wrap(kek, new byte[8]), "Too short key material.");
        Assert.Throws<ArgumentException>(() => keyWrap.Wrap(kek, new byte[17]), "Key material not a multiple of 64 bits.");
    }

    [Test]
    public void TestUnwrapWithBadArguments()
    {
        XecretsKeyWrap keyWrap = new(6, false);
        byte[] kek = new byte[16];

        Assert.Throws<ArgumentException>(() => keyWrap.Unwrap(new byte[33], new byte[24]), "Bad key encrypting key length.");
        Assert.Throws<ArgumentException>(() => keyWrap.Unwrap(kek, new byte[16]), "Too short wrapped data.");
        Assert.Throws<ArgumentException>(() => keyWrap.Unwrap(kek, new byte[25]), "Wrapped data not a multiple of 64 bits.");
    }
}
