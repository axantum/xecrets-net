using System;
using System.IO;
using MonoTouch.UIKit;
using Axantum.AxCrypt.Core.Crypto;

namespace Axantum.AxCrypt.iOS
{
	public partial class PassphraseViewController : UIViewController
	{
		public event Action<Passphrase> Done = delegate {}; 
		public event Action Cancelled = delegate {};

		string path;
		UIAlertViewDelegate alertViewDelegate;
		UIAlertView alertView;

		public PassphraseViewController (string path) : base ()
		{
			this.path = path;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			AskForPassword();
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
			Done (new Passphrase(passphrase));
		}
	}
}
