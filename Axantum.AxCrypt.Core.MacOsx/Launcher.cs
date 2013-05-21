using System;
using Axantum.AxCrypt.Core.Runtime;
using System.Diagnostics;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.MacOsx
{
	public class Launcher : ILauncher
	{
		#region ILauncher implementation
		public event EventHandler Exited;
		public bool HasExited {
			get {
				return this.state == TERMINATED;
			}
		}
		public bool WasStarted {
			get {
				return this.state >= LAUNCHED;
			}
		}
		public string Path {
			get {
				return this.encryptedSourceFile;
			}
		}
		#endregion
		#region IDisposable implementation
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}
			if (process == null)
			{
				return;
			}
			process.Dispose();
			process = null;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		const int
			ACTIVATED = 0 ,
			INACTIVATED= 1 ,
			LAUNCHED= 2 ,
			TERMINATED= 3;

		string encryptedSourceFile;
		string decryptedTargetFile;
		int state;
		Process process;
		int processId;
		string threadLock = "Axantum.AxCrypt.Core.MacOsx.Launcher";

		public Launcher (string filePath)
		{
			new NSObject ().InvokeOnMainThread ((NSAction) delegate {
				NSNotificationCenter.DefaultCenter.AddObserver ("decrypted file", not => {
					var sourceFile = not.UserInfo["source file"].ToString();
					var targetFile = not.UserInfo["target file"].ToString();

					if (targetFile == this.decryptedTargetFile) {
						this.encryptedSourceFile = sourceFile;
					}
				});

				NSApplication.Notifications.ObserveDidResignActive ((sender, args) => {
					lock(threadLock) {
						if (this.state == ACTIVATED && this.decryptedTargetFile != null)
							this.state = INACTIVATED;
					}
				});

				NSWorkspace.Notifications.ObserveDidLaunchApplication ((sender, args) => {
					lock(threadLock) {
						if (this.state == INACTIVATED) {
							this.state = LAUNCHED;
							this.processId = args.Application.ProcessIdentifier;
						}
					}
				});

				NSWorkspace.Notifications.ObserveDidTerminateApplication ((sender, args) => {
					lock(threadLock) {
						if (this.state == LAUNCHED && this.processId == args.Application.ProcessIdentifier) {
							this.state = TERMINATED;
							FireExited();
						}
					}
				});
			});

			lock(threadLock) {
				this.decryptedTargetFile = filePath;
				this.process = Process.Start (this.decryptedTargetFile);
				this.state = ACTIVATED;
			}
		}

		private void FireExited() {
			if (Exited == null || this.process == null)
				return;

			process.Dispose ();
			process = null;

			Exited (this, EventArgs.Empty);
			Exited = null;
		}
	}
}

