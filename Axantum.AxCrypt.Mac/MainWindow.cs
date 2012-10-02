
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt.Mac
{
	public partial class MainWindow : MonoMac.AppKit.NSWindow
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public MainWindow (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindow (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}
		
		#endregion

		partial void urlClicked (NSObject sender)
		{
			System.Diagnostics.Process.Start("http://www.axantum.com");
		}
		
		void InvokeWithProgress (Action<ProgressContext> action, NSProgressIndicator indicator)
		{
			ProgressContext progress = new ProgressContext();
			progress.Progressing += (sender, eventArgs) => {
				if (eventArgs.Percent < 100) {
					indicator.StartAnimation(this);
				}
				else {
					indicator.StopAnimation(this);
				}
			};
			action(progress);
		}
		
		partial void encryptClicked (NSObject sender)
		{
			InvokeWithProgress(AppController.EncryptFile, encryptingIndicator);
		}
		
		partial void viewClicked (NSObject sender)
		{
			InvokeWithProgress(AppController.DecryptAndOpenFile, openingIndicator);
		}
		
		partial void decryptClicked (NSObject sender)
		{
			InvokeWithProgress(AppController.DecryptFile, decryptingIndicator);
		}
		
		partial void aboutClicked (NSObject sender)
		{
			AppController.About(sender);
		}
	}
}

