
using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Drawing;
using MonoTouch.CoreAnimation;

namespace Axantum.AxCrypt.iOS
{
	public partial class AppViewController : DialogViewController
	{
		protected static UIColor highlightColor = UIColor.FromRGB(96, 170, 13);
		protected UIImage headerImage = UIImage.FromFile("Images/logo.png");

		public AppViewController (IntPtr handle) : base (handle)
		{
			Initialize();
		}
		
		public AppViewController () : base(UITableViewStyle.Grouped, null)
		{
			Initialize();
		}
		
		void Initialize ()
		{
			ConfigureTableView();
			ConfigureViewLayer();
			CreateHeader ("AxCrypt");
		}

		protected void ConfigureTableView ()
		{
			TableView.Bounces = false;
			TableView.ScrollEnabled = false;
			TableView.BackgroundColor = UIColor.Black;
			TableView.TableHeaderView = new UIView(new RectangleF(0, 0, View.Bounds.Width, 100)) { BackgroundColor = UIColor.Clear };
		}

		public static void ConfigureViewLayer (CALayer layer)
		{
			layer.CornerRadius = 20;
			layer.BorderColor = highlightColor.CGColor;
			layer.BorderWidth = 2.75f;
		}

		protected void ConfigureViewLayer ()
		{
			ConfigureViewLayer (View.Layer);
		}

		protected void CreateHeader (string title)
		{
			const float horizontalPadding = 15f;
			const float verticalPadding = 10f;
			const float verticalMargin = 20f;
			const float horizontalMargin = 25f;

			View.AddSubview (new UIImageView (headerImage) {
				Frame = new RectangleF (horizontalPadding, verticalPadding, View.Bounds.Width - horizontalPadding * 2, headerImage.Size.Height + verticalMargin),
				ContentMode = UIViewContentMode.TopLeft
			});
			View.AddSubview (new UILabel {
				Frame = new RectangleF (headerImage.Size.Width + horizontalMargin, verticalPadding, View.Bounds.Width - (headerImage.Size.Width + horizontalPadding + horizontalMargin), headerImage.Size.Height),
				Font = UIFont.SystemFontOfSize (36),
				Text = title,
				TextColor = UIColor.DarkTextColor,
				ShadowColor = highlightColor,
				ShadowOffset = new SizeF (1, 1),
				BackgroundColor = UIColor.Clear
			});
		}

		public static void OpenFile(string targetPath)
		{
			PassphraseViewController passphrase = new PassphraseViewController(targetPath);
			passphrase.FileDecrypted += decryptedFilePath => {
				FilePresenter presenter = new FilePresenter(decryptedFilePath);
				presenter.Done += delegate {
					UIView.Transition(presenter.View, Axantum_AxCrypt_iOSViewController.LoadedView, 1, UIViewAnimationOptions.TransitionCurlUp, delegate {
						presenter.View.RemoveFromSuperview();
						presenter.Dispose();
					});
				};
				UIView.Transition(passphrase.View, presenter.View, 1, UIViewAnimationOptions.TransitionCurlDown, delegate {
					passphrase.View.RemoveFromSuperview();
					passphrase.Dispose();
				});
			};
			passphrase.Cancelled += delegate {
				UIView.Transition(passphrase.View, Axantum_AxCrypt_iOSViewController.LoadedView, 1, UIViewAnimationOptions.TransitionCurlUp, delegate {
					passphrase.View.RemoveFromSuperview();
					passphrase.Dispose();
				});
			};
			
			
			UIView.Transition(Axantum_AxCrypt_iOSViewController.LoadedView, passphrase.View, 1f, UIViewAnimationOptions.TransitionNone, null);

		}
	}
}
