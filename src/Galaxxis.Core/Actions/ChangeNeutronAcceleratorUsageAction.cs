using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class ChangeNeutronAcceleratorUsageAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.ChangeNeutronAcceleratorUsage;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetNeutronAcceleratorUsage
                    = new ChangeUsageEventTypeData(@event.EventData).NewValue;
                planet.ClearCache();

                feedback = 
                    $"{DateStamp}: Neutron Accelerator usage changed on planet {planet.PlanetId}.";
                return true;
            }

            feedback = 
                $"Change Neutron Accelerator Usage Action: no conditions for any change found.";
            return false;
        }
    }
}
