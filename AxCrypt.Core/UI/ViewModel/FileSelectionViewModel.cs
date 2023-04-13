using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.UI.ViewModel
{
    public class FileSelectionViewModel : ViewModelBase
    {
        public FileSelectionViewModel()
        {
            SelectFiles = new DelegateAction<FileSelectionType>((selectionType) => SelectFilesAction(selectionType));
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        public event EventHandler<FileSelectionEventArgs>? SelectingFiles;

        protected virtual void OnSelectingFiles(FileSelectionEventArgs e)
        {
            SelectingFiles?.Invoke(this, e);
        }

        public IAction SelectFiles { get; private set; }

        public IEnumerable<string> SelectedFiles { get { return GetProperty<IEnumerable<string>>("SelectedFiles"); } set { SetProperty("SelectedFiles", value); } }

        private static void BindPropertyChangedEvents()
        {
        }

        private static void SubscribeToModelEvents()
        {
        }

        private void SelectFilesAction(FileSelectionType selectionType)
        {
            SelectedFiles = SelectFilesInternal(selectionType);
        }

        private IEnumerable<string> SelectFilesInternal(FileSelectionType fileSelectionType)
        {
            var fileSelectionArgs = new FileSelectionEventArgs(Array.Empty<string>())
            {
                FileSelectionType = fileSelectionType,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return Array.Empty<string>();
            }
            return fileSelectionArgs.SelectedFiles;
        }
    }
}
