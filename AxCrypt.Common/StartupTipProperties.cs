using AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Common
{
    public class StartupTipProperties
    {
        public static readonly StartupTipProperties Empty = new StartupTipProperties(StartupTipLevel.Unknown, StartupTipButtonStyle.Unknown, null);

        public StartupTipLevel Level { get; private set; }

        public StartupTipButtonStyle ButtonStyle { get; private set; }

        public Uri Url { get; private set; }

        public StartupTipProperties(StartupTipLevel level, StartupTipButtonStyle buttonStyle, Uri url)
        {
            Level = level;
            ButtonStyle = buttonStyle;
            Url = url;
        }

        public StartupTipProperties(StartupTipLevel level, StartupTipButtonStyle buttonStyle)
        {
            Level = level; 
            ButtonStyle = buttonStyle;
        }
    }
}