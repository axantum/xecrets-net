
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Diagnostics;

namespace Axantum.AxCrypt.Mac
{
	public partial class AboutWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public AboutWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public AboutWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public AboutWindowController () : base ("AboutWindow")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}
		
		#endregion
		
		//strongly typed window accessor
		public new AboutWindow Window {
			get {
				return (AboutWindow)base.Window;
			}
		}

		partial void axantumLogoClicked (NSObject sender)
		{
			Process.Start("http://www.axantum.com/");
		}

		partial void bouncyLinkClicked (NSObject sender)
		{
			Process.Start("http://www.bouncycastle.org/");
		}

		partial void tretton37LogoClicked (NSObject sender)
		{
			Process.Start("http://www.tretton37.com/");
		}

		public void SetVersion(string version)
		{
			versionLabel.StringValue = version;
		}
	}
}

