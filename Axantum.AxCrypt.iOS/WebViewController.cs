using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Axantum.AxCrypt.iOS.Infrastructure;
using System;

namespace Axantum.AxCrypt.iOS
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
			};
			base.ViewDidLoad ();
			Theme.Configure (View);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			View.LoadRequest (this.request);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			Done ();
		}
	}
}

