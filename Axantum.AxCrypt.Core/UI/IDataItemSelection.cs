using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IDataItemSelection
    {
        Task HandleSelection(FileSelectionEventArgs e);
    }
}