using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Forms
{
    public class EmbeddedResourceManager : ResourceManager
    {
        private const string RESOURCE_NAME_BASE = "AxCrypt.Content.Properties.Resources";

        private Dictionary<string, Dictionary<string, string>> _cultureResourceDictionary = new Dictionary<string, Dictionary<string, string>>();

        private Assembly _resourcesAssembly = typeof(global::AxCrypt.Content.Properties.Resources).GetTypeInfo().Assembly;

        public static void Initialize()
        {
            HackContentResourceManager();
        }

        private EmbeddedResourceManager()
            : base("AxCrypt.Content.Properties.Resources", typeof(global::AxCrypt.Content.Properties.Resources).GetTypeInfo().Assembly)
        {
            Dictionary<string, string> dictionary = CultureDictionary(String.Empty);

            NeutralResourcesLanguageAttribute neutral = _resourcesAssembly.GetCustomAttribute<NeutralResourcesLanguageAttribute>();
            _cultureResourceDictionary.Add(neutral.CultureName, dictionary);
        }

        private static void HackContentResourceManager()
        {
            Assembly me = Assembly.GetExecutingAssembly();
            ManifestResourceInfo info = me.GetManifestResourceInfo(RESOURCE_NAME_BASE + ".resources");
            if (info == null)
            {
                return;
            }

            FieldInfo fi = typeof(global::AxCrypt.Content.Content).BaseType.GetField("resourceMan", BindingFlags.NonPublic | BindingFlags.Static);
            fi.SetValue(null, new EmbeddedResourceManager());
        }

        public override string GetString(string name, CultureInfo culture)
        {
            if (culture == null)
            {
                culture = Thread.CurrentThread.CurrentUICulture;
            }

            Dictionary<string, string> dictionary;
            string value;

            dictionary = CultureDictionary(culture.Name);
            if (dictionary != null && dictionary.TryGetValue(name, out value))
            {
                return value;
            }

            string language = culture.Name.Split('-')[0];
            if (language != culture.Name)
            {
                dictionary = CultureDictionary(language);
            }
            if (dictionary != null && dictionary.TryGetValue(name, out value))
            {
                return value;
            }

            dictionary = CultureDictionary(String.Empty);
            if (dictionary.TryGetValue(name, out value))
            {
                return value;
            }

            return null;
        }

        private Dictionary<string, string> CultureDictionary(string name)
        {
            Dictionary<string, string> dictionary;
            if (_cultureResourceDictionary.TryGetValue(name, out dictionary))
            {
                return dictionary;
            }

            dictionary = TryLoadCultureDictionary(name);
            if (dictionary != null)
            {
                _cultureResourceDictionary.Add(name, dictionary);
                return dictionary;
            }

            _cultureResourceDictionary.Add(name, null);
            return null;
        }

        private Dictionary<string, string> TryLoadCultureDictionary(string name)
        {
            string resourceName = RESOURCE_NAME_BASE + ((name.Length > 0) ? "." + name : name) + ".resources";

            Stream stream = _resourcesAssembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                Dictionary<string, string> cultureDictionary = new Dictionary<string, string>();
                using (ResourceReader reader = new ResourceReader(stream))
                {
                    foreach (DictionaryEntry entry in reader)
                    {
                        string key = entry.Key as string;
                        if (key != null)
                        {
                            cultureDictionary.Add(key, entry.Value as string ?? String.Empty);
                        }
                    }
                }
                return cultureDictionary;
            }
            return null;
        }
    }
}