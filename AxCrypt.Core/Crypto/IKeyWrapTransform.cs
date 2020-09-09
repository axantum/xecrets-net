using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AxCrypt.Core.Crypto
{
    public interface IKeyWrapTransform : IDisposable
    {
        int BlockLength { get; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "A", Justification = "NIST Key Wrap specification refers to this value as 'A'")]
        byte[] A();

        byte[] TransformBlock(byte[] block);
    }
}