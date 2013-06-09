using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Dialog;

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

		static void ConfigureTableView (this UITableView view, DialogViewController owner)
		{
			view.Bounces = false;
			view.ScrollEnabled = false;
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
			const float verticalMargin = 20f;
			const float horizontalMargin = 25f;

			float viewWidth = view.Bounds.Width;

			view.AddSubview (new UIImageView (HeaderImage) {
				Frame = new RectangleF (horizontalPadding, verticalPadding, viewWidth - horizontalPadding * 2, HeaderImageHeight + verticalMargin),
				ContentMode = UIViewContentMode.TopLeft
			});

			view.AddSubview (new UILabel {
				Frame = new RectangleF (HeaderImageWidth + horizontalMargin, verticalPadding, viewWidth - (HeaderImageWidth + horizontalPadding + horizontalMargin), HeaderImageHeight),
				Font = UIFont.SystemFontOfSize (title.Length > 7 ? 28 : 36),
				Text = title.Replace(' ', '\n'),
				TextColor = UIColor.DarkTextColor,
				ShadowColor = HighlightColor,
				ShadowOffset = new SizeF (1, 1),
				BackgroundColor = UIColor.Clear,
				Lines = title.Contains(" ") ? 2 : 1
			});
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

			cell.SelectedBackgroundView = new UIView (cell.Frame) { 
				BackgroundColor = HighlightColor 
			};
			Configure (cell.SelectedBackgroundView);
		}
	}
}

