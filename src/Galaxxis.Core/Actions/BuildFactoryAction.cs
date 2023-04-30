using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class BuildFactoryAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.BuildFactory;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetFactory++;
                planet.ClearCache();
                
                feedback = $"{DateStamp}: Factory build on planet {planet.PlanetId}.";
                return true;
            }

            feedback = $"Build Factory Action: no conditions for any change found.";
            return false;
        }
    }
}
