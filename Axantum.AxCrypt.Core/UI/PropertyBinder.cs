using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Extensions;

namespace Axantum.AxCrypt.Core.UI
{
    public class PropertyBinder
    {
        private Dictionary<string, List<Action<object>>> _actions = new Dictionary<string, List<Action<object>>>();

        private INotify _notify;

        public PropertyBinder(INotify notify)
        {
            _notify = notify;
            notify.PropertyChanged += HandlePropertyChanged;
        }

        public void Notify()
        {
            _notify.Notify();
        }

        void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            object value = sender.GetProperty(e.PropertyName);
            foreach (Action<object> action in _actions[e.PropertyName])
            {
                action(value);
            }
        }

        public void Bind<T>(string name, Action<T> action)
        {
            List<Action<object>> actions;
            if (!_actions.TryGetValue(name, out actions))
            {
                actions = new List<Action<object>>();
                _actions.Add(name, actions);
            }
            actions.Add(o => action((T)o));
        }
    }
}
