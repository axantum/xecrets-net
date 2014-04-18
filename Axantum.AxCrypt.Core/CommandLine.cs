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

namespace Axantum.AxCrypt.Core
{
    public class CommandLine
    {
        private enum MutuallyExclusiveOptions
        {
            NoneSet,
            Encrypt,
            Decrypt,
            Wipe,
        }

        private enum InclusiveOptions
        {
            NoneSet,
            LogOff,
            Exit,
            Show,
        }

        private MutuallyExclusiveOptions _mutuallyExclusiveOptions = MutuallyExclusiveOptions.NoneSet;

        private InclusiveOptions _inclusiveOptions = InclusiveOptions.NoneSet;

        private bool _commandLineError = false;

        private string _startPath;

        private IEnumerable<string> _arguments;

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
                {"z", var => SetMutuallyExclusiveOption(MutuallyExclusiveOptions.Encrypt)},
                {"d", var => SetMutuallyExclusiveOption(MutuallyExclusiveOptions.Decrypt)},
                {"w", var => SetMutuallyExclusiveOption(MutuallyExclusiveOptions.Wipe)},
                {"show", var => _inclusiveOptions |= InclusiveOptions.Show},
                {"t", var => _inclusiveOptions |= InclusiveOptions.LogOff},
                {"x", var => _inclusiveOptions |= InclusiveOptions.Exit},
                {"b=", (int batch) => {}},
            };
            IList<string> files = options.Parse(_arguments);
            if (!ValidateArguments())
            {
                return;
            }
            Run(files);
        }

        private void SetMutuallyExclusiveOption(MutuallyExclusiveOptions option)
        {
            if (_mutuallyExclusiveOptions != MutuallyExclusiveOptions.NoneSet)
            {
                _commandLineError = true;
                return;
            }
            _mutuallyExclusiveOptions = option;
        }

        private bool ValidateArguments()
        {
            if (_commandLineError)
            {
                return false;
            }
            return true;
        }

        private void Run(IList<string> files)
        {
            switch (_mutuallyExclusiveOptions)
            {
                case MutuallyExclusiveOptions.Encrypt:
                    CallService(CommandVerb.Encrypt, files);
                    break;

                case MutuallyExclusiveOptions.Decrypt:
                    CallService(CommandVerb.Decrypt, files);
                    break;

                case MutuallyExclusiveOptions.Wipe:
                    CallService(CommandVerb.Wipe, files);
                    break;

                default:
                    break;
            }

            if (_mutuallyExclusiveOptions == MutuallyExclusiveOptions.NoneSet)
            {
                CallService(CommandVerb.Open, files);
            }

            if (_inclusiveOptions.HasFlag(InclusiveOptions.Show))
            {
                CallService(CommandVerb.Show, NoArguments);
            }
            if (_inclusiveOptions.HasFlag(InclusiveOptions.LogOff))
            {
                CallService(CommandVerb.LogOff, NoArguments);
            }
            if (_inclusiveOptions.HasFlag(InclusiveOptions.Exit))
            {
                CallService(CommandVerb.Exit, NoArguments);
            }
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