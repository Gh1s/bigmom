using System.Collections.Generic;

namespace Csb.BigMom.Spreading.Jcb.Job
{
    public class JobOptions
    {
        public string JobName { get; set; }

        public IEnumerable<string> SupportedApps { get; set; }

        public string OutFilePathTemplate { get; set; }

        public int OutFileLockCheckIntervalMs { get; set; }
    }
}
