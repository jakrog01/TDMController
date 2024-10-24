using System.Collections.Generic;

namespace TDMController.Models
{
    internal class Series
    {
        public bool TakePhoto { get; init; }
        public bool Measure { get; init; }
        public string ProjectKey { get; init; }
        public List<Sequence> Sequences { get; init; }

    }
}
