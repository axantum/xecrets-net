using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using MonoTouch.UIKit;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.iOS.Infrastructure;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Crypto;
using System.Reflection;
using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Mono.Portable;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public string AppVersion {
			get {
				return NSBundle.MainBundle.ObjectForInfoDictionary ("CFBundleVersion").ToString ();
			}
		}

		public static AppDelegate Current {
			get;
			private set;
		}

		// class-level declarations
		MainViewController appViewController;
		FileListingViewController fileListingViewController;
		PassphraseController passphraseController;
		DecryptionViewController decryptionViewController;
		FilePresenter filePresenter;
		MFMailComposeViewController feedbackMailViewController;
		WebViewController webViewController;
        UINavigationController navigationController;
		bool isReceivingFile = false;

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
			AppDelegate.Current = this;

			Axantum.AxCrypt.Mono.RuntimeEnvironment.RegisterTypeFactories ();
			TypeMap.Register.New<AxCryptFile>(() => new AxCryptFile()); 
			TypeMap.Register.New<AxCryptFactory>(() => new AxCryptFactory());

			Assembly[] assemblies = new Assembly[]{ Assembly.GetExecutingAssembly () };  //AppDomain.CurrentDomain.GetAssemblies());

			TypeMap.Register.Singleton<CryptoFactory> (() => new CryptoFactory (assemblies));
			TypeMap.Register.Singleton<CryptoPolicy>(() => new CryptoPolicy(assemblies));
			TypeMap.Register.Singleton<ICryptoPolicy>(() => New<CryptoPolicy>().CreateDefault());

			TypeMap.Register.New<AxCryptHMACSHA1>(() => PortableFactory.AxCryptHMACSHA1());
			TypeMap.Register.New<HMACSHA512>(() => PortableFactory.HMACSHA512());
			TypeMap.Register.New<Aes>(() => new Axantum.AxCrypt.Mono.Cryptography.AesWrapper(new System.Security.Cryptography.AesCryptoServiceProvider()));
			TypeMap.Register.New<Sha1>(() => PortableFactory.SHA1Managed());
			TypeMap.Register.New<Sha256>(() => PortableFactory.SHA256Managed());
			TypeMap.Register.New<CryptoStream>(() => PortableFactory.CryptoStream());
			TypeMap.Register.New<RandomNumberGenerator>(() => PortableFactory.RandomNumberGenerator());

			appViewController = new MainViewController();
			appViewController.OnAboutButtonTapped += ShowAbout;
			appViewController.OnFaqButtonTapped += ShowFaq;
			appViewController.OnFeedbackButtonTapped += ShowFeedbackUi;
			appViewController.OnLocalFilesButtonTapped += ShowLocalFiles;
			appViewController.OnRecentFilesButtonTapped += ShowRecentFiles;
			appViewController.OnTroubleshootingButtonTapped += ShowTroubleshooting;

            navigationController = new UINavigationController(appViewController);
            Window = new UIWindow(UIScreen.MainScreen.Bounds);
			Window.RootViewController = navigationController;
			Window.MakeKeyAndVisible ();
			return true;
		}

		void ShowAbout() {
            PresentWebViewController ("https://www.axcrypt.net/");
		}

		void ShowFaq() {
            PresentWebViewController ("https://www.axcrypt.net/support/faq/");
		}

		void ShowTroubleshooting() {
            PresentWebViewController ("https://www.axcrypt.net/forums/forum/bugs-issues/");
		}

		void FreeWebViewController() {
			Free (ref webViewController);
		}

		void PresentWebViewController(string url) {
			FreeWebViewController ();
			webViewController = new WebViewController (url);
			webViewController.Done += FreeWebViewController;
			appViewController.PresentViewController (this.webViewController, true, null);
		}

		void ShowFeedbackUi() {
			if (!MFMailComposeViewController.CanSendMail) {
                PresentWebViewController ("https://www.axcrypt.net/");
				return;
			}

			FreeFeedbackViewController ();
			feedbackMailViewController = new MFMailComposeViewController ();
			feedbackMailViewController.SetToRecipients (new[] { "info@axcrypt.net" });
			feedbackMailViewController.SetSubject (String.Concat ("Feedback on AxCrypt 2 for iOS v", AppVersion));
			feedbackMailViewController.Finished += delegate {
				FreeFeedbackViewController ();
			}; 

			appViewController.PresentViewController (feedbackMailViewController, true, null);
		}

		void ShowLocalFiles() {
			ShowFileListing ("Transferred documents", BasePath.TransferredFilesId);
		}

		void ShowRecentFiles() {
			ShowFileListing("Received documents", BasePath.ReceivedFilesId);
		}

		UIPopoverController pop;
		void ShowFileListing(string caption, int basePathId) {
			FreeFileListingViewController ();
			fileListingViewController = new FileListingViewController (caption, basePathId);
			fileListingViewController.OpenFile += HandleOpenFile;
			fileListingViewController.Done += FreeFileListingViewController;

			if (Utilities.UserInterfaceIdiomIsPhone) {
                navigationController.PushViewController(fileListingViewController, true);
			} else {
				pop = new UIPopoverController (fileListingViewController);
				pop.PresentFromRect(
					appViewController.TableView.TableHeaderView.Frame,
					appViewController.TableView.TableHeaderView,
					UIPopoverArrowDirection.Up,
					true);

			}
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
			FreePassphraseViewController();
			FreeDecryptionViewController();
			if (filePresenter != null)
				filePresenter.Dismiss ();
		}
		
		// This method should be used to release shared resources and it should store the application state.
		// If your application supports background exection this method is called instead of WillTerminate
		// when the user quits.
		public override void DidEnterBackground (UIApplication application)
		{
		}

		public override void OnActivated (UIApplication application)
		{
			if (filePresenter != null && !isReceivingFile) {
				FreeFilePresenter ();
				isReceivingFile = false;
			}
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
			if (feedbackMailViewController == null)
				return;
			feedbackMailViewController.DismissViewController (false, (NSAction) delegate {
				Free(ref feedbackMailViewController);
			});
		}

		public void HandleOpenFile(string filePath) {
            navigationController.PopToViewController(appViewController, true);

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

				if (fileListingViewController != null) {
					appViewController.DismissViewController(true, (NSAction)delegate { 
						FreeFileListingViewController();
						filePresenter.Present (targetFileName, appViewController);
					});
					return;
				}
				filePresenter.Present (targetFileName, appViewController);

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
			isReceivingFile = true;

			return true;
		}
	}
}

