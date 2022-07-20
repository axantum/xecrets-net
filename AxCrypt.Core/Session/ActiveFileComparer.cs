using System.Linq;

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

using System;
using System.Collections.Generic;

namespace AxCrypt.Core.Session
{
    public abstract class ActiveFileComparer : IComparer<ActiveFile>
    {
        private class EncryptedNameComparerImpl : ActiveFileComparer
        {
            public override int Compare(ActiveFile x, ActiveFile y)
            {
                if (x == null)
                {
                    throw new ArgumentNullException("x");
                }
                if (y == null)
                {
                    throw new ArgumentNullException("y");
                }

                return (ReverseSort ? -1 : 1) * String.Compare(x.EncryptedFileInfo.FullName, y.EncryptedFileInfo.FullName, StringComparison.OrdinalIgnoreCase);
            }
        }

        private class DecryptedNameComparerImpl : ActiveFileComparer
        {
            public override int Compare(ActiveFile x, ActiveFile y)
            {
                if (x == null)
                {
                    throw new ArgumentNullException("x");
                }
                if (y == null)
                {
                    throw new ArgumentNullException("y");
                }

                return (ReverseSort ? -1 : 1) * String.Compare(Resolve.Portable.Path().GetFileName(x.DecryptedFileInfo.FullName), Resolve.Portable.Path().GetFileName(y.DecryptedFileInfo.FullName), StringComparison.OrdinalIgnoreCase);
            }
        }

        private class SizeComparerImpl : ActiveFileComparer
        {
            public override int Compare(ActiveFile x, ActiveFile y)
            {
                if (x == null)
                {
                    throw new ArgumentNullException("x");
                }
                if (y == null)
                {
                    throw new ArgumentNullException("y");
                }

                return (ReverseSort ? -1 : 1) * String.Compare(x.Size(), y.Size(), StringComparison.OrdinalIgnoreCase);
            }
        }

        private class DateComparerImpl : ActiveFileComparer
        {
            public override int Compare(ActiveFile x, ActiveFile y)
            {
                if (x == null)
                {
                    throw new ArgumentNullException("x");
                }
                if (y == null)
                {
                    throw new ArgumentNullException("y");
                }

                return (ReverseSort ? -1 : 1) * x.Properties.LastActivityTimeUtc.CompareTo(y.Properties.LastActivityTimeUtc);
            }
        }

        private class CryptoNameComparerImpl : ActiveFileComparer
        {
            public override int Compare(ActiveFile x, ActiveFile y)
            {
                if (x == null)
                {
                    throw new ArgumentNullException("x");
                }
                if (y == null)
                {
                    throw new ArgumentNullException("y");
                }

                return (ReverseSort ? -1 : 1) * String.Compare(Resolve.CryptoFactory.Create(x.Properties.CryptoId).Name, Resolve.CryptoFactory.Create(y.Properties.CryptoId).Name, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static ActiveFileComparer EncryptedNameComparer
        { get { return new EncryptedNameComparerImpl(); } }

        public static ActiveFileComparer SizeComparer
        { get { return new SizeComparerImpl(); } }

        public static ActiveFileComparer DecryptedNameComparer
        { get { return new DecryptedNameComparerImpl(); } }

        public static ActiveFileComparer DateComparer
        { get { return new DateComparerImpl(); } }

        public static ActiveFileComparer CryptoNameComparer
        { get { return new CryptoNameComparerImpl(); } }

        public abstract int Compare(ActiveFile x, ActiveFile y);

        public bool ReverseSort { get; set; }
    }
}