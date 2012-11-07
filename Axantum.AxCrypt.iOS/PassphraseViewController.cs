using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Drawing;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using System.ComponentModel;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;
using System.IO;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.iOS
{
	public partial class PassphraseViewController : AppViewController
	{
		public event Action<string> FileDecrypted;
		public event Action Cancelled;

		string path;
		UIActivityIndicatorView activityIndicator;
		UILabel progressText;

		public PassphraseViewController (string path) : base ()
		{
			this.path = path;
			View.Frame = new RectangleF(0, 20, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - 20);
			TableView.UserInteractionEnabled = false;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			CreateLoadingView();
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			AskForPassword();
		}
		
		void AskForPassword (string title = "%fileName%", string message = "Enter passphrase")
		{
			title = title.Replace("%fileName%", Path.GetFileNameWithoutExtension(path));
			UIAlertViewDelegate del = new UIAlertViewOkCancelDelegate(DecryptFile, Cancelled);
			UIAlertView view = new UIAlertView(title, message, del, "Cancel", new string[] { "OK" });
			view.AlertViewStyle = UIAlertViewStyle.SecureTextInput;
			view.Show ();
		}

		void CreateLoadingView ()
		{
			const float subViewHeight = 20;
			const float horizontalPadding = 30;
			float verticalMidpoint = UIScreen.MainScreen.ApplicationFrame.Height / 2;
			float frameWidth = UIScreen.MainScreen.ApplicationFrame.Width;

			activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge) {
				Frame = new RectangleF(horizontalPadding, verticalMidpoint - subViewHeight, frameWidth - horizontalPadding * 2, subViewHeight),
				HidesWhenStopped = true,
				Color = highlightColor,
			};

			progressText = new UILabel {
				Frame = new RectangleF(horizontalPadding, verticalMidpoint + subViewHeight / 2, frameWidth - horizontalPadding * 2, subViewHeight * 2),
				BackgroundColor = UIColor.Clear,
				TextColor = UIColor.Black,
				TextAlignment = UITextAlignment.Center,
				Text = Path.GetFileNameWithoutExtension(path),
				Hidden = false,
				Font = UIFont.BoldSystemFontOfSize(14),
				LineBreakMode = UILineBreakMode.WordWrap,
				Lines = 2
			};

			View.AddSubview(activityIndicator);
			View.AddSubview(progressText);
		}

		void SetProgress (int percent, string message)
		{
			InvokeOnMainThread(() => {
				activityIndicator.StartAnimating();
				progressText.Hidden = false;
				progressText.Text = message;
			});
		}

		void DecryptFile (string usingPassphrase)
		{
			if (FileDecrypted == null || String.IsNullOrEmpty (usingPassphrase))
				return;

			string targetFileName = null;
			string targetDirectory = Path.GetTempPath();

			ProgressContext progress = new ProgressContext();
			progress.Progressing += (sender, e) => {
				SetProgress(e.Percent, String.Format("{0}%", e.Percent));
			};


			ThreadWorker worker = new ThreadWorker(progress);
			worker.Prepare += delegate { SetProgress(0, "Unlocking ..."); };
			worker.Work += (wo, wa) => {
				using (NSAutoreleasePool pool = new NSAutoreleasePool()) {
					AesKey key = new Passphrase(usingPassphrase).DerivedPassphrase;
					IRuntimeFileInfo sourceFile = OS.Current.FileInfo (path);
					targetFileName = AxCryptFile.Decrypt (sourceFile, targetDirectory, key, AxCryptOptions.None, progress);
				}

				if (targetFileName == null) {
					wa.Result = FileOperationStatus.Canceled;
					return;
				}
				
				wa.Result = FileOperationStatus.Success;
			};
			worker.Completed += (wo, wa) => InvokeOnMainThread(delegate {
				activityIndicator.StopAnimating();

				if (wa.Result == FileOperationStatus.Canceled) {
					AskForPassword("The passphrase you entered could not be used to open file", "Try again?");
					return;
				}
				
				FileDecrypted(Path.Combine(targetDirectory, targetFileName));
			});

			worker.Run();
		}
	}
}
