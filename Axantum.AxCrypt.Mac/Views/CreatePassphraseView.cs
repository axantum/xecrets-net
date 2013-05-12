
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Axantum.AxCrypt.Mac
{
	public partial class CreatePassphraseView : MonoMac.AppKit.NSView
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public CreatePassphraseView (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public CreatePassphraseView (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}
		
		#endregion

		public string VerifiedPassphrase {
			get {
				string phraseString = passphrase.StringValue;
				string verificationString = verification.StringValue;
				if (String.IsNullOrEmpty(phraseString) || String.IsNullOrEmpty(verificationString) || phraseString != verificationString)
					return null;

				return phraseString;
			}
		}

		public string EncryptedFileName {
			get {
				return encryptedFileName.StringValue;
			}
			set
			{
				encryptedFileName.StringValue = value;
			}
		}
	}
}

