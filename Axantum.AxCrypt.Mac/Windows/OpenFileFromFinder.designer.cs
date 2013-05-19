// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Axantum.AxCrypt.Mac.Windows
{
	[Register ("OpenFileFromFinderController")]
	partial class OpenFileFromFinderController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField FileName { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField Passphrase { get; set; }

		[Action ("Open:")]
		partial void Open (MonoMac.Foundation.NSObject sender);

		[Action ("Cancel:")]
		partial void Cancel (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (FileName != null) {
				FileName.Dispose ();
				FileName = null;
			}

			if (Passphrase != null) {
				Passphrase.Dispose ();
				Passphrase = null;
			}
		}
	}

	[Register ("OpenFileFromFinder")]
	partial class OpenFileFromFinder
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
