using System;
using System.Collections.Generic;

#nullable disable

namespace Galaxxis.Data.Models
{
    public partial class User
    {
        public User()
        {
            Planet = new HashSet<Planet>();
            Ship = new HashSet<Ship>();
        }

        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserFirstname { get; set; }
        public string UserLastname { get; set; }
        public string UserRace { get; set; }

        public virtual ICollection<Planet> Planet { get; set; }
        public virtual ICollection<Ship> Ship { get; set; }
    }
}
