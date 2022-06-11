using Galaxxis.Core.Provider;
using System.Collections.Generic;

namespace Galaxxis.Web.Views.Universe
{
    public class UniverseViewmodel
    {
        public Data.Models.Universe Universe { get; }

        public UniverseViewmodel()
        {
            this.Universe = UniverseProvider.GetFirstUniverse();
        }
    }
}
