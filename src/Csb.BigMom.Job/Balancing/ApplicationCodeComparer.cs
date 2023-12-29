using System;
using System.Collections.Generic;

namespace Csb.BigMom.Job.Balancing
{
    public class ApplicationCodeComparer : IComparer<string>
    {
        private Dictionary<string, int> _priorities;

        public ApplicationCodeComparer(Dictionary<string, int> priorities)
        {
            _priorities = priorities;
        }

        public int Compare(string x, string y)
        {
            if (_priorities.TryGetValue(x, out var xPriority) &&
                _priorities.TryGetValue(y, out var yPriority))
            {
                return xPriority == yPriority ? 0 : xPriority < yPriority ? -1 : 1;
            }
            return StringComparer.OrdinalIgnoreCase.Compare(x, y);
        }
    }
}
