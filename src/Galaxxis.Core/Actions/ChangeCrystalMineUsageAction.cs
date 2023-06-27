using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class ChangeCrystalMineUsageAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.ChangeCrystalMineUsage;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetCrystalMineUsage
                    = new ChangeUsageEventTypeData(@event.EventData).NewValue;
                planet.ClearCache();

                feedback = $"{DateStamp}: Crystal Mine usage changed on planet {planet.PlanetId}.";
                return true;
            }

            feedback = $"Change Crystal Mine Usage Action: no conditions for any change found.";
            return false;
        }
    }
}
