using System;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Mono
{
	/// <summary>
	/// Placeholder-implementation of a FileSystemWatcher as its concept is not available on iOS.
	/// </summary>
	public class FileWatcher : IFileWatcher
	{
		const string UnavailableMessage = "The FileSystemWatcher concept is not available on iOS";

		public FileWatcher (string fileToWatch)
		{
			throw new NotImplementedException(UnavailableMessage);
		}

		#region IFileWatcher implementation

		event EventHandler<FileWatcherEventArgs> IFileWatcher.FileChanged {
			add {
				throw new NotImplementedException (UnavailableMessage);
			}
			remove {
				throw new NotImplementedException (UnavailableMessage);
			}
		}

		#endregion

		#region IDisposable implementation

		void IDisposable.Dispose ()
		{
			throw new NotImplementedException (UnavailableMessage);
		}

		#endregion
	}
}

