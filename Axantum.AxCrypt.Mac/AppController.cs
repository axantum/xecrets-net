using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Axantum.AxCrypt.Core.IO;
using System.IO;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Axantum.AxCrypt.Mono;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.Mac
{
	public class AppController
	{
		const string APP_NAME = "AxCrypt for Mac";
		const string VERSION = "2.0.1.0";

		public static string FullApplicationName {
			get {
				return String.Concat(APP_NAME, ", version ", VERSION);
			}
		}

		public AppController ()
		{
		}

		public static void OperationFailureHandler (string message, ProgressContext context)
		{
			NSAlert alert = NSAlert.WithMessage(message, "OK", null, null, context.DisplayText);
			alert.InvokeOnMainThread(() => alert.RunModal());
		}

		public static void OnlineHelp ()
		{
			Process.Start("http://www.axantum.com/AxCrypt/Default.html");
		}

		static IRuntimeFileInfo GetTargetFileName (string sourceFilePath, string encryptedFileName)
		{
			if (String.IsNullOrEmpty (encryptedFileName))
				encryptedFileName = DateTime.Now.ToString ("yyyyMMddHHmmss");

			if (!encryptedFileName.EndsWith(OS.Current.AxCryptExtension))
				encryptedFileName += OS.Current.AxCryptExtension;

			return OS.Current.FileInfo(Path.Combine(Path.GetDirectoryName(sourceFilePath), encryptedFileName));
		}

		public static void EncryptFile (ProgressContext progress, Action<string, ProgressContext> failure)
		{
			CreatePassphraseViewController passphraseController = new CreatePassphraseViewController {
				EncryptedFileName = DateTime.Now.ToString("yyyyMMddHHmmss")
			};

			NSOpenPanel open = new NSOpenPanel {
				AccessoryView = passphraseController.View,
				AllowsMultipleSelection = false,
				CanChooseDirectories = false,
				CanChooseFiles = true,
				CanSelectHiddenExtension = true,
				CollectionBehavior = NSWindowCollectionBehavior.Transient,
				ExtensionHidden = true,
				Message = "Please select the file you would like to encrypt",
				Prompt = "Encrypt file",
				Title = "Encrypt",
				TreatsFilePackagesAsDirectories = false,
			};
			
			open.Begin(result => {
				if (result == 0 || open.Urls.Length == 0) return;
				if (!open.Urls[0].IsFileUrl) return;
				string sourceFilePath = open.Urls[0].Path;
				open.Close();

				IRuntimeFileInfo sourceFile = OS.Current.FileInfo(sourceFilePath);
				Passphrase passphrase = passphraseController.VerifiedPassphrase;
				if (passphrase == null) return;

				IRuntimeFileInfo targetFile = GetTargetFileName(sourceFilePath, passphraseController.EncryptedFileName);

				ThreadPool.QueueUserWorkItem(delegate { 
					using(new NSAutoreleasePool()) {
						AxCryptFile.EncryptFileWithBackupAndWipe(sourceFile, targetFile, passphrase.DerivedPassphrase, progress);
					};
				});
			});
		}

		private static void GetSourceFile (Action<IRuntimeFileInfo, Passphrase> fileSelected)
		{
			NSOpenPanel panel = NSOpenPanel.OpenPanel;
			PasswordViewController passwordController = new PasswordViewController();
			panel.AccessoryView = passwordController.View;

			panel.Begin (result => {
				if (result == 0 || panel.Urls.Length == 0) return;
				if (!panel.Urls[0].IsFileUrl) return;
				string filePath = panel.Urls[0].Path;
				panel.Close();
				ThreadPool.QueueUserWorkItem(delegate { 
					using(new NSAutoreleasePool()) {
						fileSelected(OS.Current.FileInfo(filePath), passwordController.Passphrase); 
					};
				});
			});
		}

		static void GetTargetPath (Action<string> directorySelected)
		{
			NSOpenPanel panel = NSOpenPanel.OpenPanel;
			panel.CanChooseFiles = false;
			panel.CanChooseDirectories = true;

			panel.Begin(result => {
				if (result == 0 || panel.Urls.Length == 0) return;
				if (!panel.Urls[0].IsFileUrl) return;
				string filePath = panel.Urls[0].Path;
				panel.Close();

				directorySelected(filePath);
			});
		}

		static bool TryDecrypt (IRuntimeFileInfo file, string filePath, AesKey key, ProgressContext progress, out string encryptedFileName)
		{
			encryptedFileName = AxCryptFile.Decrypt(file, filePath, key, AxCryptOptions.EncryptWithCompression, progress);
			
			if (encryptedFileName == null) {
				return false;
			}
			return true;
		}

		public static void DecryptAndOpenFile (ProgressContext progress, Action<string, ProgressContext> failure)
		{
			GetSourceFile((file, passphrase) => {
				string filePath = Path.GetTempPath();
				string fileName;
				AesKey key = passphrase.DerivedPassphrase;

				if (!TryDecrypt(file, filePath, key, progress, out fileName)) {
					failure("Could not open file", progress);
					return;
				}

				IRuntimeFileInfo target = OS.Current.FileInfo(Path.Combine(filePath, fileName));

				ILauncher launcher = OS.Current.Launch(target.FullName);
				launcher.Exited += delegate {
					AxCryptFile.EncryptFileWithBackupAndWipe(target, file, key, progress);
					launcher.Dispose();
				};
			});
		}

		public static void DecryptFile(ProgressContext progress, Action<string, ProgressContext> failure) {
			GetSourceFile((file, passphrase) => {

				string targetDirectory = Path.GetDirectoryName(file.FullName);
				string fileName;

				if (!TryDecrypt(file, targetDirectory, passphrase.DerivedPassphrase, progress, out fileName)) {
					failure("Decryption failed", progress);
					return;
				}
			});
		}

		public static void About(object sender)
		{
			AboutWindowController controller = new AboutWindowController();
			controller.ShowWindow((NSObject)sender);
			controller.SetVersion(VERSION);
		}
	}
}

