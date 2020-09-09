using System.Threading.Tasks;

namespace AxCrypt.Abstractions
{
    public interface IProgressDialog
    {
        Task<ProgressDialogClosingToken> Show(string title, string message);
    }
}