using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Axantum.AxCrypt.MonoTouch.Infrastructure;
using System;
using BigTed;

namespace Axantum.AxCrypt.MonoTouch
{
	public class WebViewController : UIViewController
	{
		public event Action Done = delegate {};

		NSUrlRequest request;

		public WebViewController (string url)
		{
			this.request = NSUrlRequest.FromUrl(NSUrl.FromString(url));
			ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
			ModalTransitionStyle = UIModalTransitionStyle.PartialCurl;
		}

		new UIWebView View {
			get {
				return (UIWebView)base.View;
			}
		}

		public override void ViewDidLoad ()
		{
			base.View = new UIWebView ();
			this.View.LoadStarted += delegate {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
			};
			this.View.LoadFinished += delegate {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
				BTProgressHUD.Dismiss();
			};
			this.View.LoadError += delegate {
				BTProgressHUD.ShowErrorWithStatus("An error occurred loading the web page. Please ensure you're connected to the internet and try again later.", 3000);
			};
			base.ViewDidLoad ();
			Theme.Configure (View);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			BTProgressHUD.Show("Loading ...");
			View.LoadRequest (this.request);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			Done ();
		}
	}
}

