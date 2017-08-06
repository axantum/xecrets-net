using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    public class VersionRange
    {
        private List<Tuple<Version, Version>> _versions;

        public VersionRange(string versionRanges)
        {
            _versions = ParseVersionRanges(versionRanges);
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

        public bool IsInRange(Version version)
        {
            foreach (Tuple<Version, Version> fromto in _versions)
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