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

        private Version _lastReliabilityUpdate;

        private Version _lastSecurityUpdate;

        private VersionUpdateKind()
            : this(string.Empty, string.Empty, string.Empty)
        {
        }

        public VersionUpdateKind(string currentVersion, string lastReliabilityUpdate, string lastSecurityUpdate)
        {
            if (!Version.TryParse(string.IsNullOrEmpty(currentVersion) ? DownloadVersion.VersionZero.ToString() : currentVersion, out _currentVersion))
            {
                throw new ArgumentException("Invalid version format", nameof(currentVersion));
            }
            _newVersion = _currentVersion;
            if (!Version.TryParse(string.IsNullOrEmpty(lastReliabilityUpdate) ? DownloadVersion.VersionZero.ToString() : lastReliabilityUpdate, out _lastReliabilityUpdate))
            {
                throw new ArgumentException("Invalid version format", nameof(lastReliabilityUpdate));
            }
            if (!Version.TryParse(string.IsNullOrEmpty(lastSecurityUpdate) ? DownloadVersion.VersionZero.ToString() : lastSecurityUpdate, out _lastSecurityUpdate))
            {
                throw new ArgumentException("Invalid version format", nameof(lastSecurityUpdate));
            }
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
                _lastReliabilityUpdate = _lastReliabilityUpdate,
                _lastSecurityUpdate = _lastSecurityUpdate,
                _newVersion = newVersion,
            };
        }

        public Version CurrentVersion { get { return _currentVersion; } }

        public Version NewVersion { get { return _newVersion; } }

        public bool NeedsCriticalReliabilityUpdate
        {
            get
            {
                return _lastReliabilityUpdate > DownloadVersion.VersionZero && _currentVersion > DownloadVersion.VersionZero && _currentVersion < _lastReliabilityUpdate;
            }
        }

        public bool NeedsCriticalSecurityUpdate
        {
            get
            {
                return _lastSecurityUpdate > DownloadVersion.VersionZero && _currentVersion > DownloadVersion.VersionZero && _currentVersion < _lastSecurityUpdate;
            }
        }
    }
}