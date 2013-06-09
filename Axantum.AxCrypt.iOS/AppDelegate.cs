using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using MonoTouch.UIKit;
using Axantum.AxCrypt.Core;

namespace Axantum.AxCrypt.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		const string AppVersion = "1.1.3";

		// class-level declarations
		MainViewController appViewController;
		FileListingViewController fileListingViewController;
		PassphraseController passphraseController;
		DecryptionViewController decryptionViewController;
		FilePresenter filePresenter;
		MFMailComposeViewController feedbackViewController;

		public override UIWindow Window {
			get;
			set;
		}

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			OS.Current = new Axantum.AxCrypt.MonoTouch.RuntimeEnvironment();
			appViewController = new MainViewController();
			appViewController.OnAboutButtonTapped += ShowAbout;
			appViewController.OnFaqButtonTapped += ShowFaq;
			appViewController.OnFeedbackButtonTapped += ShowFeedbackUi;
			appViewController.OnLocalFilesButtonTapped += ShowLocalFiles;
			appViewController.OnRecentFilesButtonTapped += ShowRecentFiles;
			appViewController.OnTroubleshootingButtonTapped += delegate {};

			Window = new UIWindow(UIScreen.MainScreen.Bounds);
			Window.RootViewController = appViewController;
			Window.MakeKeyAndVisible ();
			return true;
		}

		void ShowAbout() {
			new UIAlertView (
				"About AxCrypt for iOS",
				@"By installing this app, you've given your other apps super powers! 

Your other apps are now able to open .axx documents through AxCrypt.

Just look for the Send To / Action icon and then tap AxCrypt",
				null,
				"OK, I get it")
				.Show ();
		}

		void ShowFaq() {
			UIApplication.SharedApplication.OpenUrl (NSUrl.FromString("http://monodeveloper.org/axcrypt-ios-faq/"));
		}

		void ShowFeedbackUi() {
			if (!MFMailComposeViewController.CanSendMail) {
				UIApplication.SharedApplication.OpenUrl (NSUrl.FromString("http://monodeveloper.org/axcrypt-ios-feedback/"));
				return;
			}

			FreeFeedbackViewController ();
			feedbackViewController = new MFMailComposeViewController ();
			feedbackViewController.SetToRecipients (new[] { "sami.lamti+axcrypt-ios-feedback@tretton37.com" });
			feedbackViewController.SetSubject (String.Concat ("Feedback on AxCrypt for iOS v", AppVersion));
			feedbackViewController.Finished += delegate {
				FreeFeedbackViewController ();
			}; 

			appViewController.PresentViewController (feedbackViewController, true, null);
		}

		void ShowLocalFiles() {
			ShowFileListing ("Local files");
		}

		void ShowRecentFiles() {
			ShowFileListing("Recent files", "Recent");
		}

		void ShowFileListing(string caption, string pathSuffix = null) {
			FreeFileListingViewController ();
			string path = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			if (pathSuffix != null)
				path = Path.Combine (path, pathSuffix);
			fileListingViewController = new FileListingViewController (caption, path);
			fileListingViewController.OpenFile += HandleOpenFile;
			fileListingViewController.Done += FreeFileListingViewController;
			appViewController.PresentViewControllerAsync (fileListingViewController, true);
		}

		static void Free<T> (ref T viewController) where T: UIViewController
		{
			if (viewController == null)
				return;
			viewController.RemoveFromParentViewController ();
			viewController.Dispose ();
			viewController = null;
		}

		void FreeFileListingViewController() {
			if (fileListingViewController == null)
				return;
			fileListingViewController.OpenFile -= HandleOpenFile;
			fileListingViewController.Done -= FreeFileListingViewController;
			Free (ref fileListingViewController);
		}

		void FreePassphraseViewController() {
			if (passphraseController == null)
				return;
			passphraseController.Dispose();
			passphraseController = null;
		}

		void FreeDecryptionViewController() {
			Free (ref decryptionViewController);
		}

		void FreeFilePresenter() {
			if (filePresenter == null)
				return;
			filePresenter.Dispose ();
			filePresenter = null;
		}
		
		// This method is invoked when the application is about to move from active to inactive state.
		// OpenGL applications should use this method to pause.
		public override void OnResignActivation (UIApplication application)
		{
			if (filePresenter != null) {
				filePresenter.Dismiss();
				FreeFilePresenter ();
			}

			FreePassphraseViewController();
			FreeDecryptionViewController();

			Window.RootViewController = appViewController;
		}
		
		// This method should be used to release shared resources and it should store the application state.
		// If your application supports background exection this method is called instead of WillTerminate
		// when the user quits.
		public override void DidEnterBackground (UIApplication application)
		{
		}
		
		/// This method is called as part of the transiton from background to active state.
		public override void WillEnterForeground (UIApplication application)
		{
		}

		/// This method is called when the application is about to terminate. Save data, if needed. 
		public override void WillTerminate (UIApplication application)
		{
		}

		void FreeViewControllers() {
			if (passphraseController != null)
				FreePassphraseViewController ();
			if (decryptionViewController != null)
				FreeDecryptionViewController ();
		}

		void FreeFeedbackViewController() {
			if (feedbackViewController == null)
				return;
			feedbackViewController.DismissViewController (false, (NSAction) delegate {
				Free(ref feedbackViewController);
			});
		}

		void HandleOpenFile(string filePath) {
 			FreePassphraseViewController ();
			FreeDecryptionViewController ();
			FreeFilePresenter ();
			FreeFeedbackViewController ();

			passphraseController = new PassphraseController (filePath);
			decryptionViewController = new DecryptionViewController (filePath);
			filePresenter = new FilePresenter ();

			passphraseController.Done += decryptionViewController.Decrypt;
			passphraseController.Cancelled += FreeViewControllers;
			decryptionViewController.Succeeded += targetFileName => {
				FreeViewControllers();
				appViewController.DismissViewController(true, (NSAction)delegate { 
					FreeFileListingViewController();
					filePresenter.Present (targetFileName, appViewController);
				});

			};
			decryptionViewController.Failed += passphraseController.AskForPassword;
			filePresenter.Done += delegate {
				FreeViewControllers();
				FreeFilePresenter();
			};

			passphraseController.AskForPassword ();
		}

		public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			string targetPath = Path.Combine(
				Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), 
				"Recent",
				Path.GetFileName(url.Path));
			if (File.Exists (targetPath))
				File.Delete (targetPath);
			File.Move(url.Path, targetPath);

			HandleOpenFile(targetPath);

			return true;
		}
	}
}

