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

using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Ipc
{
    public class CommandHandler
    {
        private static ArgumentBundles _bundles = new ArgumentBundles();

        public void RequestReceived(object sender, CommandServiceEventArgs e)
        {
            if (!Instance.KnownKeys.IsLoggedOn)
            {
                OnCommandComplete(new CommandCompleteEventArgs(CommandVerb.Show, new string[0]));
                return;
            }
            switch (e.Verb)
            {
                case CommandVerb.Unknown:
                    break;

                case CommandVerb.AddFiles:
                    _bundles.AddArguments(e.BundleId, e.Arguments);
                    break;

                case CommandVerb.Encrypt:
                case CommandVerb.Decrypt:
                case CommandVerb.Open:
                case CommandVerb.Wipe:
                case CommandVerb.RandomRename:
                    _bundles.AddArguments(e.BundleId, e.Arguments);
                    OnCommandComplete(new CommandCompleteEventArgs(e.Verb, _bundles.Arguments(e.BundleId)));
                    break;

                default:
                    OnCommandComplete(new CommandCompleteEventArgs(e.Verb, e.Arguments));
                    break;
            }
        }

        public event EventHandler<CommandCompleteEventArgs> CommandComplete;

        protected virtual void OnCommandComplete(CommandCompleteEventArgs e)
        {
            EventHandler<CommandCompleteEventArgs> handler = CommandComplete;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}