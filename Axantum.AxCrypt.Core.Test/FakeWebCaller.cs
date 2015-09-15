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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeWebCaller : IWebCaller
    {
        private WebCallerResponse _result;

        public event EventHandler<EventArgs> Calling;

        public FakeWebCaller(string result)
        {
            _result = new WebCallerResponse(HttpStatusCode.OK, result);
        }

        #region IWebCaller Members

        public WebCallerResponse Send(LogOnIdentity identity, WebCallerRequest request)
        {
            OnCalling();
            return _result;
        }

        #endregion IWebCaller Members

        private void OnCalling()
        {
            EventHandler<EventArgs> handler = Calling;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }
}