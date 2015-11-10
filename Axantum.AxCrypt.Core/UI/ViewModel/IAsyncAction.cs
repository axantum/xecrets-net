﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public interface IAsyncAction : IAction
    {
        Task ExecuteAsync(object parameter);
    }
}