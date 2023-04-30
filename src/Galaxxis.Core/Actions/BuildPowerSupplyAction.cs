using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class BuildPowerSupplyAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.BuildPowerSupply;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetPowerSupply++;
                planet.ClearCache();
                
                feedback = $"{DateStamp}: Power Supply build on planet {planet.PlanetId}.";
                return true;
            }

            feedback = $"Build Power Supply  Action: no conditions for any change found.";
            return false;
        }
    }
}
