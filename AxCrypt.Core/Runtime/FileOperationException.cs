﻿#region Coypright and License

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
using System;

namespace AxCrypt.Core.Runtime
{
    /// <summary>
    /// A file access operation has failed, perhaps due to insufficient permissions, or space etc.
    /// </summary>
    public class FileOperationException : AxCryptException
    {
        public FileOperationException()
            : base()
        {
        }

        public FileOperationException(string description)
            : base(description, ErrorStatus.InternalError)
        {
        }

        public FileOperationException(string description, ErrorStatus errorStatus)
            : base(description, errorStatus)
        {
        }

        public FileOperationException(string description, string fullName, ErrorStatus errorStatus)
            : base(description, errorStatus)
        {
            DisplayContext = fullName;
        }

        public FileOperationException(string description, Exception innerException)
            : this(description, ErrorStatus.InternalError, innerException)
        {
        }

        public FileOperationException(string description, ErrorStatus errorStatus, Exception innerException)
            : base(description, errorStatus, innerException)
        {
        }

        public FileOperationException(string description, string fullName, ErrorStatus errorStatus, Exception innerException)
            : base(description, errorStatus, innerException)
        {
            DisplayContext = fullName;
        }
    }
}