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
			OnFaqButtonTapped,
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

			Theme.Configure ("AxCrypt", this);

			Section receivedDocumentsSection = new Section {
				new ThemedStringElement ("Received documents", OnRecentFilesButtonTapped)
			};
			Section transferredDocumentsSection = new Section { 
				new ThemedStringElement("Transferred documents", OnLocalFilesButtonTapped) 
			};

			if (Utilities.iPhone5OrPad) {
				// We've got plenty of vertical space to be a little more verbose
				receivedDocumentsSection.Footer = "documents received from other apps";
				transferredDocumentsSection.Footer = "documents transferred from iTunes";
			}

			Root = new RootElement(String.Empty) {
				receivedDocumentsSection,
				transferredDocumentsSection,
				new Section { 
					new ThemedStringElement("About", OnAboutButtonTapped),
					new ThemedStringElement("Frequently Asked Questions", OnFaqButtonTapped),
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
