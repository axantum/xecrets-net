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
		public EditableTableViewSource (DialogViewController controller) : base(controller) {}

		public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			if (editingStyle == UITableViewCellEditingStyle.Delete) {
				string fileName = Root[indexPath.Section][indexPath.Row].Caption;
				string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				File.Delete(Path.Combine(dir, fileName) + OS.Current.AxCryptExtension);
				Root[indexPath.Section].Remove(indexPath.Row);
			}
		}
	}
}

