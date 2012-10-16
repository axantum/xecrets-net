using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using System.IO;

namespace Axantum.AxCrypt.iOS
{
	public class FilePresenter : UIViewController
	{
		public event EventHandler Done;

		private string openFile;

		public FilePresenter (string fileToPresent)
		{
			openFile = fileToPresent;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			RectangleF bounds = UIScreen.MainScreen.Bounds;
			const float navigationBarHeight = 44;
			
			UIWebView webView = new UIWebView(new RectangleF(0, navigationBarHeight, bounds.Width, bounds.Height));
			webView.ScalesPageToFit = true;
			
			NSUrlRequest request = NSUrlRequest.FromUrl(NSUrl.FromFilename(openFile));
			webView.LoadRequest(request);
			
			UINavigationBar navigationBar = new UINavigationBar(new RectangleF(0, 0, bounds.Width, navigationBarHeight)) { Translucent = true } ;
			navigationBar.SetItems(new UINavigationItem[] {
				new UINavigationItem(Path.GetFileName(openFile)) {
					LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done, Done)
				}
			}, true);
			
			View.AddSubview(webView);
			View.AddSubview(navigationBar);
		}
	}
}

