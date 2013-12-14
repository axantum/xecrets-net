using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Session
{
    public abstract class ActiveFileComparer : IComparer<ActiveFile>
    {
        private class EncryptedNameComparerImpl : ActiveFileComparer
        {
            public override int Compare(ActiveFile x, ActiveFile y)
            {
                return (ReverseSort ? -1 : 1) * String.Compare(x.EncryptedFileInfo.FullName, y.EncryptedFileInfo.FullName, StringComparison.OrdinalIgnoreCase);
            }
        }

        private class DecryptedNameComparerImpl : ActiveFileComparer
        {
            public override int Compare(ActiveFile x, ActiveFile y)
            {
                return (ReverseSort ? -1 : 1) * String.Compare(Path.GetFileName(x.DecryptedFileInfo.FullName), Path.GetFileName(y.DecryptedFileInfo.FullName), StringComparison.OrdinalIgnoreCase);
            }
        }

        private class DateComparerImpl : ActiveFileComparer
        {
            public override int Compare(ActiveFile x, ActiveFile y)
            {
                return (ReverseSort ? -1 : 1) * x.LastActivityTimeUtc.CompareTo(y.LastActivityTimeUtc);
            }
        }

        public static ActiveFileComparer EncryptedNameComparer { get { return new EncryptedNameComparerImpl(); } }

        public static ActiveFileComparer DecryptedNameComparer { get { return new DecryptedNameComparerImpl(); } }

        public static ActiveFileComparer DateComparer { get { return new DateComparerImpl(); } }

        public abstract int Compare(ActiveFile x, ActiveFile y);

        public bool ReverseSort { get; set; }
    }
}