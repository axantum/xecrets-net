using MonoTouch.UIKit;
using System;
using MonoTouch.Foundation;

namespace Axantum.AxCrypt.iOS
{
	public class FilePresenter : UIDocumentInteractionControllerDelegate
	{
		UIDocumentInteractionController documentInteractionController;
		UIViewController owner;
		bool userInteractive = true;

		public event EventHandler ReadyToPresent = delegate {};
		public event EventHandler Done = delegate {};

		public FilePresenter ()
		{
			this.documentInteractionController = new UIDocumentInteractionController ();
			this.documentInteractionController.Delegate = this;
		}

		public override UIViewController ViewControllerForPreview (UIDocumentInteractionController controller)
		{
			return owner;
		}

		public override void WillBeginPreview (UIDocumentInteractionController controller)
		{
			ReadyToPresent (controller, EventArgs.Empty);
		}

		public override void DidEndPreview (UIDocumentInteractionController controller)
		{
			if (userInteractive) {
				Done (controller, EventArgs.Empty);
			}
		}

		public override UIView ViewForPreview (UIDocumentInteractionController controller)
		{
			return owner.View;
		}

		public override System.Drawing.RectangleF RectangleForPreview (UIDocumentInteractionController controller)
		{
			return owner.View.Frame;
		}

		public void Present(string file, UIViewController owner) {
			this.owner = owner;

			NSUrl url = NSUrl.FromFilename (file);
			this.documentInteractionController.Url = url;
			this.documentInteractionController.PresentPreview(true);
		}

		public void Dismiss() {
			this.userInteractive = false;
			this.documentInteractionController.DismissPreview (false);
		}
	}
}

