using Galaxxis.Core.Provider;

namespace Galaxxis.Web.Views.Galaxy
{
    public class GalaxyViewmodel
    {
        public Data.Models.Galaxy Galaxy { get; }

        public GalaxyViewmodel(int galaxyId)
        {
            this.Galaxy = GalaxyProvider.GetGalaxy(galaxyId);
        }
    }
}
