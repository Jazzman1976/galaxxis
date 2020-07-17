using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class SystemToGalaxy
    {
        public int SystemId { get; set; }
        public int GalaxyId { get; set; }

        public virtual Galaxy Galaxy { get; set; }
        public virtual System System { get; set; }
    }
}
