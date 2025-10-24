namespace Galaxxis.Web.Views.CrystalMine
{
    public class CrystalMineViewModel
    {
        public Data.Models.Planet Planet { get; set; }

        public CrystalMineViewModel(
            int planetId, 
            Code.Services.CurrentUserService.ICurrentUserService currentUserService)
        {
            Data.Models.User user = currentUserService
                .GetGalaxxisUserAsync()
                .GetAwaiter()
                .GetResult();
            
            foreach (Data.Models.Planet p in user.Planet)
            {
                if (p.PlanetId == planetId)
                {
                    this.Planet = p;
                    break;
                }
            }
        }
    }
}
