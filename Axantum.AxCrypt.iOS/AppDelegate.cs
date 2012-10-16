using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Axantum.AxCrypt.Core;
using System.IO;

namespace Axantum.AxCrypt.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		
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
			OS.Current = new Axantum.AxCrypt.Mono.RuntimeEnvironment();
			return true;
		}
		
		// This method is invoked when the application is about to move from active to inactive state.
		// OpenGL applications should use this method to pause.
		public override void OnResignActivation (UIApplication application)
		{
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
			Axantum_AxCrypt_iOSViewController.ReloadFileSystem();
		}

		/// This method is called when the application is about to terminate. Save data, if needed. 
		public override void WillTerminate (UIApplication application)
		{
		}

		public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			string targetPath = Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), Path.GetFileName(url.Path));
			System.IO.File.Move(url.Path, targetPath);
			Axantum_AxCrypt_iOSViewController.ReloadFileSystem();

			AppViewController.OpenFile(targetPath);

			return true;
		}
	}
}

