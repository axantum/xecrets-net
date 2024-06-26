﻿using Xecrets.Net.Content.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Content
{
    public class ContentResourceManager : ResourceManager
    {
        public ContentResourceManager()
            : base("AxCrypt.Content.Content", typeof(Texts).GetTypeInfo().Assembly)
        {
        }

        public override string? GetString(string name, CultureInfo? culture)
        {
            return Resources.ResourceManager.GetString(name, culture);
        }
    }
}
