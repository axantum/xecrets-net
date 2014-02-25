// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Axantum.AxCrypt.Mac.Views
{
	[Register ("CreatePassphraseView")]
	partial class CreatePassphraseView
	{
		[Outlet]
		MonoMac.AppKit.NSSecureTextField passphrase { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField verification { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField encryptedFileName { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (passphrase != null) {
				passphrase.Dispose ();
				passphrase = null;
			}

			if (verification != null) {
				verification.Dispose ();
				verification = null;
			}

			if (encryptedFileName != null) {
				encryptedFileName.Dispose ();
				encryptedFileName = null;
			}
		}
	}

	[Register ("CreatePassphraseViewController")]
	partial class CreatePassphraseViewController
	{
		[Outlet]
		MonoMac.AppKit.NSSecureTextField passphrase { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField verification { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField encryptedFIleName { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (passphrase != null) {
				passphrase.Dispose ();
				passphrase = null;
			}

			if (verification != null) {
				verification.Dispose ();
				verification = null;
			}

			if (encryptedFIleName != null) {
				encryptedFIleName.Dispose ();
				encryptedFIleName = null;
			}
		}
	}
}
