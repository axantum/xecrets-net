using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// Holds information about a folder that is watched for file changes, to enable
    /// automatic encryption of files for example. Instances of this class are
    /// immutable
    /// </summary>
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class WatchedFolder : IEquatable<WatchedFolder>
    {
        [DataMember(Name = "Path")]
        public string Path { get; private set; }

        public WatchedFolder(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            Path = path;
        }

        public bool Equals(WatchedFolder other)
        {
            if (other == null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            return Path == other.Path;
        }

        public override bool Equals(object obj)
        {
            WatchedFolder watchedFolder = obj as WatchedFolder;
            if (watchedFolder == null)
            {
                return false;
            }

            return Equals(watchedFolder);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public static bool operator ==(WatchedFolder left, WatchedFolder right)
        {
            if ((object)left == null || ((object)right == null))
            {
                return Object.Equals(left, right);
            }

            return left.Equals(right);
        }

        public static bool operator !=(WatchedFolder left, WatchedFolder right)
        {
            return !(left == right);
        }
    }
}
