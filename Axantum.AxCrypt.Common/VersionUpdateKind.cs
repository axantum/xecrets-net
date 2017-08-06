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

        private List<Tuple<Version, Version>> _unreliableVersions;

        private List<Tuple<Version, Version>> _insecureVersions;

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
            _unreliableVersions = ParseVersionRanges(unreliableVersions);
            _insecureVersions = ParseVersionRanges(insecureVersions);
        }

        /// <summary>
        /// Parses version ranges in the form 1.0.0.0 1.1.0.0 1.2.0.0-1.3.0.0 etc
        /// </summary>
        /// <param name="versionRanges">The version ranges.</param>
        /// <returns></returns>
        private List<Tuple<Version, Version>> ParseVersionRanges(string versionRanges)
        {
            versionRanges = versionRanges.Trim();
            string[] versions = versionRanges.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            List<Tuple<Version, Version>> versionRangeList = new List<Tuple<Version, Version>>();
            foreach (string version in versions)
            {
                Tuple<Version, Version> aRange = ParseVersionRange(version);
                versionRangeList.Add(aRange);
            }

            return versionRangeList;
        }

        private Tuple<Version, Version> ParseVersionRange(string version)
        {
            string[] fromandto = version.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (fromandto.Length < 1 || fromandto.Length > 2)
            {
                throw new ArgumentException($"Bad format of range or version '{version}'.", nameof(version));
            }

            List<Version> range = new List<Version>();
            foreach (string fromorto in fromandto)
            {
                Version v;
                if (!Version.TryParse(string.IsNullOrEmpty(fromorto) ? DownloadVersion.VersionZero.ToString() : fromorto, out v))
                {
                    throw new ArgumentException($"Invalid version format '{fromorto}'.", nameof(version));
                }
                range.Add(v);
            }

            if (fromandto.Length == 1)
            {
                return new Tuple<Version, Version>(range[0], range[0]);
            }
            if (range[0] > range[1])
            {
                throw new ArgumentException($"Bad range '{version}'.", nameof(version));
            }
            return new Tuple<Version, Version>(range[0], range[1]);
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
                return IsInRange(_currentVersion, _unreliableVersions);
            }
        }

        public bool NeedsCriticalSecurityUpdate
        {
            get
            {
                return IsInRange(_currentVersion, _insecureVersions);
            }
        }

        private bool IsInRange(Version version, List<Tuple<Version, Version>> ranges)
        {
            foreach (Tuple<Version, Version> fromto in ranges)
            {
                if (version <= DownloadVersion.VersionZero)
                {
                    continue;
                }
                if (version >= fromto.Item1 && version <= fromto.Item2)
                {
                    return true;
                }
            }
            return false;
        }
    }
}