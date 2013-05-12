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
		public const string APP_NAME = "AxCrypt for Mac";
		public const string VERSION = "2.0.1.0";

		private static AesKey lastUsedKey;

		public static string FullApplicationName {
			get {
				return String.Concat(APP_NAME, ", version ", VERSION);
			}
		}

		public AppController ()
		{
		}

		public static void Initialize()
		{
			const int
			ACTIVATED = 0 ,
			INACTIVATED= 1 ,
			LAUNCHED= 2 ,
			TERMINATED= 3;

			int state = ACTIVATED;
			string encryptedSourceFile = null, decryptedTargetFile = null;

			NSNotificationCenter.DefaultCenter.AddObserver ("decrypted file", not => {
				encryptedSourceFile = not.UserInfo["source file"].ToString();
				decryptedTargetFile = not.UserInfo["target file"].ToString();
			});
			
			NSApplication.Notifications.ObserveDidBecomeActive ((sender, args) => {
				if (state == TERMINATED) {
					// Re-encrypt file.
					if (encryptedSourceFile != null && decryptedTargetFile != null) {

						AxCryptFile.EncryptFileWithBackupAndWipe(
							decryptedTargetFile, 
							encryptedSourceFile, 
							lastUsedKey, 
							new ProgressContext());
					}
				}

				state = ACTIVATED;
				encryptedSourceFile = null;
				decryptedTargetFile = null;
			});

			NSApplication.Notifications.ObserveDidResignActive ((sender, args) => {
				state = INACTIVATED;
			});

			NSWorkspace.Notifications.ObserveDidActivateApplication ((sender, args) => {
				if (state == INACTIVATED)
					state = LAUNCHED;
			});

			NSWorkspace.Notifications.ObserveDidLaunchApplication ((sender, args) => {
				if (state == INACTIVATED)
					state = LAUNCHED;
			});

			NSWorkspace.Notifications.ObserveDidTerminateApplication ((sender, args) => {
				if (state == LAUNCHED)
					state = TERMINATED;
			});

			NSWorkspace.Notifications.ObserveDidDeactivateApplication ((sender, args) => {
				if (state == LAUNCHED)
					state = TERMINATED;
			});
		}

		public static void OperationFailureHandler (string message, ProgressContext context)
		{
			NSAlert alert = NSAlert.WithMessage(message, "OK", null, null, "Check your password and try again");
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
				Passphrase generatedPassphrase = passwordController.Passphrase;
				panel.Close();

				ThreadPool.QueueUserWorkItem(delegate { 
					using(new NSAutoreleasePool()) {
						fileSelected(OS.Current.FileInfo(filePath), generatedPassphrase); 
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
			GetSourceFile((encryptedDocument, passphrase) => {
				string tempPath = Path.GetTempPath();
				string decryptedFileName;
				lastUsedKey = passphrase.DerivedPassphrase;

				if (!TryDecrypt(encryptedDocument, tempPath, lastUsedKey, progress, out decryptedFileName)) {
					failure("Could not open file", progress);
					return;
				}

				string fullPathToDecryptedFile = Path.Combine(tempPath, decryptedFileName);
				IRuntimeFileInfo decryptedFile = OS.Current.FileInfo(fullPathToDecryptedFile);

				NSDictionary userInfo = new NSDictionary(
					"source file", encryptedDocument.FullName,
					"target file", decryptedFile.FullName);
				NSNotification notification = NSNotification.FromName("decrypted file", new NSObject(), userInfo);
				NSNotificationCenter.DefaultCenter.PostNotification(notification);

				using(ILauncher launcher = OS.Current.Launch(fullPathToDecryptedFile));
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

