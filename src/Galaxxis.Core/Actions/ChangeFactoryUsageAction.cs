using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class ChangeFactoryUsageAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.ChangeFactoryUsage;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetFactoryUsage
                    = new ChangeUsageEventTypeData(@event.EventData).NewValue;
                planet.ClearCache();

                feedback = $"{DateStamp}: Power Supply usage changed on planet {planet.PlanetId}.";
                return true;
            }

            feedback = $"Change Factory Usage Action: no conditions for any change found.";
            return false;
        }
    }
}
