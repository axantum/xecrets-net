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

using System.Collections;

using NDesk.Options;

namespace Xecrets.Core.Desktop.CommandLine;

public class OptionSet(Version version) : IEnumerable<Option>
{
    private readonly OptionSetCollection _options = [];

    private readonly List<Option> _definitions = [];

    public Version Version { get; } = version;

    public void Add(string prototype, Action<string?> action) =>
        Add(prototype, null, action);

    public void Add(string prototype, string? description, Action<string?> action) =>
        AddOption(new ActionOption(prototype, description, 1, values => action(values[0])));

    public void Add(string prototype, Action<string?, string?> action) =>
        Add(prototype, null, action);

    public void Add(string prototype, string? description, Action<string?, string?> action) =>
        AddOption(new ActionOption(prototype, description, 2, values => action(values[0], values[1])));

    public void Add(string prototype, string? description, Action<string?, string?, string?> action) =>
        AddOption(new ActionOption(prototype, description, 3, values => action(values[0], values[1], values[2])));

    public IList<string> Parse(IEnumerable<string> arguments)
    {
        try
        {
            return _options.Parse(arguments);
        }
        catch (NDesk.Options.OptionException ex)
        {
            throw new OptionException(ex.Message, ex);
        }
    }

    public IEnumerator<Option> GetEnumerator() => _definitions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void AddOption(OptionBase option)
    {
        _options.Add(option);
        _definitions.Add(new Option(option.GetNames(), option.Description));
    }

    private sealed class ActionOption(string prototype, string? description, int count, Action<IList<string?>> action) : OptionBase(prototype, description!, count)
    {
        protected override void OnParseComplete(OptionContext optionContext) => action(optionContext.OptionValues);
    }
}
