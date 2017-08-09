using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    public class VersionUpdateKind
    {
        private Version _currentVersion;

        private Version _newVersion;

        private VersionRange _unreliableVersions;

        private VersionRange _insecureVersions;

        private VersionUpdateKind()
            : this(string.Empty, string.Empty, string.Empty)
        {
        }

        public VersionUpdateKind(string currentVersion, string unreliableVersions, string insecureVersions)
        {
            if (!Version.TryParse(string.IsNullOrEmpty(currentVersion) ? DownloadVersion.VersionZero.ToString() : currentVersion, out _currentVersion))
            {
                throw new ArgumentException("Invalid version format", nameof(currentVersion));
            }
            _newVersion = _currentVersion;
            _unreliableVersions = new VersionRange(unreliableVersions);
            _insecureVersions = new VersionRange(insecureVersions);
        }

        public static readonly VersionUpdateKind Empty = new VersionUpdateKind();

        public VersionUpdateKind New(Version newVersion)
        {
            if (newVersion == null)
            {
                throw new ArgumentNullException(nameof(newVersion));
            }

            return new VersionUpdateKind()
            {
                _currentVersion = _currentVersion,
                _unreliableVersions = _unreliableVersions,
                _insecureVersions = _insecureVersions,
                _newVersion = newVersion,
            };
        }

        public Version CurrentVersion { get { return _currentVersion; } }

        public Version NewVersion { get { return _newVersion; } }

        public bool NeedsCriticalReliabilityUpdate
        {
            get
            {
                return _unreliableVersions.IsInRange(_currentVersion);
            }
        }

        public bool NeedsCriticalSecurityUpdate
        {
            get
            {
                return _insecureVersions.IsInRange(_currentVersion);
            }
        }
    }
}