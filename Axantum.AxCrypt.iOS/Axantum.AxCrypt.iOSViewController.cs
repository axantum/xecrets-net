using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.iOS
{
	public partial class Axantum_AxCrypt_iOSViewController : AppViewController
	{
		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public static Axantum_AxCrypt_iOSViewController Instance {
			get; private set;
		}

		public static UIView LoadedView {
			get {
				return Instance.View;
			} 
		}

		Section fileSection;

		public static void ReloadFileSystem ()
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

			Instance.fileSection.Clear();
			foreach (StyledStringElement item in selection) {
				string targetPath = Path.Combine(appFiles, item.Caption + ".axx");
				item.Tapped += () => AppViewController.OpenFile(targetPath);
				Instance.fileSection.Add(item);
			}
		}

		public Axantum_AxCrypt_iOSViewController (IntPtr handle) : base (handle)
		{
			Instance = this;
			TableView.Source = new EditableTableViewSource(this);
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		#region View lifecycle


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
		}
		

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			if (Root.Count > 0) return;

			Root.Add(new Section[] {
				new Section("Opening encrypted files", "By installing this app, you have given your other apps super powers! In your mail app, for example, you can now simply tap on .axx documents to open them with AxCrypt!"),
				(fileSection = new Section("Local files", "Local files can be transferred from iTunes") {

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
				})
			});
			
			ReloadFileSystem();
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}
		
		#endregion
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return (toInterfaceOrientation == UIInterfaceOrientation.Portrait || toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown);
		}
	}
}

