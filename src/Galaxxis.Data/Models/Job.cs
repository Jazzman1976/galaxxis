using System;
using System.Collections.Generic;

#nullable disable

namespace Galaxxis.Data.Models
{
    public partial class Job
    {
        public long JobId { get; set; }
        public int JobUniverseId { get; set; }
        public long JobTick { get; set; }
        public string JobEvent { get; set; }
        public string JobData { get; set; }

        public virtual Universe JobUniverse { get; set; }
    }
}
