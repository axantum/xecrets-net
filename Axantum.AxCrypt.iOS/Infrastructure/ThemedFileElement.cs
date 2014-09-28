using System;
using MonoTouch.Dialog;
using System.IO;
using System.Text;
using MonoTouch.UIKit;

namespace Axantum.AxCrypt.MonoTouch.Infrastructure
{
	public class ThemedFileElement : StyledStringElement
	{
		public int PathId {
			get;
			private set;
		}

		public ThemedFileElement (string fileName, DateTime lastAccessTime, int pathId) : base(
			caption: Path.GetFileNameWithoutExtension(fileName),
			value: String.Concat("Last opened ", FormatDateTime(lastAccessTime)),
			style: Utilities.UserInterfaceIdiomIsPhone ? UITableViewCellStyle.Subtitle : UITableViewCellStyle.Value1)
		{
			PathId = pathId;
		}

		static string FormatDateTime(DateTime value) {
			DateTime now = DateTime.Now;
			string nowDateString = now.ToShortDateString ();
			string yesterdayString = now.AddDays (-1).ToShortDateString ();
			string valueDateString = value.ToShortDateString ();
			StringBuilder format = new StringBuilder ();

			if (nowDateString == valueDateString) {
				format.Append ("today at");
			} else if (yesterdayString == valueDateString) {
				format.Append ("yesterday at");
			} else {
				format.Append (nowDateString);
			}

			format.Append (" ").Append (now.ToShortTimeString());
			return format.ToString ();
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = base.GetCell (tv);
			Theme.Configure (cell);
			return cell;
		}
	}
}

