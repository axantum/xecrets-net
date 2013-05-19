
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Axantum.AxCrypt.Core.Crypto;

namespace Axantum.AxCrypt.Mac.Views
{
	public partial class CreatePassphraseViewController : MonoMac.AppKit.NSViewController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public CreatePassphraseViewController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public CreatePassphraseViewController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public CreatePassphraseViewController () : base ("CreatePassphraseView", NSBundle.MainBundle)
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}
		
		#endregion
		
		//strongly typed view accessor
		public new CreatePassphraseView View {
			get {
				return (CreatePassphraseView)base.View;
			}
		}

		public Passphrase VerifiedPassphrase {
			get {
				if (View.VerifiedPassphrase == null) {
					NSAlert
						.WithMessage("Phrase mismatch", "OK", null, null, "The two phrases you have entered either do not match, or they are empty")
							.RunModal();
					return null;
				}

				return new Passphrase(View.VerifiedPassphrase);
			}
		}

		public string EncryptedFileName {
			get {
				return View.EncryptedFileName;
			}
			set {
				View.EncryptedFileName = value;
			}
		}
	}
}

