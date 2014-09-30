using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Axantum.AxCrypt.MonoTouch.Infrastructure
{
	public class ThemedStringElement : StringElement {

		public ThemedStringElement (string caption, Action action) : base(caption, new NSAction(action))
		{
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = base.GetCell (tv);

			Theme.Configure (cell);

			return cell;
		}
	}
}

