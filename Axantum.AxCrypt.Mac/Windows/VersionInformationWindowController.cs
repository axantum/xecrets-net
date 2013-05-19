using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Axantum.AxCrypt.Mac
{
	public partial class VersionInformationWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public VersionInformationWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public VersionInformationWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public VersionInformationWindowController () : base ("VersionInformationWindow")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
			// 
		}
		#endregion

		public override void WindowDidLoad ()
		{
			progressIndicator.StartAnimation (this);
			versionInformation.CommitedLoad += delegate {
				progressIndicator.StopAnimation(this);
			};

			NSUrlRequest request = NSUrlRequest.FromUrl (NSUrl.FromString(AppController.VersionInformationUrl));
			versionInformation.MainFrame.LoadRequest (request);
			showOnNextExecution.State = ShouldShowVersionInformation ? NSCellStateValue.On : NSCellStateValue.Off;

			base.WindowDidLoad ();
		}
		
		//strongly typed window accessor
		public new VersionInformationWindow Window {
			get {
				return (VersionInformationWindow)base.Window;
			}
		}

		private static string VersionInformationUserDefaultsKey { get { return "Have Seen Version Information For " + AppController.VERSION; } }

		public static bool ShouldShowVersionInformation {
			get {
				return !NSUserDefaults.StandardUserDefaults.BoolForKey (VersionInformationUserDefaultsKey);
			}
			private set {
				NSUserDefaults.StandardUserDefaults.SetBool (!value, VersionInformationUserDefaultsKey);
			}
		}

		partial void closeWindow (NSObject sender)
		{
			ShouldShowVersionInformation = this.showOnNextExecution.State == NSCellStateValue.On;
			Dispose ();
		}
	}
}

