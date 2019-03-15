using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    public class CultureNameMapper
    {
        private IDictionary<string, string> _cultureMap { get; set; }

        private Func<Task<IList<CultureInfo>>> _getCultureInfos;

        public CultureNameMapper(Func<Task<IList<CultureInfo>>> getCultureInfos)
        {
            _getCultureInfos = getCultureInfos;
        }

        public async Task<IDictionary<string, string>> GetCultureMap()
        {
            if (_cultureMap == null)
            {
                IList<CultureInfo> cultureInfos = await _getCultureInfos().Free();
                _cultureMap = GetSupportedCultureCodeCountryName(cultureInfos);
            }

            return _cultureMap;
        }

        public IDictionary<string, string> GetSupportedCultureCodeCountryName(IList<CultureInfo> cultureInfos)
        {
            Dictionary<string, string> cultureMap = new Dictionary<string, string>();
            foreach (CultureInfo culture in cultureInfos)
            {
                string cultureDisplayName = string.Format("{0} ({1})", ToTitleCase(culture, culture.Parent.NativeName), culture.Parent.DisplayName);
                cultureMap.Add(cultureDisplayName, culture.Name);
            }

            return cultureMap;
        }

        private static string ToTitleCase(CultureInfo culture, string nativeName)
        {
            return culture.TextInfo.ToUpper(nativeName[0]) + nativeName.Substring(1);
        }
    }
}