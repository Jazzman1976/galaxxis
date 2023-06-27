using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class BuildDefenseSystemAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.BuildDefenseSystem;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetDefenseSystem++;
                planet.ClearCache();
                
                feedback = $"{DateStamp}: Defense System build on planet {planet.PlanetId}.";
                return true;
            }

            feedback = $"Build Defense System Action: no conditions for any change found.";
            return false;
        }
    }
}
