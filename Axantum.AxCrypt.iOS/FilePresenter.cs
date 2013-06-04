using MonoTouch.UIKit;
using System;
using MonoTouch.Foundation;

namespace Axantum.AxCrypt.iOS
{
	public class FilePresenter : UIDocumentInteractionControllerDelegate
	{
		UIDocumentInteractionController documentInteractionController;
		UIViewController owner;
		public event EventHandler ReadyToPresent = delegate {};
		public event EventHandler Done = delegate {};

		public FilePresenter ()
		{
			owner = new UIViewController ();
			owner.View = new UIView(UIScreen.MainScreen.ApplicationFrame);
		}

		public FilePresenter (UIViewController owner)
		{
			this.owner = owner;
			documentInteractionController = new UIDocumentInteractionController ();
			documentInteractionController.Delegate = this;
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
			Done (controller, EventArgs.Empty);
		}

		public override UIView ViewForPreview (UIDocumentInteractionController controller)
		{
			return owner.View;
		}

		public override System.Drawing.RectangleF RectangleForPreview (UIDocumentInteractionController controller)
		{
			return owner.View.Frame;
		}

		public void Present(string file) {
			NSUrl url = NSUrl.FromFilename (file);
			documentInteractionController.Url = url;
			documentInteractionController.PresentPreview(true);
		}
	}
}

