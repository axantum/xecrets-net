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
            OptionSet options = new OptionSet()
            {
                {"x", var => Exit = true},
            };
            List<string> files = options.Parse(_arguments);
            Run(files);
        }

        private void Run(List<string> files)
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