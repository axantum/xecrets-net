using System.Threading.Tasks;

namespace AxCrypt.Core.UI
{
    public interface IDataItemSelection
    {
        Task HandleSelection(FileSelectionEventArgs e);
    }
}