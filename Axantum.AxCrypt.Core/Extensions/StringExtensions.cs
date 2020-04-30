#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Properties;
using Axantum.AxCrypt.Core.UI;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Extension for String.Format using InvariantCulture
        /// </summary>
        /// <param name="format"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string InvariantFormat(this string format, params object[] parameters)
        {
            string formatted = String.Format(CultureInfo.InvariantCulture, format, parameters);
            return formatted;
        }

        /// <summary>
        /// Formats with String.Format, but does query string value encoding first.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static string QueryFormat(this string format, Uri baseUrl, params object[] parameters)
        {
            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }
            if (baseUrl == null)
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            string[] encoded = new string[parameters.Length + 1];
            encoded[0] = baseUrl.ToString();
            for (int i = 0; i < parameters.Length; ++i)
            {
                encoded[i + 1] = Uri.EscapeDataString(parameters[i].ToString());
            }

            return format.InvariantFormat(encoded);
        }

        /// <summary>
        /// Removes the 'Controller' suffix, we nameof() can be used.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <returns>The string minus and 'Controller' prefix.</returns>
        public static string MvcController(this string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException(nameof(fullName));
            }

            int i = fullName.LastIndexOf("Controller");
            if (i == -1)
            {
                return fullName;
            }
            return fullName.Substring(0, i);
        }

        /// <summary>
        /// Convenience extension for String.Format using the provided CultureInfo
        /// </summary>
        /// <param name="format"></param>
        /// <param name="cultureInfo"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string FormatWith(this string format, CultureInfo cultureInfo, params object[] parameters)
        {
            string formatted = String.Format(cultureInfo, format, parameters);
            return formatted;
        }

        public static string Default(this string value, object defaultValue)
        {
            if (defaultValue == null)
            {
                throw new ArgumentNullException(nameof(defaultValue));
            }

            if (string.IsNullOrEmpty(value))
            {
                return defaultValue.ToString();
            }
            return value;
        }

        public static string ToUtf8Base64(this string passphrase)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(passphrase));
        }

        /// <summary>
        /// Create a file name based on an existing, but convert the file name to the pattern used by
        /// AxCrypt for encrypted files. The original must not already be in that form.
        /// </summary>
        /// <param name="fileInfo">A file name representing a file that is not encrypted</param>
        /// <returns>A corresponding file name representing the encrypted version of the original</returns>
        public static string CreateEncryptedName(this string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            string extension = Resolve.Portable.Path().GetExtension(fullName);
            string encryptedName = fullName;
            encryptedName = encryptedName.Substring(0, encryptedName.Length - extension.Length);
            encryptedName += extension.Replace('.', '-');
            encryptedName += OS.Current.AxCryptExtension;

            return encryptedName;
        }

        public static FileLock CreateUniqueFile(this string fullName)
        {
            IDataStore pathInfo = New<IDataStore>(fullName);
            string extension = Resolve.Portable.Path().GetExtension(fullName);
            int version = 0;
            while (true)
            {
                try
                {
                    string alternateExtension = (version > 0 ? "." + version.ToString(CultureInfo.InvariantCulture) : String.Empty) + extension;
                    string alternateName = Resolve.Portable.Path().GetFileNameWithoutExtension(pathInfo.Name) + alternateExtension;
                    IDataStore alternateFileInfo = pathInfo.Container.CreateNewFile(alternateName);
                    return New<FileLocker>().Acquire(alternateFileInfo);
                }
                catch (AxCryptException ace)
                {
                    if (ace.ErrorStatus != ErrorStatus.FileExists)
                    {
                        throw;
                    }
                    New<IReport>().Exception(ace);
                }
                ++version;
            }
        }

        /// <summary>
        /// Trim a log message from extra information in front, specifically text preceding the
        /// log level text such as Information, Warning etc. There must be a space preceding
        /// the log level text. Recognized texts are 'Information', 'Warning', 'Debug', 'Error'
        /// and 'Fatal'.
        /// </summary>
        /// <param name="message">A log message</param>
        /// <returns>A possible trimmed message</returns>
        /// <remarks>
        /// This is primarily intended to facilitate more compact logging in a GUI
        /// </remarks>
        public static string TrimLogMessage(this string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            int skipIndex = message.IndexOf(" Information", StringComparison.Ordinal);
            skipIndex = skipIndex < 0 ? message.IndexOf(" Warning", StringComparison.Ordinal) : skipIndex;
            skipIndex = skipIndex < 0 ? message.IndexOf(" Debug", StringComparison.Ordinal) : skipIndex;
            skipIndex = skipIndex < 0 ? message.IndexOf(" Error", StringComparison.Ordinal) : skipIndex;
            skipIndex = skipIndex < 0 ? message.IndexOf(" Fatal", StringComparison.Ordinal) : skipIndex;

            return message.Substring(skipIndex + 1);
        }

        /// <summary>
        /// Gets a representation of a data container (folder) from an environment name or similar.
        /// </summary>
        /// <param name="name">The environment name.</param>
        /// <returns>A container, or null if the name was not found or was empty.</returns>
        /// <exception cref="System.ArgumentNullException">name</exception>
        public static IDataContainer FolderFromEnvironment(this string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            string value = OS.Current.EnvironmentVariable(name);
            if (String.IsNullOrEmpty(value))
            {
                return null;
            }

            return New<IDataContainer>(value);
        }

        public static string NormalizeFilePath(this string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            filePath = filePath.Replace(Resolve.Portable.Path().DirectorySeparatorChar == '/' ? '\\' : '/', Resolve.Portable.Path().DirectorySeparatorChar);
            return filePath;
        }

        public static string NormalizeFolderPath(this string folder)
        {
            folder = folder.NormalizeFilePath();
            if (String.Compare(folder, Resolve.Portable.Path().GetPathRoot(folder), StringComparison.OrdinalIgnoreCase) == 0)
            {
                return folder;
            }
            int directorySeparatorChars = 0;
            while (folder.Length - (directorySeparatorChars + 1) > 0 && folder[folder.Length - (directorySeparatorChars + 1)] == Resolve.Portable.Path().DirectorySeparatorChar)
            {
                ++directorySeparatorChars;
            }

            if (directorySeparatorChars == 0)
            {
                return folder + Resolve.Portable.Path().DirectorySeparatorChar;
            }
            return folder.Substring(0, folder.Length - (directorySeparatorChars - 1));
        }

        public static byte[] FromHex(this string hex)
        {
            if (hex == null)
            {
                throw new ArgumentNullException("hex");
            }
            hex = hex.Replace(" ", String.Empty).Replace("\r", String.Empty).Replace("\n", String.Empty);
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Odd number of characters is not allowed in a hex string.");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = Byte.Parse(hex.Substring(i + i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return bytes;
        }

        public static bool IsValidEmailOrEmpty(this string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                return true;
            }
            return email.IsValidEmail();
        }

        public static bool IsValidEmail(this string email)
        {
            string address;
            return New<IEmailParser>().TryParse(email, out address);
        }

        private static string[] _topLevelDomains = LoadTopLevelDomains();

        /// <summary>
        /// Loads the top level domains. Get the updated list here: http://data.iana.org/TLD/tlds-alpha-by-domain.txt .
        /// </summary>
        /// <returns>A sorted array of TLDs without dots.</returns>
        private static string[] LoadTopLevelDomains()
        {
            List<string> topLevelDomains = new List<string>();
            using (StringReader sr = new StringReader(Resources.tlds_alpha_by_domain))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }
                    topLevelDomains.Add(line);
                }
            }
            topLevelDomains.Sort(StringComparer.OrdinalIgnoreCase);
            return topLevelDomains.ToArray();
        }

        /// <summary>
        /// Determines whether the value is a valid top level domain name, whithout
        /// a preceeding 'dot'.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static bool IsValidTopLevelDomain(this string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (String.Compare(value, "local", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }

            if (Array.BinarySearch(_topLevelDomains, value, StringComparer.OrdinalIgnoreCase) < 0)
            {
                return false;
            }
            return true;
        }

        public static void ShowWarning(this string message, string title)
        {
            New<IUIThread>().PostTo(async () => await New<IPopup>().ShowAsync(PopupButtons.Ok, title, message));
        }

        public static void ShowWarning(this string message, string title, DoNotShowAgainOptions doNotShowAgainOption)
        {
            New<IUIThread>().PostTo(async () => await New<IPopup>().ShowAsync(PopupButtons.Ok, title, message, doNotShowAgainOption));
        }

        /// <summary>
        /// Tells if the given string matches the given wildcard.
        /// Two wildcards are allowed: '*' and '?'
        /// '*' matches 0 or more characters
        /// '?' matches any character
        /// </summary>
        /// <param name="wildcard">The wildcard.</param>
        /// <param name="text">The s.</param>
        /// <returns></returns>
        public static bool WildcardMatch(this string wildcard, string text)
        {
            if (wildcard == null)
            {
                throw new ArgumentNullException(nameof(wildcard));
            }
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return WildcardMatch(wildcard, text, 0, 0);
        }

        /// <summary>
        /// Internal matching algorithm.
        /// </summary>
        /// <param name="wildcard">The wildcard.</param>
        /// <param name="text">The text.</param>
        /// <param name="wildcardIndex">Index of the wildcard.</param>
        /// <param name="textIndex">Index of the text.</param>
        /// <returns></returns>
        private static bool WildcardMatch(string wildcard, string text, int wildcardIndex, int textIndex)
        {
            while (wildcardIndex < wildcard.Length)
            {
                char c = wildcard[wildcardIndex];
                switch (c)
                {
                    // always a match
                    case '?':
                        break;

                    case '*':
                        // if this is the last wildcard char, then we have a match, whatever the tested string is
                        if (wildcardIndex == wildcard.Length - 1)
                        {
                            return true;
                        }

                        // test if a match follows
                        return Enumerable.Range(textIndex, text.Length - textIndex).Any(i => WildcardMatch(wildcard, text, wildcardIndex + 1, i));

                    default:
                        if (textIndex == text.Length)
                        {
                            return false;
                        }
                        char cc = char.ToLower(c);
                        char sc = char.ToLower(text[textIndex]);
                        if (cc != sc)
                        {
                            return false;
                        }
                        break;
                }

                ++wildcardIndex;
                ++textIndex;
            }

            return textIndex == text.Length;
        }

        public static Uri GetPasswordResetUrl(this string userEmail)
        {
            UriBuilder url = new UriBuilder(Texts.PasswordResetHyperLink);
            url.Query = $"email={userEmail}";
            return url.Uri;
        }

        public static Uri GetloginUrl(this string userEmail)
        {
            UriBuilder url = new UriBuilder("https://account.axcrypt.net/Home/Login");
            url.Query = $"email={userEmail}";
            return url.Uri;
        }
    }
}