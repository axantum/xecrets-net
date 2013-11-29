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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Extensions;
using System.Reflection;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class ViewModelBinder
    {
        private Dictionary<string, List<Action<object>>> _actions = new Dictionary<string, List<Action<object>>>();

        private IViewModel _viewModel;

        public ViewModelBinder(IViewModel viewModel)
        {
            _viewModel = viewModel;
            viewModel.PropertyChanged += HandlePropertyChanged;
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            object value = GetProperty(sender, e.PropertyName);
            foreach (Action<object> action in _actions[e.PropertyName])
            {
                action(value);
            }
        }

        public void BindPropertyToView<T>(string name, Action<T> action)
        {
            List<Action<object>> actions;
            if (!_actions.TryGetValue(name, out actions))
            {
                actions = new List<Action<object>>();
                _actions.Add(name, actions);
            }
            actions.Add(o => action((T)o));
            action(_viewModel.GetProperty<T>(name));
        }

        public void BindEventToViewHandler<T>(string name, EventHandler<T> handler) where T : EventArgs
        {
            if (!name.StartsWith("event ", StringComparison.Ordinal))
            {
                throw new ArgumentException("To make the code clear to read, the event to bind must be prefixed with 'event'", "name");
            }
            AddEvent(_viewModel, name.Substring("event ".Length), handler);
        }

        public void BindViewEventToAction(Action<EventHandler> subscribe, string name)
        {
            if (!name.EndsWith("()"))
            {
                throw new ArgumentException("To make the code clear to read, the action to bind must be suffixed with '()'", "name");
            }
            Action action = GetAction(_viewModel, name.Substring(0, name.Length - "()".Length));
            subscribe((object sender, EventArgs e) => action());
        }

        public void BindViewEventToAction<TEventArgs>(Action<EventHandler<TEventArgs>> subscribe, string name) where TEventArgs : EventArgs
        {
            Action action = GetAction(_viewModel, name);
            subscribe((object sender, TEventArgs e) => action());
        }

        private static bool HasValueChanged<T>(object me, PropertyInfo pi, T value)
        {
            object o = pi.GetValue(me, null);
            if (o == null)
            {
                return value != null;
            }
            T oldValue = (T)o;
            return !oldValue.Equals(value);
        }

        private static object GetProperty(object me, string name)
        {
            PropertyInfo pi = me.GetType().GetProperty(name);
            return pi.GetValue(me, null);
        }

        private static void AddEvent<T>(object me, string name, EventHandler<T> handler) where T : EventArgs
        {
            EventInfo ei = me.GetType().GetEvent(name);
            ei.AddEventHandler(me, handler);
        }

        private static Action GetAction(object me, string name)
        {
            MethodInfo mi = me.GetType().GetMethod(name);
            Action action = (Action)Delegate.CreateDelegate(typeof(Action), me, mi);
            return action;
        }
    }
}
