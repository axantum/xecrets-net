using Axantum.AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    public class StartupTipProperties
    {
        public static readonly StartupTipProperties Empty = new StartupTipProperties(StartupTipLevel.Unkown, StartupTipButtonStyle.Unknown, null);

        public StartupTipLevel Level { get; private set; }

        public StartupTipButtonStyle ButtonStyle { get; private set; }

        public Uri Url { get; private set; }

        public StartupTipProperties(StartupTipLevel level, StartupTipButtonStyle buttonStyle, Uri url)
        {
            Level = level;
            ButtonStyle = buttonStyle;
            Url = url;
        }
    }
}