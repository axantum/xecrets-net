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

using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

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

        public int ValidationError { get { return GetProperty<int>(nameof(ValidationError)); } set { SetProperty(nameof(ValidationError), value); } }

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

        private static bool HasValueChanged<T>(object o, T value)
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
            Action<T> actionUi = (T arg) => Resolve.UIThread.SendTo(() => action((T)arg));
            BindPropertyChangedInternal<T>(name, actionUi);
            actionUi(GetProperty<T>(name));
        }

        public void BindPropertyAsyncChanged<T>(string name, Func<T, Task> action)
        {
            Action<T> actionUi = (T arg) => Resolve.UIThread.SendToAsync(async () => await action((T)arg));
            BindPropertyChangedInternal<T>(name, actionUi);
            actionUi(GetProperty<T>(name));
        }

        protected void BindPropertyChangedInternal<T>(string name, Action<T> action)
        {
            PropertyInfo pi = GetType().GetRuntimeProperty(name);
            if (pi.PropertyType != typeof(T))
            {
                throw new InvalidOperationException($"Binding to property {name} with type {pi.PropertyType} using mismatching type {typeof(T)} is not allowed.");
            }

            List<Action<object>> actions;
            if (!_actions.TryGetValue(name, out actions))
            {
                actions = new List<Action<object>>();
                _actions.Add(name, actions);
            }
            actions.Add(arg => action((T)arg));
        }

        private static object GetProperty(object me, string name)
        {
            PropertyInfo pi = me.GetType().GetRuntimeProperty(name);
            if (pi == null)
            {
                throw new InvalidOperationException("No property named '{0}' was found. Probably an error in the name argument to GetProperty() or SetProperty().".InvariantFormat(name));
            }
            return pi.GetValue(me, null);
        }

        public string Error
        {
            get
            {
                IEnumerable<string> propertyNames = GetType().GetRuntimeProperties().Select(pi => pi.Name).Where(s => s != "Item" && s != "ValidationError" && s != "Error");

                return
                    (from name in propertyNames
                     let error = this[name]
                     where !String.IsNullOrEmpty(error)
                     select error)
                        .Aggregate(new StringBuilder(), (acc, next) => acc.Append(acc.Length > 0 ? " " : String.Empty).Append(next))
                        .ToString();
            }
        }

        public virtual string this[string columnName]
        {
            get
            {
                if (GetType().GetRuntimeProperty(columnName) == null)
                {
                    throw new ArgumentException("Non-existing property name.", columnName);
                }

                bool isValid;

                try
                {
                    isValid = Task.Run(async () => await ValidateAsync(columnName)).Result;
                }
                catch (AggregateException aex)
                {
                    Exception innerException = aex.InnerExceptions.First();
                    ExceptionDispatchInfo.Capture(innerException).Throw();
                    return string.Empty;
                }

                return isValid ? String.Empty : ValidationError.ToString(CultureInfo.InvariantCulture);
            }
        }

        protected virtual Task<bool> ValidateAsync(string columnName)
        {
            return Task.FromResult(true);
        }
    }
}