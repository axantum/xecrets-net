using System;
using System.Drawing;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.iOS.Infrastructure;

namespace Axantum.AxCrypt.iOS
{
	public partial class PassphraseViewController : UIViewController
	{
		public event Action Decrypting = delegate {};
		public event Action<string> FileDecrypted = delegate {};
		public event Action Cancelled = delegate {};

		string path;
		UIProgressView progressView;
		UIActivityIndicatorView activityIndicator;
		UILabel progressText;

		UIAlertViewDelegate alertViewDelegate;
		UIAlertView alertView;

		public PassphraseViewController (string path) : base ()
		{
			this.path = path;
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
			alertViewDelegate = new UIAlertViewOkCancelDelegate(DecryptFile, Cancelled);
			alertView = new UIAlertView(title, message, alertViewDelegate, "Cancel", new string[] { "OK" });
			alertView.AlertViewStyle = UIAlertViewStyle.SecureTextInput;
			alertView.Show ();
		}

		void CreateLoadingView ()
		{
			const float subViewHeight = 20;
			float horizontalPadding = View.Bounds.Width / 4;
			float verticalMidpoint = UIScreen.MainScreen.ApplicationFrame.Height / 2;
			float frameWidth = UIScreen.MainScreen.ApplicationFrame.Width;

			View.Frame = new RectangleF(
				x: horizontalPadding,
				y: verticalMidpoint - subViewHeight * 2f,
				width: View.Bounds.Width - horizontalPadding * 2f,
				height: subViewHeight * 6
				);

			activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge) {
				Frame = new RectangleF(subViewHeight, subViewHeight * 2, frameWidth - horizontalPadding * 2 - subViewHeight * 2, subViewHeight),
				HidesWhenStopped = true,
				Color = Theme.HighlightColor
			};

			progressText = new UILabel {
				Frame = new RectangleF(0, subViewHeight * 3.5f, frameWidth - horizontalPadding * 2, subViewHeight * 2),
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
			View.BackgroundColor = UIColor.White.ColorWithAlpha(.95f);
			View.Layer.BorderColor = Theme.HighlightColor.CGColor;
			View.Layer.BorderWidth = Theme.BorderWith;
			View.Layer.CornerRadius = Theme.CornerRadius;
			View.Layer.Opacity = 0;
		}

		void SetProgress (int percent, string message)
		{
			InvokeOnMainThread(() => {
				Decrypting();

				if (percent > 0) {
					if (activityIndicator != null) {
						progressView = new UIProgressView(activityIndicator.Frame) {
							ProgressTintColor = Theme.HighlightColor,
						};
						progressView.Center = new PointF(progressView.Center.X, (View.Bounds.Height - progressView.Bounds.Height) / 2);

						UIView.Animate (.5d, delegate {
							activityIndicator.RemoveFromSuperview();
							View.Add(progressView);
							progressText.Center = new PointF(progressView.Center.X, progressView.Center.Y + 20);
						}, delegate {
							if (activityIndicator != null) {
								activityIndicator.Dispose();
								activityIndicator = null;
							}
						});
					}
					progressView.SetProgress(percent / 100f, true);
				}
				else {
					UIView.Animate(.5d, delegate {
						View.Layer.Opacity = .875f;
					});
					activityIndicator.StartAnimating();
					progressText.Hidden = false;
				}
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
				SetProgress(e.Percent, "Decrypting ...");
			};


			ThreadWorker worker = new ThreadWorker(progress);
			worker.Prepare += delegate { SetProgress(0, "Unlocking ..."); };
			worker.Work += (wo, wa) => {
				using (NSAutoreleasePool pool = new NSAutoreleasePool()) {
					AesKey key = new Passphrase(usingPassphrase).DerivedPassphrase;
					IRuntimeFileInfo sourceFile = OS.Current.FileInfo (path);
					targetFileName = AxCryptFile.Decrypt (sourceFile, targetDirectory, key, AxCryptOptions.None, progress);
				

				if (targetFileName == null) {
					wa.Result = FileOperationStatus.Canceled;
					return;
				}
				
				wa.Result = FileOperationStatus.Success;
				}
			};
			worker.Completed += (wo, wa) => InvokeOnMainThread(delegate {

				if (wa.Result == FileOperationStatus.Canceled) {
					activityIndicator.StopAnimating();
					UIView.Animate(.5d, delegate {
						View.Layer.Opacity = 0;
					});

					AskForPassword("The passphrase you entered could not be used to open file", "Try again?");
					worker.Dispose();
					return;
				}
				
				FileDecrypted(Path.Combine(targetDirectory, targetFileName));
				worker.Dispose();
			});

			worker.Run();
		}
	}
}
