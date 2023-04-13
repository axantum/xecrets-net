using AxCrypt.Core.Extensions;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AxCrypt.Core.Runtime
{
    public class AboutAssembly
    {
        private Assembly _assembly;

        public AboutAssembly(Assembly assembly)
        {
            _assembly = assembly;
        }

        public string AboutTitleText
        {
            get
            {
                return Texts.TitleMainWindow.InvariantFormat(AssemblyProduct, AssemblyVersion, string.IsNullOrEmpty(AssemblyDescription) ? string.Empty : " " + AssemblyDescription);
            }
        }

        public string AboutVersionText
        {
            get
            {
                return AssemblyVersion + (string.IsNullOrEmpty(AssemblyDescription) ? string.Empty : " " + AssemblyDescription);
            }
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                IEnumerable<Attribute> attributes = _assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute));
                if (attributes.Any())
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes.First();
                    if (!String.IsNullOrEmpty(titleAttribute.Title))
                    {
                        return titleAttribute.Title;
                    }
                }
                return _assembly.GetName().Name ?? throw new InvalidOperationException("Assembly.Name returned null.");
            }
        }

        public string AssemblyVersion
        {
            get
            {
                IEnumerable<Attribute> attributes = _assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute));
                if (!attributes.Any())
                {
                    return string.Empty;
                }
                return ((AssemblyInformationalVersionAttribute)attributes.First()).InformationalVersion;
            }
        }

        public string AssemblyDescription
        {
            get
            {
                IEnumerable<Attribute> attributes = _assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute));
                if (!attributes.Any())
                {
                    return string.Empty;
                }
                return ((AssemblyDescriptionAttribute)attributes.First()).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                IEnumerable<Attribute> attributes = _assembly.GetCustomAttributes(typeof(AssemblyProductAttribute));
                if (!attributes.Any())
                {
                    return string.Empty;
                }
                return ((AssemblyProductAttribute)attributes.First()).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                IEnumerable<Attribute> attributes = _assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute));
                if (!attributes.Any())
                {
                    return string.Empty;
                }
                return ((AssemblyCopyrightAttribute)attributes.First()).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                IEnumerable<Attribute> attributes = _assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute));
                if (!attributes.Any())
                {
                    return string.Empty;
                }
                return ((AssemblyCompanyAttribute)attributes.First()).Company;
            }
        }

        #endregion Assembly Attribute Accessors
    }
}
