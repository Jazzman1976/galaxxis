using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class Galaxy
    {
        public Galaxy()
        {
            GalaxyToUniverse = new HashSet<GalaxyToUniverse>();
        }

        public int GalaxyId { get; set; }
        public string GalaxyName { get; set; }

        public virtual ICollection<GalaxyToUniverse> GalaxyToUniverse { get; set; }
    }
}
