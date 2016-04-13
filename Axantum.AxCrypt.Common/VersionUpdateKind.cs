using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    public class VersionUpdateKind
    {
        private const string _versionZero = "2.0.0.0";

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
            if (!Version.TryParse(string.IsNullOrEmpty(currentVersion) ? _versionZero : currentVersion, out _currentVersion))
            {
                throw new ArgumentException("Invalid version format", nameof(currentVersion));
            }
            _newVersion = _currentVersion;
            if (!Version.TryParse(string.IsNullOrEmpty(lastReliabilityUpdate) ? _versionZero : lastReliabilityUpdate, out _lastReliabilityUpdate))
            {
                throw new ArgumentException("Invalid version format", nameof(lastReliabilityUpdate));
            }
            if (!Version.TryParse(string.IsNullOrEmpty(lastSecurityUpdate) ? _versionZero : lastSecurityUpdate, out _lastSecurityUpdate))
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
                return _currentVersion < _lastReliabilityUpdate;
            }
        }

        public bool NeedsCriticalSecurityUpdate
        {
            get
            {
                return _currentVersion < _lastSecurityUpdate;
            }
        }
    }
}