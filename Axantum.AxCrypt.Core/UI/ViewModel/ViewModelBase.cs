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
using System.Reflection;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class ViewModelBase : IViewModel
    {
        private Dictionary<string, List<Action<object>>> _actions = new Dictionary<string, List<Action<object>>>();

        private Dictionary<string, object> _items = new Dictionary<string, object>();

        public ViewModelBase()
        {
            PropertyChanged += HandlePropertyChanged;
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            object value = GetProperty(sender, e.PropertyName);
            List<Action<object>> actions;
            if (!_actions.TryGetValue(e.PropertyName, out actions))
            {
                return;
            }
            foreach (Action<object> action in actions)
            {
                action(value);
            }
        }

        protected void SetProperty<T>(string name, T value)
        {
            if (!HasValueChanged(GetProperty<T>(name), value))
            {
                return;
            }
            _items[name] = value;
            OnPropertyChanged(new PropertyChangedEventArgs(name));
        }

        private bool HasValueChanged<T>(object o, T value)
        {
            if (o == null)
            {
                return value != null;
            }
            T oldValue = (T)o;
            return !oldValue.Equals(value);
        }

        public T GetProperty<T>(string name)
        {
            object value;
            _items.TryGetValue(name, out value);
            if (value == null)
            {
                return default(T);
            }
            return (T)value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void BindPropertyChanged<T>(string name, Action<T> action)
        {
            List<Action<object>> actions;
            if (!_actions.TryGetValue(name, out actions))
            {
                actions = new List<Action<object>>();
                _actions.Add(name, actions);
            }
            actions.Add(o => action((T)o));
            action(GetProperty<T>(name));
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

        public string Error
        {
            get
            {
                IEnumerable<PropertyDescriptor> properties = TypeDescriptor.GetProperties(GetType()).Cast<PropertyDescriptor>();

                return
                    (from property in properties
                     let error = this[property.Name]
                     where !String.IsNullOrEmpty(error)
                     select error)
                        .Aggregate(new StringBuilder(), (acc, next) => acc.Append(acc.Length > 0 ? " " : String.Empty).Append(next))
                        .ToString();
            }
        }

        public virtual string this[string propertyName]
        {
            get
            {
                return String.Empty;
            }
        }
    }
}