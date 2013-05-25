using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using MonoTouch.CoreAnimation;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace Axantum.AxCrypt.iOS
{
	public partial class AppViewController : DialogViewController
	{
		public static UIColor HighlightColor = UIColor.FromRGB(96, 170, 13);
		public const float CornerRadius = 20f;
		public const float BorderWith = 2.75f;
		public const float VerticalPadding = 15f;

		private const string HeaderImagePath = "Images/logo.png";
		private UIImage headerImage;
		private Section fileSection;

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public AppViewController (IntPtr handle) : base (handle)
		{
			Initialize();
		}
		
		public AppViewController () : base(UITableViewStyle.Grouped, null)
		{
			Initialize();
		}

		public void ReloadFileSystem ()
		{
			string appFiles = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var selection = Directory.EnumerateFiles(appFiles)
				.Select (file => new { file, creationTime = File.GetCreationTime(file) })
					.OrderByDescending(projection => projection.creationTime)
					.Take(UserInterfaceIdiomIsPhone ? 3 : 20)
					.Select(projection => new StyledStringElement(
						Path.GetFileNameWithoutExtension(projection.file), 
						String.Concat(projection.creationTime.ToLongDateString(), " ", projection.creationTime.ToShortTimeString()),
						UserInterfaceIdiomIsPhone ? UITableViewCellStyle.Subtitle : UITableViewCellStyle.Value1));
			
			fileSection.Clear();
			foreach (StyledStringElement item in selection) {
				string targetPath = Path.Combine(appFiles, item.Caption + Axantum.AxCrypt.Core.OS.Current.AxCryptExtension);
				item.Tapped += () => OpenFile(targetPath);
				fileSection.Add(item);
			}
		}

		void Initialize ()
		{
			BeginInvokeOnMainThread(delegate {
				using(headerImage = UIImage.FromFile(HeaderImagePath)) {
					CreateFileListing();
					ConfigureTableView();
					ConfigureViewLayer();
					CreateHeader("AxCrypt");
					ReloadFileSystem();
				}
			});
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
				ShadowColor = HighlightColor,
				ShadowOffset = new SizeF (1, 1),
				BackgroundColor = UIColor.Clear
			});
		}

		void ConfigureTableView ()
		{
			TableView.Bounces = false;
			TableView.ScrollEnabled = false;
			TableView.TableHeaderView = new UIView(new RectangleF(0, 0, View.Bounds.Width, headerImage.Size.Height + 15f)) { BackgroundColor = UIColor.Clear };
			TableView.Source = new EditableTableViewSource(this);
		}

		void ConfigureViewLayer ()
		{
			View.Layer.CornerRadius = CornerRadius;
			View.Layer.BorderColor = HighlightColor.CGColor;
			View.Layer.BorderWidth = BorderWith;
		}

		void CreateFileListing() {
			Section introduction = new Section("Opening encrypted files", "By installing this app, you have given your other apps super powers! In your mail app, for example, you can now simply tap on .axx documents to open them with AxCrypt!");
			fileSection = new Section("Local files", "Local files can be transferred from iTunes") {
				#region Example data (be sure to comment out the call to ReloadFileSystem below)
				// iPhone examples
				//					new StyledStringElement("Medical records", "December 1st, 2012 12:03pm", UITableViewCellStyle.Subtitle),
				//					new StyledStringElement("Income tax returns", "November 5th, 2012 9:29am", UITableViewCellStyle.Subtitle),
				//					new StyledStringElement("Real estate contract", "November 3rd, 2012 3:30pm", UITableViewCellStyle.Subtitle),
				
				// iPad examples
				//					new StyledStringElement("Medical records", "December 1st, 2012 12:03pm", UITableViewCellStyle.Value1),
				//					new StyledStringElement("Income tax and National Insurance returns", "November 5th, 2012 9:29am", UITableViewCellStyle.Value1),
				//					new StyledStringElement("Real estate contract", "November 3rd, 2012 3:30pm", UITableViewCellStyle.Value1),
				#endregion
			};
			Root = new RootElement(String.Empty) {
				introduction,
				fileSection,
			};
		}

		void FadeViewController (UIViewController controller)
		{
			UIView.Animate (.5d, delegate {
				controller.View.Layer.Opacity = 0f;
			}, delegate {
				controller.RemoveFromParentViewController();
				controller.View.RemoveFromSuperview();
				controller.Dispose();
			});

		}

		public void OpenFile(string targetPath)
		{
			PassphraseViewController passphrase = new PassphraseViewController(targetPath);
			passphrase.Decrypting += delegate {
				TableView.UserInteractionEnabled = false;
			};
			passphrase.FileDecrypted += decryptedFilePath => {
				FilePresenter.Present(decryptedFilePath, this, delegate {
					FadeViewController(passphrase);
					TableView.UserInteractionEnabled = true;
				}, null);
			};
			passphrase.Cancelled += delegate {
				FadeViewController (passphrase);
				TableView.UserInteractionEnabled = true;
			};

			AddChildViewController(passphrase);
			Add (passphrase.View);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (fileSection != null)
				ReloadFileSystem();
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return (toInterfaceOrientation == UIInterfaceOrientation.Portrait || toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown);
		}
	}
}
