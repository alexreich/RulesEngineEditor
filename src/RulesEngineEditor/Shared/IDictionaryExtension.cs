// Copyright (c) Alex Reich.
// Licensed under the CC BY 4.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngineEditor.Shared
{
    public class DictItem: INotifyChanged
    {
        private string _key;
        public string Key { 
            get { return _key; } 
            set {
                KeyValuePair<string, object> oldValue = new KeyValuePair<string, object>(_key, _value);
                _key = value;
                PropertyChanged(this, new ChangedEventArgs(oldValue, 
                    new KeyValuePair<string, object>(_key, _value)));
            } 
        }
        private object _value;
        public object Value {
            get { return _value; }
            set {
                KeyValuePair<string, object> oldValue = new KeyValuePair<string, object>(_key, _value);
                _value = value;
                PropertyChanged(this, new ChangedEventArgs(oldValue,
                    new KeyValuePair<string, object>(_key, _value)));
            }
        }

        public DictItem(string key, object value)
        {
            _key = key;
            _value = value;
        }

        public event PropertyChangedEventHandlerExtended PropertyChanged;
    }

    public static class IDictionaryExtension
    {
        public static ObservableCollection<DictItem> ToObservableCollection(this IDictionary<string, object> dict,
            Action<DictItem, ChangedEventArgs> itemChangeAction,
            Action<ObservableCollection<DictItem>, NotifyCollectionChangedEventArgs> collectionChangeAction)
        {
            var result = new ObservableCollection<DictItem>();
            foreach (var kvp in dict)
            {
                var dictItem = new DictItem(kvp.Key, kvp.Value);
                dictItem.PropertyChanged += (o, e) => itemChangeAction((DictItem)o, e);
                result.Add(dictItem);
            }

            result.CollectionChanged += (c, e) => collectionChangeAction((ObservableCollection<DictItem>)c, e);
            return result;
        }
    }
}
