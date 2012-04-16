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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt.Core
{
    [Serializable]
    public abstract class AxCryptException : Exception
    {
        public ErrorStatus ErrorStatus { get; set; }

        protected AxCryptException()
            : base()
        {
            ErrorStatus = ErrorStatus.Unknown;
        }

        protected AxCryptException(string message, ErrorStatus errorStatus)
            : base(message)
        {
            ErrorStatus = errorStatus;
        }

        protected AxCryptException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorStatus = (ErrorStatus)info.GetInt32("ErrorStatus");
        }

        protected AxCryptException(string message, ErrorStatus errorStatus, Exception innerException)
            : base(message, innerException)
        {
            ErrorStatus = errorStatus;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ErrorStatus", (int)ErrorStatus);
        }
    }
}