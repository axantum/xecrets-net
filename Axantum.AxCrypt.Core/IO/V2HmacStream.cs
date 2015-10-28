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

using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Crypto;
using System;
using System.IO;

namespace Axantum.AxCrypt.Core.IO
{
    public sealed class V2HmacStream : V2HmacStream<Stream>
    {
        /// <summary>
        /// Creates a AxCrypt HMAC-SHA-512 calculating stream.
        /// </summary>
        /// <typeparam name="TChained">The type of the chained actual output stream.</typeparam>
        /// <param name="hmacCalculator">The hmac calculator to use.</param>
        /// <param name="chainedStream">The chained stream. Will be disposed when this instance is disposed.</param>
        /// <returns>A stream to write data to calculate HMAC for to.</returns>
        /// <remarks>This factory method is used instead of a constructor in order to use type inference and offer a cleaner syntax for the comsumer.</remarks>
        public static V2HmacStream<TChained> Create<TChained>(V2HmacCalculator hmacCalculator, TChained chainedStream) where TChained : Stream
        {
            return ChainedStream<TChained>.Create((chained) => new V2HmacStream<TChained>(hmacCalculator, chainedStream), chainedStream);
        }

        /// <summary>
        /// Instantiate an AxCrypt HMAC-calculating stream.
        /// </summary>
        /// <param name="hmacCalculator">The hmac calculator to use.</param>
        /// <returns>A stream to write data to calculate HMAC for to.</returns>
        /// <remarks>This factory method is used instead of a constructor in order to use type inference and offer a cleaner syntax for the comsumer.</remarks>
        public static V2HmacStream<Stream> Create(V2HmacCalculator hmacCalculator)
        {
            return Create<Stream>(hmacCalculator, Stream.Null);
        }
    }

    /// <summary>
    /// An AxCrypt HMAC-SHA-512-calculating stream filter
    /// </summary>
    /// <typeparam name="T">The type of the stream to actually write to</typeparam>
    public class V2HmacStream<T> : ChainedStream<T> where T : Stream
    {
        private V2HmacCalculator _calculator;

        protected V2HmacStream()
            : base(Stream.Null as T)
        {
        }

        protected V2HmacStream(V2HmacCalculator calculator)
            : this(calculator, Stream.Null as T)
        {
        }

        /// <summary>
        /// An AxCrypt HMAC-SHA-512-calculating stream.
        /// </summary>
        /// <param name="key">The key for the HMAC</param>
        /// <param name="chainedStream">A stream where data is written to. This stream is disposed of when this instance is disposed.</param>
        internal V2HmacStream(V2HmacCalculator calculator, T chainedStream)
            : base(chainedStream)
        {
            if (calculator == null)
            {
                throw new ArgumentNullException("calculator");
            }

            _calculator = calculator;
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return _calculator.Count; }
        }

        public override long Position
        {
            get
            {
                return _calculator.Count;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Flush()
        {
            Chained.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureNotDisposed();
            _calculator.Write(buffer, offset, count);
            Chained.Write(buffer, offset, count);
        }

        public Hmac Hmac { get { return _calculator.Hmac; } }

        private void EnsureNotDisposed()
        {
            if (Chained == null)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}