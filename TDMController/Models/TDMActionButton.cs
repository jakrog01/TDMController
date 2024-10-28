using Material.Icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TDMController.Models
{
    public class TDMActionButton(string label, MaterialIconKind icon, ICommand? buttonCommand)
    {
        public string Label { get; } = label;
        public MaterialIconKind Icon { get; } = icon;

        public ICommand? ButtonCommand { get; } = buttonCommand;
    }
}
