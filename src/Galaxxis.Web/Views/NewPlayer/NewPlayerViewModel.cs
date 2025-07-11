using System.Collections.Generic;
using System.Data;
using DataModels = Galaxxis.Data.Models;

namespace Galaxxis.Web.Views.NewPlayer
{
    public class NewPlayerViewModel
    {
        public List<DataModels.Universe> Universes { get; set; } = new List<DataModels.Universe>();
        public int UniverseId { get; set; } = 0;
        public bool HasAnyPlanet { get; set; } = false;
    }
}
