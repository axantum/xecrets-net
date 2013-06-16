using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Dialog;
using System;

namespace Axantum.AxCrypt.iOS.Infrastructure
{
	public static class Theme
	{
		public static UIColor HighlightColor = UIColor.FromRGB(96, 170, 13);
		public const float CornerRadius = 20f;
		public const float BorderWith = 2.75f;
		public const float VerticalPadding = 15f;

		const string HeaderImagePath = "Images/logo.png";
		static UIImage HeaderImage;
		static float HeaderImageWidth, HeaderImageHeight;
		static UIView headerView;

		static void ConfigureTableView (this UITableView view, DialogViewController owner)
		{
			view.Bounces = true;
			view.ScrollEnabled = true;

			view.TableHeaderView = new UIView (new RectangleF (0, 0, view.Bounds.Width, HeaderImage.Size.Height + 15f)) {
				BackgroundColor = UIColor.Clear
			};
		}

		public static void Configure (UIView view)
		{
			view.Layer.CornerRadius = CornerRadius;
			view.Layer.BorderColor = HighlightColor.CGColor;
			view.Layer.BorderWidth = BorderWith;
		}

		static void CreateHeader (string title, UIView view)
		{
			const float horizontalPadding = 15f;
			const float verticalPadding = 10f;

			float viewWidth = view.Bounds.Width;
			headerView = new UIView ();
			UIImageView logo = new UIImageView (HeaderImage) {
				ContentMode = Utilities.UserInterfaceIdiomIsPhone ? UIViewContentMode.TopLeft : UIViewContentMode.Center
			};

			UILabel logoText = new UILabel {
				Font = UIFont.SystemFontOfSize (title.Length > 7 ? 28 : 36),
				Text = Utilities.UserInterfaceIdiomIsPhone ? title.Replace(' ', '\n') : title,
				TextColor = UIColor.DarkTextColor,
				ShadowColor = HighlightColor,
				ShadowOffset = new SizeF (1, 1),
				BackgroundColor = UIColor.Clear,
				Lines = Utilities.UserInterfaceIdiomIsPhone && title.Contains(" ") ? 2 : 1
			};

			logo.SizeToFit ();
			logoText.SizeToFit ();
			float requiredWidth = logo.Bounds.Width + horizontalPadding + logoText.Bounds.Width;
			float requiredHeight = Math.Max (logo.Bounds.Height, logoText.Bounds.Height);
			headerView.Frame = new RectangleF ((viewWidth - requiredWidth) / 2, verticalPadding, requiredWidth, requiredHeight + verticalPadding);
			logo.Frame = new RectangleF (0, 0, logo.Bounds.Width, logo.Bounds.Height);
			logoText.Frame = new RectangleF (logo.Bounds.Width + horizontalPadding / 2, (logo.Bounds.Height - logoText.Bounds.Height) / 2, logoText.Bounds.Width, logoText.Bounds.Height);
			headerView.Add (logo);
			headerView.Add (logoText);

			view.Add (headerView);
		}

		public static void Configure (string text, DialogViewController viewController)
		{
			if (HeaderImage == null) {
				HeaderImage = UIImage.FromFile (HeaderImagePath);
				HeaderImageWidth = HeaderImage.Size.Width;
				HeaderImageHeight = HeaderImage.Size.Height;
			}
			Configure (viewController.View);
			ConfigureTableView (viewController.TableView, viewController);
			CreateHeader (text, viewController.View);
		}

		public static void Configure(UITableViewCell cell) {
			cell.TextLabel.TextColor = HighlightColor;
			cell.TextLabel.TextAlignment = UITextAlignment.Center;
			if (cell.DetailTextLabel != null) {
				cell.DetailTextLabel.Font = UIFont.SystemFontOfSize (UIFont.SmallSystemFontSize);
			}

			cell.SelectedBackgroundView = new UIView (cell.Frame) { 
				BackgroundColor = HighlightColor 
			};
			Configure (cell.SelectedBackgroundView);
		}
	}
}

