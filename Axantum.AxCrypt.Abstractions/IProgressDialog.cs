using System.Threading.Tasks;

namespace Axantum.AxCrypt.Abstractions
{
    public interface IProgressDialog
    {
        Task<ProgressDialogClosingToken> Show(string title, string message);
    }
}