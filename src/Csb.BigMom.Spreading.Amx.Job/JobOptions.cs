using System.Collections.Generic;

namespace Csb.BigMom.Spreading.Amx.Job
{
    public class JobOptions
    {
        public string JobName { get; set; }

        public IEnumerable<string> SupportedApps { get; set; }
    }
}
