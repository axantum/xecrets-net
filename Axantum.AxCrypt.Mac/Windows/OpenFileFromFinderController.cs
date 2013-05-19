using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Axantum.AxCrypt.Mac.Windows
{
	public partial class OpenFileFromFinderController : MonoMac.AppKit.NSWindowController
	{
		public event Action<string> UserChoseOpen;
		public event Action UserChoseCancel;

		#region Constructors
		
		// Called when created from unmanaged code
		public OpenFileFromFinderController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public OpenFileFromFinderController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public OpenFileFromFinderController () : base ("OpenFileFromFinder")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		
		//strongly typed window accessor
		public new OpenFileFromFinder Window {
			get {
				return (OpenFileFromFinder)base.Window;
			}
		}

		partial void Open(NSObject sender) {
			if (UserChoseOpen != null)
				UserChoseOpen(Passphrase.StringValue);
			Dispose();
		}

		partial void Cancel(NSObject sender) {
			if (UserChoseCancel != null)
				UserChoseCancel();
			Dispose ();
		}

		public void ShowWindow (string fileName, NSObject sender)
		{
			base.ShowWindow (sender);
			FileName.StringValue = fileName;
		}
	}
}

