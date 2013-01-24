
using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Drawing;
using System.ComponentModel;
using System.IO;

namespace Axantum.AxCrypt.iOS
{
	class UIAlertViewOkCancelDelegate : UIAlertViewDelegate
	{
		Action okAction, cancelAction;
		Action<string> okActionOfString;

		public UIAlertViewOkCancelDelegate (Action okAction, Action cancelAction)
		{
			this.okAction = okAction;
			this.cancelAction = cancelAction;
		}

		public UIAlertViewOkCancelDelegate (Action<string> okAction, Action cancelAction)
		{
			okActionOfString = okAction;
			this.cancelAction = cancelAction;
		}

		public override void Dismissed (UIAlertView alertView, int buttonIndex)
		{
			if (buttonIndex == alertView.CancelButtonIndex)
				cancelAction();
			else if (alertView.AlertViewStyle == UIAlertViewStyle.SecureTextInput)
				okActionOfString(alertView.GetTextField(0).Text);
			else
				okAction();
		}
	}
}
