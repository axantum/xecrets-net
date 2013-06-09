using MonoTouch.UIKit;
using System;
using MonoTouch.Foundation;
using System.Drawing;

namespace Axantum.AxCrypt.iOS
{
	public class FilePresenter : UIDocumentInteractionControllerDelegate
	{
		UIDocumentInteractionController documentInteractionController;
		WeakReference<UIViewController> owner;

		public event EventHandler ReadyToPresent = delegate {};
		public event EventHandler Done = delegate {};

		public FilePresenter ()
		{
			this.documentInteractionController = new UIDocumentInteractionController ();
			this.documentInteractionController.Delegate = this;
		}

		public override UIViewController ViewControllerForPreview (UIDocumentInteractionController controller)
		{
			UIViewController value;
			owner.TryGetTarget(out value);
			return value;
		}

		public override void WillBeginPreview (UIDocumentInteractionController controller)
		{
			ReadyToPresent (controller, EventArgs.Empty);
		}

		public override void DidEndPreview (UIDocumentInteractionController controller)
		{
			Done (controller, EventArgs.Empty);
		}

		public override UIView ViewForPreview (UIDocumentInteractionController controller)
		{
			UIViewController vc = ViewControllerForPreview (controller);
			if (vc == null)
				return null;
			return vc.View;
		}

		public override System.Drawing.RectangleF RectangleForPreview (UIDocumentInteractionController controller)
		{
			UIView view = ViewForPreview (controller);
			if (view == null)
				return RectangleF.Empty;
			return view.Frame;
		}

		public void Present(string file, UIViewController owner) {
			this.owner = new WeakReference<UIViewController>(owner);

			NSUrl url = NSUrl.FromFilename (file);
			this.documentInteractionController.Url = url;
			this.documentInteractionController.PresentPreview(true);
		}

		public void Dismiss() {
			documentInteractionController.Delegate = null;
			documentInteractionController.DismissMenu (false);
			documentInteractionController.DismissPreview (false);
			documentInteractionController.Dispose ();
			documentInteractionController = null;
		}
	}
}

