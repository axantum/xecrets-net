#region Coypright and GPL License

/*
 * Xecrets.Net - Copyright © 2022-2026, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets.Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets.Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets.Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
 */

#endregion Coypright and GPL License

namespace Xecrets.Core.Models;

public sealed record Progress(ProgressKind Kind, long Count)
{
    public static Progress LevelStarted() => new(ProgressKind.LevelStarted, 0);

    /// <summary>
    /// Add to the total work count.
    /// </summary>
    /// <param name="count">The amount of work to add.</param>
    public static Progress TotalAdded(long count) => new(ProgressKind.TotalAdded, count);

    /// <summary>
    /// Add to the count of work having been performed. May lead to a Progressing event.
    /// </summary>
    /// <param name="count">The amount of work having been performed in this step.</param>
    public static Progress CountAdded(long count) => new(ProgressKind.CountAdded, count);

    public static Progress LevelFinished() => new(ProgressKind.LevelFinished, 0);
}
