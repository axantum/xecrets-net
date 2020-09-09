using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Common
{
    /// <summary>
    /// Helper class to make passing names of fields and properties type safe and less error prone.
    /// </summary>
    public class NameOf
    {
        public static readonly NameOf Empty = new NameOf(string.Empty);

        public NameOf(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}