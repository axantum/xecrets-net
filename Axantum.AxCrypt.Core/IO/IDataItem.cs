using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public interface IDataItem
    {
        /// <summary>
        /// Gets a folder containing this file, or as close as possible. If the file is on a readon-only medium
        /// for example, an alternate location may be given. There is no guarantee that the container returned
        /// actually contains this file.
        /// </summary>
        IDataContainer Container { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is available in the underlying file system or otherwise.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is available; otherwise, <c>false</c>.
        /// </value>
        bool IsAvailable { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is a file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a file; otherwise, <c>false</c>.
        /// </value>
        bool IsFile { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is a folder.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a folder; otherwise, <c>false</c>.
        /// </value>
        bool IsFolder { get; }

        /// <summary>
        /// Get the Name part without the folder part of the path.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get the full name including drive, directory and file name if any
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Deletes the underlying file this instance refers to.
        /// </summary>
        void Delete();

        bool IsNetworkPath { get; }
    }
}