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
				return this.state == TerminatedAssociatedApplication;
			}
		}
		public bool WasStarted {
			get {
				return this.state >= LaunchedAssociatedApplication;
			}
		}
		public string Path {
			get {
				return this.decryptedTargetFile;
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
			Unknown = 0 ,
			DecryptedTargetFile= 1 ,
			LaunchedAssociatedApplication= 2 ,
			TerminatedAssociatedApplication= 3;

		string decryptedTargetFile;
		int state;
		Process process;
		int processId;
		string threadLock = "Axantum.AxCrypt.Core.MacOsx.Launcher";

		public const string FileDecryptedNotification = "decrypted file";
		public const string TargetFileUserInfoKey = "target file";

		public Launcher (string filePath)
		{
			new NSObject ().InvokeOnMainThread ((NSAction) delegate {
				NSNotificationCenter.DefaultCenter.AddObserver (FileDecryptedNotification, not => {
					var targetFile = not.UserInfo[TargetFileUserInfoKey].ToString();

					if (targetFile == this.decryptedTargetFile) {
						state = DecryptedTargetFile;
					}
				});

				NSWorkspace.Notifications.ObserveDidLaunchApplication ((sender, args) => {
					lock(threadLock) {
						if (this.state == DecryptedTargetFile) {
							this.state = LaunchedAssociatedApplication;
							this.processId = args.Application.ProcessIdentifier;
						}
					}
				});

				NSWorkspace.Notifications.ObserveDidTerminateApplication ((sender, args) => {
					lock(threadLock) {
						if (this.state == LaunchedAssociatedApplication && this.processId == args.Application.ProcessIdentifier) {
							this.state = TerminatedAssociatedApplication;
							FireExited();
						}
					}
				});
			});

			lock(threadLock) {
				this.decryptedTargetFile = filePath;
				this.process = Process.Start (this.decryptedTargetFile);
				this.state = Unknown;
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

