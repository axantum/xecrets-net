using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using Axantum.AxCrypt.iOS.Infrastructure;

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

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (Root != null)
				return;

			//BeginInvokeOnMainThread(delegate {
			Theme.Configure ("AxCrypt", this);
			//});

			Root = new RootElement(String.Empty) {
				new Section("", "documents received from other apps") { new ThemedStringElement("Received documents", OnRecentFilesButtonTapped) },
				new Section("", "documents transferred from iTunes") { new ThemedStringElement("Transferred documents", OnLocalFilesButtonTapped) },
				new Section { 
					new ThemedStringElement("About", OnAboutButtonTapped),
					new ThemedStringElement("Troubleshooting", OnTroubleshootingButtonTapped),
					new ThemedStringElement("Feedback", OnFeedbackButtonTapped)
				},
			};
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return (toInterfaceOrientation == UIInterfaceOrientation.Portrait || toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown);
		}
	}
}
