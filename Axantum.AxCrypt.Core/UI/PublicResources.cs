#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public static class PublicResources
    {
#if DEBUG
#pragma warning disable 414

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "This is just a dummy here to ensure 100% code coverage in Unit Tests.")]
        private static Axantum.AxCrypt.Core.Properties.Resources _codeCoverageForInternalDesignerGeneratedConstructorDummy = new Axantum.AxCrypt.Core.Properties.Resources();

#pragma warning restore 414
#endif

        public static CultureInfo Culture
        {
            get
            {
                return Resources.Culture;
            }
            set
            {
                Resources.Culture = value;
            }
        }

        public static Stream AxCryptIcon
        {
            get
            {
                return typeof(Resources).Assembly.GetManifestResourceStream("Axantum.AxCrypt.Core.resources.axcrypticon.ico");
            }
        }

        public static string BouncycastleLicense
        {
            get
            {
                return Resources.bouncycastlelicense;
            }
        }

        public static string JsonNetLicense
        {
            get
            {
                return Resources.json_netlicense;
            }
        }
    }
}