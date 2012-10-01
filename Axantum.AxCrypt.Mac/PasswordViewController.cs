
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Axantum.AxCrypt.Core.Crypto;

namespace Axantum.AxCrypt.Mac
{
	public partial class PasswordViewController : MonoMac.AppKit.NSViewController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public PasswordViewController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public PasswordViewController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public PasswordViewController () : base ("PasswordView", NSBundle.MainBundle)
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}
		
		#endregion
		
		//strongly typed view accessor
		public new PasswordView View {
			get {
				return (PasswordView)base.View;
			}
		}

		public Passphrase Passphrase {
			get {
				return new Passphrase(View.EnteredPassphrase);
			}
		}
	}
}

