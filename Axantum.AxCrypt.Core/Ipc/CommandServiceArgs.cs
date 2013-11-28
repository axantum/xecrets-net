using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Ipc
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CommandServiceArgs : EventArgs
    {
        [JsonProperty("Command")]
        public CommandVerb RequestCommand { get; private set; }

        [JsonProperty("Paths")]
        public IList<string> Paths { get; private set; }

        public CommandServiceArgs()
        {
            RequestCommand = CommandVerb.Unknown;
            Paths = new List<string>();
        }

        public CommandServiceArgs(CommandVerb requestCommand, IEnumerable<string> paths)
        {
            RequestCommand = requestCommand;
            Paths = new List<string>(paths);
        }

        public CommandServiceArgs(CommandVerb requestCommand, params string[] paths)
        {
            RequestCommand = requestCommand;
            Paths = new List<string>(paths);
        }
    }
}