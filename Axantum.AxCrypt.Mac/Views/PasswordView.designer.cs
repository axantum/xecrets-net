// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Axantum.AxCrypt.Mac.Views
{
	[Register ("PasswordView")]
	partial class PasswordView
	{
		[Outlet]
		MonoMac.AppKit.NSSecureTextField passphrase { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (passphrase != null) {
				passphrase.Dispose ();
				passphrase = null;
			}
		}
	}

	[Register ("PasswordViewController")]
	partial class PasswordViewController
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
