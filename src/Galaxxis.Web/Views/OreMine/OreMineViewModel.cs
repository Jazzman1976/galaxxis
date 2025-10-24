namespace Galaxxis.Web.Views.OreMine
{
    public class OreMineViewModel
    {
        public Data.Models.Planet Planet { get; set; }

        public OreMineViewModel(
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
