using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMController.Models
{
    internal class Sequence
    {
        public int Repeat { get; init; }
        public List<int> Commands { get; init; }
        public int ActionPerStep { get; init; }
    }
}
