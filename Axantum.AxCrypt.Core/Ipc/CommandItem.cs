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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Ipc
{
    public class CommandItem
    {
        public CommandItem(CommandVerb verb, int bundleId, IEnumerable<string> arguments)
        {
            Verb = verb;
            BundleId = bundleId;

            List<string> args = new List<string>();
            args.AddRange(arguments);
            Arguments = args;
        }

        public CommandVerb Verb { get; private set; }

        public int BundleId { get; private set; }

        public IEnumerable<string> Arguments { get; private set; }
    }
}