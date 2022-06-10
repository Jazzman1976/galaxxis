using Galaxxis.Core.Provider;
using Galaxxis.Data.Models;
using Galaxxis.Web.Code.Identity;
using System.Collections.Generic;

namespace Galaxxis.Web.Views.Universe
{
    public class UniverseViewmodel
    {
        public List<Galaxy> Galaxies { get; }

        public UniverseViewmodel()
        {
            // only 1 universe per server for now! Later more universes.
            this.Galaxies = GalaxyProvider.GetGalaxies(1);
        }
    }
}
