using Axantum.AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    public class StartupTipMessage
    {
        public static readonly StartupTipMessage Empty = new StartupTipMessage(string.Empty, StartupTipProperties.Empty);

        public string Text { get; }

        public StartupTipProperties Properties { get; }

        public StartupTipMessage(string text, StartupTipProperties properties)
        {
            Text = text;
            Properties = properties;
        }
    }
}