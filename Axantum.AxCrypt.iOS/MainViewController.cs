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

		public MainViewController () : base(new RootElement(String.Empty))
		{
			
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			if (!Utilities.UserInterfaceIdiomIsPhone) {
				TableView.Source = new EditableTableViewSource (this);
			}
		}

		FileListingViewController recent, local;
		public override void ViewWillAppear (bool animated)
		{
			if (Root.Count != 0)
				return;

			Theme.Configure ("AxCrypt", this);

			Section receivedDocumentsSection = new Section ();
			Section transferredDocumentsSection = new Section();

			if (Utilities.UserInterfaceIdiomIsPhone) {
				receivedDocumentsSection.Add (new ThemedStringElement ("Received documents", OnRecentFilesButtonTapped));
				transferredDocumentsSection.Add (new ThemedStringElement("Transferred documents", OnLocalFilesButtonTapped));

				if (Utilities.iPhone5OrPad) {
					// We've got plenty of vertical space to be a little more verbose
					receivedDocumentsSection.Footer = "documents received from other apps";
					transferredDocumentsSection.Footer = "documents transferred from iTunes";
				}
			} else {

				receivedDocumentsSection.Caption = "Documents received from other Apps";
				receivedDocumentsSection.Footer = " ";
				recent = new FileListingViewController (String.Empty, BasePath.ReceivedFilesId);
				recent.OpenFile += AppDelegate.Current.HandleOpenFile;
				receivedDocumentsSection.AddAll (recent.GetElements());

				transferredDocumentsSection.Caption = "Documents transferred via iTunes";
				transferredDocumentsSection.Footer = " ";
				local = new FileListingViewController (String.Empty, BasePath.TransferredFilesId);
				local.OpenFile += AppDelegate.Current.HandleOpenFile;
				transferredDocumentsSection.AddAll (local.GetElements());

			}


			Root.Add (new[] {
				receivedDocumentsSection,
				transferredDocumentsSection,
			});

			if (!Utilities.UserInterfaceIdiomIsPhone) {
				Root.Add (new Section(String.Empty));
			}

			Root.Add (new [] { 
				new Section { 
					new ThemedStringElement("About", OnAboutButtonTapped),
					new ThemedStringElement("Frequently Asked Questions", OnFaqButtonTapped),
					new ThemedStringElement("Troubleshooting", OnTroubleshootingButtonTapped),
					new ThemedStringElement("Feedback", OnFeedbackButtonTapped)
				},
			});

			TableView.ScrollEnabled = receivedDocumentsSection.Count + receivedDocumentsSection.Count > 20;
			base.ViewWillAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			//return (toInterfaceOrientation == UIInterfaceOrientation.Portrait || toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown);
			return true;
		}
	}
}
