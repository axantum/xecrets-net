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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Content;
using System;
using System.Linq;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Extensions
{
    public static class PrimitiveTypeExtensions
    {
        private static bool _isLittleEndian = OS.Current.IsLittleEndian;

        public static void SetLittleEndian(this bool isLittleEndian)
        {
            _isLittleEndian = isLittleEndian;
        }

        public static byte[] GetLittleEndianBytes(this long value)
        {
            if (_isLittleEndian)
            {
                return BitConverter.GetBytes(value);
            }

            byte[] bytes = new byte[sizeof(long)];

            for (int i = 0; value != 0 && i < bytes.Length; ++i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }

        public static byte[] GetLittleEndianBytes(this int value)
        {
            if (_isLittleEndian)
            {
                return BitConverter.GetBytes(value);
            }

            byte[] bytes = new byte[sizeof(int)];

            for (int i = 0; value != 0 && i < bytes.Length; ++i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }

        public static byte[] GetBigEndianBytes(this long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!_isLittleEndian)
            {
                return bytes;
            }

            byte b;

            b = bytes[0];
            bytes[0] = bytes[7];
            bytes[7] = b;

            b = bytes[1];
            bytes[1] = bytes[6];
            bytes[6] = b;

            b = bytes[2];
            bytes[2] = bytes[5];
            bytes[5] = b;

            b = bytes[3];
            bytes[3] = bytes[4];
            bytes[4] = b;

            return bytes;
        }

        public static byte[] GetBigEndianBytes(this int value)
        {
            if (!_isLittleEndian)
            {
                return BitConverter.GetBytes(value);
            }

            byte[] bytes = new byte[sizeof(int)];

            for (int i = bytes.Length - 1; value != 0 && i >= 0; --i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }

        public static T Fallback<T>(this T value, T fallbackValue) where T : IEquatable<T>
        {
            return !value.Equals(default(T)) ? value : fallbackValue;
        }

        public static bool IsBetween(this DateTime time, TimeSpan from, TimeSpan to)
        {
            return time.IsOlderThan(from) && time.IsMoreRecentThan(to);
        }

        public static bool IsOlderThan(this DateTime time, TimeSpan interval)
        {
            return time.IsOlderThan(New<INow>().Utc - interval);
        }

        public static bool IsOlderThan(this DateTime time, DateTime thanTime)
        {
            return time < thanTime;
        }

        public static bool IsMoreRecentThan(this DateTime time, DateTime thanTime)
        {
            return time > thanTime;
        }

        public static bool IsMoreRecentThan(this DateTime time, TimeSpan interval)
        {
            return time.IsMoreRecentThan(New<INow>().Utc - interval);
        }

        public static string ToValidationMessage(this int validationError)
        {
            switch ((ValidationError)validationError)
            {
                case ValidationError.None:
                    return string.Empty;

                case ValidationError.VerificationPassphraseWrong:
                    return Texts.PassphraseVerificationMismatch;

                case ValidationError.WrongPassphrase:
                    return Texts.WrongPassphrase;

                case ValidationError.InvalidEmail:
                    return Texts.InvalidEmail;

                case ValidationError.SamePasswordAlreadySignedIn:
                    return Texts.SameFilePasswordAsSignedInError;

                case ValidationError.IdentityExistsAlready:
                case ValidationError.OnlineRequired:
                case ValidationError.PremiumRequired:
                case ValidationError.KeyFileInaccessible:
                default:
                    return "Unexpected Validation Error";
            }
        }

        private static bool IsLegacy(Guid cryptoId)
        {
            return cryptoId == new V1Aes128CryptoFactory().CryptoId;
        }

        private static bool IsStandardAndHasStrongerCapability(Guid cryptoId)
        {
            if (cryptoId != new V2Aes128CryptoFactory().CryptoId)
            {
                return false;
            }
            if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.StrongerEncryption))
            {
                return false;
            }

            return true;
        }

        public static bool ShouldUpgradeEncryption(this Guid cryptoId)
        {
            if (!IsLegacy(cryptoId) && !IsStandardAndHasStrongerCapability(cryptoId))
            {
                return false;
            }

            if (New<UserSettings>().EncryptionUpgradeMode != EncryptionUpgradeMode.AutoUpgrade)
            {
                return false;
            }

            if (!New<KnownIdentities>().IsLoggedOn)
            {
                return false;
            }

            return true;
        }
    }
}