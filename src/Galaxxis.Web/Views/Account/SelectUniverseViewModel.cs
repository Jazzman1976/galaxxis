using System.Collections.Generic;

namespace Galaxxis.Web.Views.Account
{
    public class SelectUniverseViewModel
    {
        public string UserEmail { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }

        public List<Data.Models.Universe> Universes { get; set; }
        public int SelectedUniverseId { get; set; }
    }
}
