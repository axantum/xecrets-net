using System;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Axantum.AxCrypt.iOS.Infrastructure;
using System.Drawing;

namespace Axantum.AxCrypt.iOS
{
	public partial class MainViewController : DialogViewController
	{
		public event Action 
			OnRecentFilesButtonTapped,
			OnLocalFilesButtonTapped,
			OnAboutButtonTapped, 
			OnTroubleshootingButtonTapped, 
			OnFeedbackButtonTapped;

		public MainViewController () : base(null)
		{
			
		}

		void FadeViewController (UIViewController controller)
		{
			UIView.Animate (.5d, delegate {
				controller.View.Layer.Opacity = 0f;
			}, delegate {
				controller.RemoveFromParentViewController();
				controller.View.RemoveFromSuperview();
				controller.Dispose();
			});

		}

		public void OpenFile(string targetPath)
		{
			PassphraseViewController passphrase = new PassphraseViewController(targetPath);
			FilePresenter presenter = new FilePresenter (this);
			presenter.Done += delegate {
				FadeViewController (passphrase);
				TableView.UserInteractionEnabled = true;
			};
			passphrase.Decrypting += delegate {
				TableView.UserInteractionEnabled = false;
			};
			passphrase.FileDecrypted += decryptedFilePath => {
				presenter.Present(decryptedFilePath);
			};
			passphrase.Cancelled += delegate {
				FadeViewController (passphrase);
				TableView.UserInteractionEnabled = true;
			};

			AddChildViewController(passphrase);
			Add (passphrase.View);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (Root != null)
				return;

			//BeginInvokeOnMainThread(delegate {
			Theme.Configure ("AxCrypt", this);
			//});

			Root = new RootElement(String.Empty) {
				new Section { new ThemedStringElement("Recent files", OnRecentFilesButtonTapped) },
				new Section { new ThemedStringElement("Local files", OnLocalFilesButtonTapped) },
				new Section { 
					new ThemedStringElement("About", OnAboutButtonTapped),
					new ThemedStringElement("Troubleshooting", OnTroubleshootingButtonTapped),
					new ThemedStringElement("Feedback", OnFeedbackButtonTapped)
				},
			};
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return (toInterfaceOrientation == UIInterfaceOrientation.Portrait || toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown);
		}
	}
}
