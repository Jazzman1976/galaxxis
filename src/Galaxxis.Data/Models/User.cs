using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class User
    {
        public User()
        {
            PlanetToUser = new HashSet<PlanetToUser>();
        }

        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserFirstname { get; set; }
        public string UserLastname { get; set; }

        public virtual ICollection<PlanetToUser> PlanetToUser { get; set; }
    }
}
