using System;
using System.IO;
using MonoTouch.UIKit;
using Axantum.AxCrypt.Core.Crypto;

namespace Axantum.AxCrypt.iOS
{
	public partial class PassphraseController : IDisposable
	{
		public event Action<LogOnIdentity> Done = delegate {}; 
		public event Action Cancelled = delegate {};

		string path;
		UIAlertViewDelegate alertViewDelegate;
		UIAlertView alertView;

		public PassphraseController (string path)
		{
			this.path = path;
		}

		public void AskForPassword ()
		{
			string title, message; 

			if (alertViewDelegate == null) {
				alertViewDelegate = new UIAlertViewOkCancelDelegate(InvokeDone, Cancelled);
				title = Path.GetFileNameWithoutExtension(path);
				message = "Enter passphrase";
			}
			else {
				title = "The passphrase you entered could not be used to open file";
				message = "Try again?";
			}

			if (alertView == null) {
				alertView = new UIAlertView (title, message, alertViewDelegate, "Cancel", new string[] { "OK" });
				alertView.AlertViewStyle = UIAlertViewStyle.SecureTextInput;
			} 
			else {
				alertView.Title = title;
				alertView.Message = message;
			}
			alertView.Show ();
		}

		void InvokeDone(string passphrase) {
			Done (new LogOnIdentity(passphrase));
		}

		public void Dispose ()
		{
			if (this.alertViewDelegate != null) {
				this.alertViewDelegate.Dispose ();
				this.alertViewDelegate = null;
			}
			if (this.alertView != null) {
				this.alertView.Delegate = null;
				this.alertView.DismissWithClickedButtonIndex (alertView.CancelButtonIndex, false);
				this.alertView.Dispose ();
				this.alertView = null;
			}
		}
	}
}
