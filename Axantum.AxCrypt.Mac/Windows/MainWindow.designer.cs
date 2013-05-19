// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Axantum.AxCrypt.Mac.Windows
{
	[Register ("MainWindow")]
	partial class MainWindow
	{
		[Outlet]
		MonoMac.AppKit.NSButton openingButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSProgressIndicator openingIndicator { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton encryptingButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSProgressIndicator encryptingIndicator { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton decryptingButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSProgressIndicator decryptingIndicator { get; set; }

		[Action ("viewClicked:")]
		partial void viewClicked (MonoMac.Foundation.NSObject sender);

		[Action ("encryptClicked:")]
		partial void encryptClicked (MonoMac.Foundation.NSObject sender);

		[Action ("decryptClicked:")]
		partial void decryptClicked (MonoMac.Foundation.NSObject sender);

		[Action ("aboutClicked:")]
		partial void aboutClicked (MonoMac.Foundation.NSObject sender);

		[Action ("urlClicked:")]
		partial void urlClicked (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (openingButton != null) {
				openingButton.Dispose ();
				openingButton = null;
			}

			if (openingIndicator != null) {
				openingIndicator.Dispose ();
				openingIndicator = null;
			}

			if (encryptingButton != null) {
				encryptingButton.Dispose ();
				encryptingButton = null;
			}

			if (encryptingIndicator != null) {
				encryptingIndicator.Dispose ();
				encryptingIndicator = null;
			}

			if (decryptingButton != null) {
				decryptingButton.Dispose ();
				decryptingButton = null;
			}

			if (decryptingIndicator != null) {
				decryptingIndicator.Dispose ();
				decryptingIndicator = null;
			}
		}
	}

	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
