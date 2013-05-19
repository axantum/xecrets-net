// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Axantum.AxCrypt.Mac.Windows
{
	[Register ("AboutWindowController")]
	partial class AboutWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField versionLabel { get; set; }

		[Action ("axantumLogoClicked:")]
		partial void axantumLogoClicked (MonoMac.Foundation.NSObject sender);

		[Action ("bouncyLinkClicked:")]
		partial void bouncyLinkClicked (MonoMac.Foundation.NSObject sender);

		[Action ("tretton37LogoClicked:")]
		partial void tretton37LogoClicked (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (versionLabel != null) {
				versionLabel.Dispose ();
				versionLabel = null;
			}
		}
	}

	[Register ("AboutWindow")]
	partial class AboutWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
