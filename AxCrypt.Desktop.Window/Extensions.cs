﻿#region Coypright and License

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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.UI;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AxCrypt.Common;
using static AxCrypt.Abstractions.TypeResolve;
using System.Threading.Tasks;

namespace AxCrypt.Desktop.Window
{
    internal static class Extensions
    {
        public static Point Fallback(this Point value, Point fallback)
        {
            return value != default(Point) ? value : fallback;
        }

        public static Point Safe(this Point value)
        {
            if (value.X < 0)
            {
                value = new Point(0, value.Y);
            }
            if (value.Y < 0)
            {
                value = new Point(value.X, 0);
            }
            return value;
        }

        public static IEnumerable<string> GetDragged(this DragEventArgs e)
        {
            IList<string> dropped = e.Data.GetData(DataFormats.FileDrop) as IList<string>;
            if (dropped == null)
            {
                return new string[0];
            }

            return dropped;
        }
    }
}