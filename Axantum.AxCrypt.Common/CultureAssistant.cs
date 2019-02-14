using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    public static class CultureAssistant
    {
        private static IList<CultureInfo> _cultureList { get; set; }

        public static async Task<List<object>> GetResultAsyc(Func<Task<IList<CultureInfo>>> operation)
        {
            _cultureList = await operation().Free();
            return GetSupportedCultureCodeCountryName();
        }

        public static List<object> GetSupportedCultureCodeCountryName()
        {
            if (_cultureList == null)
            {
                return _localCultureList;
            }

            if (!_cultureList.Any())
            {
                return _localCultureList;
            }

            List<object> listOfCultures = new List<object>();
            foreach (CultureInfo culture in _cultureList)
            {
                string cultureDisplayName = string.Format("{0}({1})", culture.Parent.NativeName, culture.Parent.DisplayName);
                listOfCultures.Add(new { Name = cultureDisplayName, Tag = culture.Name });
            }

            return listOfCultures;
        }

        private static List<object> _localCultureList = new List<object>() {
            new { Name = Texts.EnglishLanguageToolStripMenuItemText, Tag = "en-US" },
            new { Name = Texts.GermanLanguageSelectionText, Tag = "de-DE" },
            new { Name = Texts.DutchLanguageSelection, Tag = "nl-NL" },
            new { Name = Texts.SpanishLanguageToolStripMenuItemText, Tag = "es-ES" },
            new { Name = Texts.FrancaisLanguageToolStripMenuItemText, Tag = "fr-FR" },
            new { Name = Texts.ItalianLanguageSelection, Tag = "it-IT" },
            new { Name = Texts.KoreanLanguageSelection, Tag = "ko" },
            new { Name = Texts.PolishLanguageToolStripMenuItemText, Tag = "pl-PL" },
            new { Name = Texts.PortugueseBrazilLanguageSelection, Tag = "pt-BR" },
            new { Name = Texts.RussianLanguageSelection, Tag = "ru-RU" },
            new { Name = Texts.SwedishLanguageToolStripMenuItemText, Tag = "sv-SE" },
            new { Name = Texts.TurkishLanguageToolStripMenuItemText, Tag = "tr-TR" }
        };
    }
}