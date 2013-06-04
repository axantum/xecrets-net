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
		string filePath;

		public FileListingViewController (string title, string path) : base (UITableViewStyle.Plain, new RootElement(title), false)
		{
			filePath = path;
			fileSection = new Section ();
			Root.Add (fileSection);

			ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
			ModalTransitionStyle = UIModalTransitionStyle.PartialCurl;
		}

		public override void Selected (NSIndexPath indexPath)
		{
			base.Selected (indexPath);
			string caption = fileSection [indexPath.Row].Caption;
			string targetPath = Path.Combine(filePath, caption + OS.Current.AxCryptExtension);
			OpenFile (targetPath);
		}

		void ReloadFileSystem ()
		{
			var selection = Directory.EnumerateFiles(filePath)
				.Select (file => new { file, accessTime = File.GetLastAccessTime(file) })
					.OrderByDescending(projection => projection.accessTime)
					.Select(projection => new StyledStringElement(
						caption: Path.GetFileNameWithoutExtension(projection.file),
						value: projection.accessTime.ToString("F"),
						style: UITableViewCellStyle.Subtitle));
			Root [0].AddAll (selection);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			Theme.Configure (Root.Caption, this);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			ReloadFileSystem ();
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			Done ();
		}

	}
}
