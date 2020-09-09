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
            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        public event EventHandler<FileSelectionEventArgs> SelectingFiles;

        protected virtual void OnSelectingFiles(FileSelectionEventArgs e)
        {
            EventHandler<FileSelectionEventArgs> handler = SelectingFiles;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public IAction SelectFiles { get; private set; }

        public IEnumerable<string> SelectedFiles { get { return GetProperty<IEnumerable<string>>("SelectedFiles"); } set { SetProperty("SelectedFiles", value); } }

        private void InitializePropertyValues()
        {
            SelectFiles = new DelegateAction<FileSelectionType>((selectionType) => SelectFilesAction(selectionType));
        }

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
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[0])
            {
                FileSelectionType = fileSelectionType,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return new string[0];
            }
            return fileSelectionArgs.SelectedFiles;
        }
    }
}