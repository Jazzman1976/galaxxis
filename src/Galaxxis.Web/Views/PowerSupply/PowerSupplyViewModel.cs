namespace Galaxxis.Web.Views.PowerSupply
{
    public class PowerSupplyViewModel
    {
        public Data.Models.Planet Planet { get; set; }
        public Core.Classes.Buildings.RessourceBuildings.PowerSupply PowerSupply { get; }
        public string InfoMessage { get; set; }

        public PowerSupplyViewModel(int planetId, Code.Services.CurrentUserService.ICurrentUserService currentUserService)
        {
            Data.Models.User user = currentUserService.GetGalaxxisUserAsync().GetAwaiter().GetResult();
            
            foreach (Data.Models.Planet p in user.Planet)
            {
                if (p.PlanetId == planetId)
                {
                    this.Planet = p;
                    break;
                }
            }
            if (this.Planet != null)
            {
                this.PowerSupply = new Core.Classes.Buildings.RessourceBuildings.PowerSupply(Planet);
            }
        }
    }
}
