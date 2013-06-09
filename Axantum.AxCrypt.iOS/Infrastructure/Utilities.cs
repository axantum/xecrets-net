using MonoTouch.UIKit;

namespace Axantum.AxCrypt.iOS.Infrastructure
{
	public class Utilities
	{
		public static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public static bool iPhone5OrPad {
			get { return UIScreen.MainScreen.Bounds.Size.Height >= 568; }
		}
	}
}

