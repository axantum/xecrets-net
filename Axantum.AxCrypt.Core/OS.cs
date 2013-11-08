#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// Provides syntactically convenient access to runtime dependent instances.
    /// </summary>
    public static class OS
    {
        /// <summary>
        /// Gets the current IRuntimeEnvironment platform dependent implementation instance.
        /// </summary>
        /// <value>
        /// The current IRuntimeEnvironment platform dependent implementation instance.
        /// </value>
        public static IRuntimeEnvironment Current
        {
            get
            {
                return Instance.Environment;
            }
        }

        /// <summary>
        /// Gets the global logging instance. This is a convenience method to get the logger from
        /// the current IRuntimeEnvironment instance.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public static ILogging Log { get { return Current.Log; } }

        private static readonly List<Regex> _pathFilters = new List<Regex>();

        public static IList<Regex> PathFilters
        {
            get
            {
                return _pathFilters;
            }
        }
    }
}