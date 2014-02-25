using System;
using System.IO;
using Axantum.AxCrypt.Core;

namespace Axantum.AxCrypt.iOS.Infrastructure
{
	public static class BasePath
	{
		public const int 
			TransferredFilesId = 1, 
			ReceivedFilesId = 2;

		public static string TransferredFiles {
			get {
				return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
		}

		public static string ReceivedFiles {
			get {
				return Path.Combine (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Recent");
			}
		}

		public static string GetBasePath(int pathId) {
			switch (pathId) {
			case TransferredFilesId:
				return TransferredFiles;
			case ReceivedFilesId:
				return ReceivedFiles;
			default:
				throw new ArgumentException ("Invalid path id: " + pathId);
			}
		}

		public static string Expand(string fileName, int pathId) {
			string basePath = GetBasePath (pathId);
			if (!fileName.EndsWith (OS.Current.AxCryptExtension))
				fileName += OS.Current.AxCryptExtension;
			return Path.Combine (basePath, fileName);
		}
	}
}

