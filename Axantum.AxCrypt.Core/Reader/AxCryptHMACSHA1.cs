#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://AxCrypt.codeplex.com/ please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Reader
{
    /// <summary>
    /// Calculate HMACSHA1 the AxCrypt way.
    /// </summary>
    /// <remarks>
    /// The .NET standard implementation uses a block size of 64, which is really only relevant
    /// as to how to treat the key. AxCrypt uses a block size of 20. This class is required because
    /// the .NET implementation has the BlockSizeValue as protected.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Both HMAC and SHA1 are meaningful acronyms and this is the way .NET does the naming.")]
    public sealed class AxCryptHMACSHA1 : HMACSHA1
    {
        private AxCryptHMACSHA1()
        {
            // We can't do it all in the constructor because then we need to call virtual methods and that's a bad
            // idea in a constructor.
        }

        public static HMAC Create(byte[] key)
        {
            AxCryptHMACSHA1 hmac = new AxCryptHMACSHA1();
            hmac.BlockSizeValue = 20;
            hmac.Key = key;

            return hmac;
        }
    }
}