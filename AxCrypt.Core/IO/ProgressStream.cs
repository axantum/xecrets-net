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

using AxCrypt.Core.UI;
using System;
using System.IO;

namespace AxCrypt.Core.IO
{
    public class ProgressStream : WrappedBaseStream
    {
        private IProgressContext _progress;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressStream"/> class.
        /// </summary>
        /// <param name="stream">The stream. Will be disposed of when this instance is disposed.</param>
        /// <param name="progress">The progress.</param>
        /// <exception cref="System.ArgumentNullException">
        /// stream
        /// or
        /// progress
        /// </exception>
        public ProgressStream(Stream stream, IProgressContext progress)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            WrappedStream = stream;
            _progress = progress;

            _progress.NotifyLevelStart();
            if (stream.CanSeek)
            {
                _progress.AddTotal(WrappedStream.Length - WrappedStream.Position);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytes = base.Read(buffer, offset, count);

            _progress.AddCount(bytes);
            return bytes;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);

            _progress.AddCount(count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
            base.Dispose(disposing);
        }

        private void DisposeInternal()
        {
            if (IsDisposed)
            {
                return;
            }

            _progress.NotifyLevelFinished();
        }
    }
}