using System;
using MonoTouch.UIKit;
using Axantum.AxCrypt.Core.Crypto;
using System.IO;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.Runtime;
using MonoTouch.Foundation;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core;
using BigTed;

namespace Axantum.AxCrypt.iOS
{
	public class DecryptionViewController : UIViewController
	{
		public event Action<string> Succeeded = delegate {};
		public event Action Failed = delegate {};

		ProgressContext context;
		ThreadWorker worker;
		IRuntimeFileInfo sourceFile;
		AesKey key;
		string targetFilePath;

		public DecryptionViewController (string sourceFilePath)
		{
			this.context = new ProgressContext ();
			//			context.Progressing += (sender, e) => {
			//				SetProgress(e.Percent, "Decrypting ...");
			//			};
			this.sourceFile = OS.Current.FileInfo (sourceFilePath);
		}

		void CreateWorker() {
			this.worker = new ThreadWorker (this.context);
			//worker.Prepare += delegate { SetProgress(0, "Unlocking ..."); };
			worker.Work += Work;
			worker.Completed += WorkerCompleted;
		}

		void Work(object sender, ThreadWorkerEventArgs args) {
			using (NSAutoreleasePool pool = new NSAutoreleasePool()) {
				string targetDirectory = Path.GetTempPath();
				string extractedFileName = AxCryptFile.Decrypt (
					this.sourceFile, 
					targetDirectory, 
					this.key, 
					AxCryptOptions.None, 
					this.context);

				if (extractedFileName == null) {
					args.Result = FileOperationStatus.Canceled;
					return;
				}

				this.targetFilePath = Path.Combine(targetDirectory,	extractedFileName);
				args.Result = FileOperationStatus.Success;
			}
		}

		void WorkerCompleted(object sender, ThreadWorkerEventArgs args) {
			BTProgressHUD.Dismiss ();
			if (args.Result == FileOperationStatus.Canceled) {
				Failed();
				return;
			}

			Succeeded(this.targetFilePath);
		}

		public void Decrypt(Passphrase passphrase) {
			BTProgressHUD.Show ("Opening ...", maskType: BTProgressHUD.MaskType.Gradient);
			CreateWorker ();
			this.key = passphrase.DerivedPassphrase;
			worker.Run();
		}
	}
}

