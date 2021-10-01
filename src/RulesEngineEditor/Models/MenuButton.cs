// Copyright (c) Alex Reich.
// Licensed under the CC BY 4.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngineEditor.Models
{
    public class MenuButton
    {
        public MenuButton (string name, bool enabled = true)
        {
            Name = name;
            Enabled = enabled;
        }
        public string Name {  get; set; }
        public bool Enabled {  get; set; }
    }
}
