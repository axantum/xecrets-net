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

using Axantum.AxCrypt.Core.Ipc;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core
{
    public class CommandLine
    {
        private string _startPath;

        private IEnumerable<string> _arguments;

        private bool Exit { get; set; }

        private static readonly IEnumerable<string> NoArguments = new string[0];

        public CommandLine(string startPath, IEnumerable<string> arguments)
        {
            _startPath = startPath;
            _arguments = arguments.ToList();
        }

        public void Execute()
        {
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x", var => Exit = true},
            };
            IList<string> files = options.Parse(_arguments);
            Run(files);
        }

        private void Run(IList<string> files)
        {
            if (Exit)
            {
                CallService(CommandVerb.Exit, NoArguments);
                return;
            }
            CallService(CommandVerb.Open, files);
        }

        private void CallService(CommandVerb verb, IEnumerable<string> files)
        {
            EnsureFirstInstanceRunning();
            CommandStatus status = Instance.CommandService.Call(verb, files);
            if (status == CommandStatus.Success)
            {
                return;
            }
            OS.Current.ExitApplication(1);
        }

        private void EnsureFirstInstanceRunning()
        {
            if (OS.Current.FirstInstanceRunning(TimeSpan.Zero))
            {
                return;
            }
            using (OS.Current.Launch(_startPath)) { }
            if (OS.Current.FirstInstanceRunning(TimeSpan.FromSeconds(1)))
            {
                return;
            }
            OS.Current.ExitApplication(2);
        }
    }
}