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

namespace Axantum.AxCrypt.Core.Reader
{
    public enum AxCryptItemType
    {
        /// <summary>
        /// Initial state before we have read any items at all
        /// </summary>
        None,
        /// <summary>
        /// We have seen the AxCrypt Guid
        /// </summary>
        MagicGuid,
        /// <summary>
        /// A header block of HeaderBlockType has been found
        /// </summary>
        HeaderBlock,
        /// <summary>
        /// A (part) of Encrypted and possibly Compressed data has been found
        /// </summary>
        Data,
        /// <summary>
        /// The end of the stream has been reached
        /// </summary>
        EndOfStream,
    }
}