using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.iOS.Infrastructure;
using MonoTouch.Foundation;

namespace Axantum.AxCrypt.iOS
{
	public partial class FileListingViewController : DialogViewController
	{
		public event Action<string> OpenFile = delegate {};
		public event Action Done = delegate {};

		Section fileSection;
		int basePathId;

		public FileListingViewController (string title, int pathId) : base (UITableViewStyle.Plain, new RootElement(title), false)
		{
			basePathId = pathId;
			fileSection = new Section ();
            fileSection.AddAll(ReadFileSystem());
            Root.Add(fileSection);

			if (Utilities.UserInterfaceIdiomIsPhone) {
				ModalPresentationStyle = UIModalPresentationStyle.CurrentContext;
				ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
			} else {
				ModalPresentationStyle = UIModalPresentationStyle.CurrentContext;
				ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;
			}
		}

		public override void Selected (NSIndexPath indexPath)
		{
			base.Selected (indexPath);
			ThemedFileElement element = (ThemedFileElement)fileSection [indexPath.Row];
			string targetPath = BasePath.Expand (element.Caption, element.PathId);
			OpenFile (targetPath);
		}

		IEnumerable<ThemedFileElement> ReadFileSystem ()
		{
			string basePath = BasePath.GetBasePath (basePathId);
			Directory.CreateDirectory (basePath);

			return Directory.EnumerateFiles (basePath)
				.Select (file => new { file, accessTime = File.GetLastAccessTime(file) })
					.OrderByDescending (projection => projection.accessTime)
					.Select(projection => new ThemedFileElement(projection.file, projection.accessTime, basePathId));
		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			Theme.Configure (Root.Caption, this);
			TableView.Source = new EditableTableViewSource (this);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
            NavigationItem.SetHidesBackButton(false, false);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			Done ();
		}

		public IEnumerable<Element> GetElements() {
			foreach (ThemedFileElement element in ReadFileSystem ()) {
				element.Tapped += () => OpenFile(BasePath.Expand(element.Caption, basePathId));
				yield return element;
			}
		}
	}
}
