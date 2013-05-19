// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Axantum.AxCrypt.Mac
{
	[Register ("VersionInformationWindowController")]
	partial class VersionInformationWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField versionLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton showOnNextExecution { get; set; }

		[Outlet]
		MonoMac.WebKit.WebView versionInformation { get; set; }

		[Outlet]
		MonoMac.AppKit.NSProgressIndicator progressIndicator { get; set; }

		[Action ("closeWindow:")]
		partial void closeWindow (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (versionLabel != null) {
				versionLabel.Dispose ();
				versionLabel = null;
			}

			if (showOnNextExecution != null) {
				showOnNextExecution.Dispose ();
				showOnNextExecution = null;
			}

			if (versionInformation != null) {
				versionInformation.Dispose ();
				versionInformation = null;
			}

			if (progressIndicator != null) {
				progressIndicator.Dispose ();
				progressIndicator = null;
			}
		}
	}

	[Register ("VersionInformationWindow")]
	partial class VersionInformationWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
