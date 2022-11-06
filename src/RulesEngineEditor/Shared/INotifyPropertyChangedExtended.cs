// Copyright (c) Alex Reich.
// Licensed under the CC BY 4.0 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngineEditor.Shared
{
    public interface INotifyChanged
    {
        event PropertyChangedEventHandlerExtended PropertyChanged;
    }

    public delegate void PropertyChangedEventHandlerExtended(object sender, ChangedEventArgs e);

    public class ChangedEventArgs
    {
        public virtual object OldValue { get; private set; }
        public virtual object NewValue { get; private set; }

        public ChangedEventArgs(object oldValue,
                object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
