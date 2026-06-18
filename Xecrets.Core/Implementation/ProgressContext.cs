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

namespace Xecrets.Core.Implementation;

public abstract class ProgressContext : IProgressContext
{
    private readonly AxCrypt.Core.UI.IProgressContext _axCryptContext;

    protected ProgressContext(TimeSpan timeToFirstProgress, TimeSpan progressTimeInterval)
        : this(new AxCrypt.Core.UI.ProgressContext(timeToFirstProgress, progressTimeInterval))
    {
    }

    private ProgressContext(AxCrypt.Core.UI.IProgressContext axCryptContext)
    {
        _axCryptContext = axCryptContext;
        _axCryptContext.Progressing += (_, e) => OnProgressing(new ProgressEventArgs(e.Percent, e.Display));
    }

    public void Report(Progress value)
    {
        switch (value.Kind)
        {
            case ProgressKind.LevelStarted:
                _axCryptContext.NotifyLevelStart();
                break;
            case ProgressKind.TotalAdded:
                _axCryptContext.AddTotal(value.Count);
                break;
            case ProgressKind.CountAdded:
                _axCryptContext.AddCount(value.Count);
                break;
            case ProgressKind.LevelFinished:
                _axCryptContext.NotifyLevelFinished();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    public string Display { get => _axCryptContext.Display; set => _axCryptContext.Display = value; }

    public event EventHandler<ProgressEventArgs>? Progressing;

    protected virtual void OnProgressing(ProgressEventArgs e)
    {
        Progressing?.Invoke(this, e);
    }
}
