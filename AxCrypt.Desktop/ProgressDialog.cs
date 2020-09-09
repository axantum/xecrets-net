using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxCrypt.Desktop
{
    public class ProgressDialog : IProgressDialog
    {
        public Task<ProgressDialogClosingToken> Show(string title, string message)
        {
            return Task.FromResult(new ProgressDialogClosingToken());
        }
    }
}