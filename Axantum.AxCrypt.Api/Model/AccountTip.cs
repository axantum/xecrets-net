using Axantum.AxCrypt.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountTip
    {
        public AccountTip()
        {
            Message = string.Empty;
            Level = StartupTipLevel.Unkown;
            ButtonStyle = StartupTipButtonStyle.Unknown;
        }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("level")]
        public StartupTipLevel Level { get; set; }

        [JsonProperty("button_style")]
        public StartupTipButtonStyle ButtonStyle { get; set; }
    }
}