﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.Portable
{
    public interface IDataErrorInfo
    {
        int ValidationError { get; }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming for this pattern.")]
        string Error { get; }

        string this[string columnName] { get; }
    }
}