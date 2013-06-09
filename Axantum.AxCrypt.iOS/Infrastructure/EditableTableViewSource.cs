using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.IO;
using Axantum.AxCrypt.Core;

namespace Axantum.AxCrypt.iOS
{
	public class EditableTableViewSource : DialogViewController.Source
	{
		string basePath;

		public EditableTableViewSource (DialogViewController controller, string basePath) : base(controller) {
			this.basePath = basePath;
		}

		public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			if (editingStyle != UITableViewCellEditingStyle.Delete)
				return;

			Section section = Root [indexPath.Section];
			string fileName = section [indexPath.Row].Caption;
			string fullPath = Path.Combine (this.basePath, fileName) + OS.Current.AxCryptExtension;
			File.Delete (fullPath);
			Root [indexPath.Section].Remove (indexPath.Row);
		}
	}
}

