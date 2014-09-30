using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.IO;

namespace Axantum.AxCrypt.MonoTouch.Infrastructure
{
	public class EditableTableViewSource : DialogViewController.Source
	{
		public EditableTableViewSource (DialogViewController controller) : base(controller) {
		}

		public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return base.Root [indexPath.Section] [indexPath.Row] is ThemedFileElement ? UITableViewCellEditingStyle.Delete : UITableViewCellEditingStyle.None;
		}

		public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			if (editingStyle != UITableViewCellEditingStyle.Delete)
				return;

			Section section = Root [indexPath.Section];
			ThemedFileElement fileElement = section [indexPath.Row] as ThemedFileElement;
			if (fileElement == null)
				return;
			string fullPath = BasePath.Expand (fileElement.Caption, fileElement.PathId);
			File.Delete (fullPath);
			section.Remove (indexPath.Row);
		}
	}
}

