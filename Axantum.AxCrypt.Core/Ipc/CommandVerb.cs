using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Ipc
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CommandVerb
    {
        Unknown,
        Open,
        Show,
        Exit,
    }
}