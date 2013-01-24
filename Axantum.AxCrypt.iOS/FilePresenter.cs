using MonoTouch.UIKit;
using System;
using MonoTouch.Foundation;

namespace Axantum.AxCrypt.iOS
{
	public class FilePresenter
	{
		public static void Present(string file, UIViewController onto, EventHandler readyToPresent, EventHandler done) {
			UIDocumentInteractionController documentInteractionController = UIDocumentInteractionController.FromUrl (NSUrl.FromFilename (file));
			documentInteractionController.ViewControllerForPreview = delegate {
				return onto;
			};
			documentInteractionController.ViewForPreview = delegate {
				return onto.View;
			};
			documentInteractionController.RectangleForPreview = delegate {
				return onto.View.Bounds;
			};
			documentInteractionController.WillBeginPreview += readyToPresent;
			if (done != null)
				documentInteractionController.DidEndPreview += done;
			documentInteractionController.PresentPreview(true);
		}
	}
}

