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
            List<CommandItem> _commandItems = new List<CommandItem>();
            int bundleId = 0;
            CommandVerb fileVerb = CommandVerb.Unknown;

            OptionSetCollection options = new OptionSetCollection()
            {
                {"batch", var => bundleId = 0},
                {"bundle=", (int id) => bundleId = id},
                {"files", var => fileVerb = CommandVerb.AddFiles},
                {"encrypt", var => fileVerb = CommandVerb.Encrypt},
                {"decrypt", var => fileVerb = CommandVerb.Decrypt},
                {"wipe", var =>  fileVerb = CommandVerb.Wipe},
                {"open", var =>  fileVerb = CommandVerb.Open},
                {"rename", var => fileVerb = CommandVerb.RandomRename},
                {"show", var => _commandItems.Add(new CommandItem(CommandVerb.Show, bundleId, NoArguments))},
                {"exit", var => _commandItems.Add(new CommandItem(CommandVerb.Exit, bundleId, NoArguments))},
                {"use_application=", (string path) => _commandItems.Add(new CommandItem(CommandVerb.UseForOpen, bundleId, new string[]{path}))},
                {"login=", (string name) =>_commandItems.Add(new CommandItem(CommandVerb.LogOn, bundleId, new string[]{name}))},
                {"passphrase=", (string passphrase) => _commandItems.Add(new CommandItem(CommandVerb.SetPassphrase, bundleId, new string[]{passphrase}))},
                {"key_file=", (string path) => _commandItems.Add(new CommandItem(CommandVerb.SetKeyFile, bundleId, new string[]{path}))},
                {"about", var => _commandItems.Add(new CommandItem(CommandVerb.About, bundleId, NoArguments))},
                {"register", var => _commandItems.Add(new CommandItem(CommandVerb.Register, bundleId, NoArguments))},
            };
            IList<string> arguments = options.Parse(_arguments);
            if (fileVerb == CommandVerb.Unknown)
            {
                fileVerb = bundleId == 0 ? CommandVerb.Open : CommandVerb.AddFiles;
            }
            if (arguments.Count > 0 || bundleId != 0)
            {
                _commandItems.Add(new CommandItem(fileVerb, bundleId, arguments));
            }
            Run(_commandItems);
        }

        private void Run(IList<CommandItem> commandItems)
        {
            if (commandItems.Count == 0)
            {
                EnsureFirstInstanceRunning();
                return;
            }
            foreach (CommandItem commandItem in commandItems)
            {
                CallService(commandItem.Verb, commandItem.BundleId, commandItem.Arguments);
            }
        }

        private void CallService(CommandVerb verb, int batchId, IEnumerable<string> files)
        {
            bool firstIsRunning = OS.Current.FirstInstanceRunning(TimeSpan.Zero);
            if (verb == CommandVerb.Exit && !firstIsRunning)
            {
                return;
            }
            if (!firstIsRunning)
            {
                StartFirstInstance();
            }
            CommandStatus status = Instance.CommandService.Call(verb, batchId, files);
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
            StartFirstInstance();
        }

        private void StartFirstInstance()
        {
            using (OS.Current.Launch(_startPath)) { }
            if (OS.Current.FirstInstanceRunning(TimeSpan.FromSeconds(1)))
            {
                return;
            }
            OS.Current.ExitApplication(2);
        }
    }
}