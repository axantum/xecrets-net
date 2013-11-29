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

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class ViewModelBase : IViewModel
    {
        private Dictionary<string, object> _items = new Dictionary<string, object>();

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
    }
}
