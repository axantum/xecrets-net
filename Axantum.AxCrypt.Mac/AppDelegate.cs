using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.UI;
using System.Diagnostics;

namespace Axantum.AxCrypt.Mac
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		NSWindowController mainWindowController;

		public AppDelegate ()
		{
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
		}
    }
}

