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
		OpenFileFromFinderController openFile;

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
			AppController.Initialize ();

			if (openFile == null) {
				mainWindowController = new MainWindowController (); 
				mainWindowController.ShowWindow (this);
			}
		}

		partial void about (NSObject sender)
		{
			AppController.About(sender);
		}

		partial void view (NSObject sender)
		{
			AppController.DecryptAndOpenFile(new ProgressContext(), AppController.OperationFailureHandler);
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
			AppController.DecryptAndOpenFile(new ProgressContext(), AppController.OperationFailureHandler);
		}

		public override void OpenFiles (NSApplication sender, string[] filenames)
		{
			openFile = new OpenFileFromFinderController ();
			openFile.UserChoseOpen += (string passphrase) => {
				AppController.DecryptAndOpenFile(
					OS.Current.FileInfo(filenames[0]),
					new Passphrase(passphrase),
					new ProgressContext(),
					AppController.OperationFailureHandler);
				ReleaseOpenFileController();
			};
			openFile.UserChoseCancel += () => {
				ReleaseOpenFileController();
			};
			openFile.ShowWindow (sender);
		}

		void ReleaseOpenFileController ()
		{
			openFile.Close ();
			openFile.Dispose ();
			openFile = null;
		}
	}
}

