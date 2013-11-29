using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt.Presentation
{
    public class ButtonPresentation : INotify
    {
        public bool? LogonEnabled {get; private set;}

        public bool? EncryptFileEnabled { get; private set; }

        public bool? DecryptFileEnabled { get; private set; }

        public bool? OpenEncryptedEnabled { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        //public void ToggleLogOn()
        //{
        //    if (Instance.KnownKeys.IsLoggedOn)
        //    {
        //        Instance.KnownKeys.Clear();
        //        return;
        //    }

        //    if (Instance.FileSystemState.Identities.Any(identity => true))
        //    {
        //        TryLogOnToExistingIdentity();
        //    }
        //    else
        //    {
        //        string passphrase = _passphrasePresentation.AskForNewEncryptionPassphrase(String.Empty);
        //        if (String.IsNullOrEmpty(passphrase))
        //        {
        //            return;
        //        }

        //        Instance.KnownKeys.DefaultEncryptionKey = Passphrase.Derive(passphrase);
        //    }
        //}
        //private void TryLogOnToExistingIdentity()
        //{
        //    string passphrase = _passphrasePresentation.AskForLogOnPassphrase(PassphraseIdentity.Empty);
        //    if (String.IsNullOrEmpty(passphrase))
        //    {
        //        return;
        //    }
        //}

        public event EventHandler<DialogEventArgs> PassphraseNeeded;

        protected virtual void OnPassphraseNeeded(DialogEventArgs e) {
            EventHandler<DialogEventArgs> handler = PassphraseNeeded;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Notify()
        {
            this.SetProperty("LogonEnabled", Instance.KnownKeys.DefaultEncryptionKey == null);
            this.SetProperty("EncryptFileEnabled", Instance.KnownKeys.DefaultEncryptionKey != null);
            this.SetProperty("DecryptFileEnabled", Instance.KnownKeys.DefaultEncryptionKey != null);
            this.SetProperty("OpenEncryptedEnabled", Instance.KnownKeys.DefaultEncryptionKey != null);
        }
    }
}