using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core;
using System.IO;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.Runtime;
using System.Diagnostics;
using Axantum.AxCrypt.Mono;
using Axantum.AxCrypt.Mac.Windows;

namespace Axantum.AxCrypt.Mac
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		NSWindowController mainWindowController;

		public AppDelegate ()
		{
			OS.Current = new RuntimeEnvironment();

			UpdateCheck updatecheck = new UpdateCheck(UpdateCheck.VersionUnknown);
			Uri restApiUri = new Uri("https://www.axantum.com/Xecrets/RestApi.ashx/axcrypt2version/mac");
			Uri versionUri = new Uri("http://www.axantum.com/");
			string currentVersion = UpdateCheck.VersionUnknown.ToString();

			updatecheck.VersionUpdate += (sender, versionArguments) => {
				if (versionArguments.VersionUpdateStatus == VersionUpdateStatus.NewerVersionIsAvailable) {
					int response = NSAlert.WithMessage("New version available!", "Update now", "Update later", null,
					                    "A new version of Axantum AxCrypt for Mac is available! " +
					                    "Would you like to download and install it now?")
						.RunModal();

					if (response == 1) {
						Process.Start(versionArguments.UpdateWebpageUrl.AbsoluteUri);
						NSApplication.SharedApplication.Terminate(this);
					}
				}
			};

			updatecheck.CheckInBackground(DateTime.UtcNow, currentVersion, restApiUri, versionUri);
		}

		public override void FinishedLaunching (NSObject notification)
		{
			// You can put any code here after your app launched.
			mainWindowController = new MainWindowController (); 
			mainWindowController.ShowWindow (this);

			AppController.Initialize ();
		}

		partial void about (NSObject sender)
		{
			AppController.About(sender);
		}

		partial void view (NSObject sender)
		{
			AppController.DecryptAndOpenFile(null, new ProgressContext(), AppController.OperationFailureHandler);
		}

		partial void onlineHelp (NSObject sender)
		{
			AppController.OnlineHelp();
		}

		partial void encrypt (NSObject sender)
		{
			AppController.EncryptFile(new ProgressContext(), AppController.OperationFailureHandler);
		}

		partial void decrypt (NSObject sender)
		{
			AppController.DecryptAndOpenFile(null, new ProgressContext(), AppController.OperationFailureHandler);
		}

		/// <summary>
		/// Non-interactive. Double-click while not running.
		/// </summary>
		/// <returns><c>true</c>, if file was opened, <c>false</c> otherwise.</returns>
		/// <param name="sender">Sender.</param>
		/// <param name="filename">Filename.</param>
		public override bool OpenFile (NSApplication sender, string filename)
		{
			if (!filename.EndsWith(".axx"))
				return false;

			NSAlert.WithMessage ("opefile", "OK", null, null, "You like " + filename).RunModal ();

			//AppController.DecryptAndOpenFile (filename, new ProgressContext (), AppController.OperationFailureHandler);

			return true;
		}

		/// <summary>
		/// Interactive. Double-click while running.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="filenames">Filenames.</param>
		public override void OpenFiles (NSApplication sender, string[] filenames)
		{
			NSAlert.WithMessage ("Open files", "OK", null, null, string.Join (",",filenames)).RunSheetModal (mainWindowController.Window);
		}
	}
}

