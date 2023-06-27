using Galaxxis.Core.Provider;
using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Actions
{
    internal class FleetCommandMoveAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.FleetCommandMove;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                // local variables
                MoveEventTypeData data = new MoveEventTypeData(@event.EventData);
                Planet planet = PlanetProvider.GetPlanet(data.ToLocationId, false, context);

                // add ressources
                planet.PlanetOre += data.Ore;
                planet.PlanetEnergy += data.Energy;
                planet.PlanetTools += data.Tools;
                planet.PlanetMana += data.Mana;
                planet.PlanetCrystals += data.Crystals;
                planet.PlanetAntimatter += data.Antimatter;
                planet.PlanetElementium += data.Elementium;

                // log event
                context.EventLog.Add(new EventLog(@event, data.GetXml));

                feedback = $"{DateStamp}:Fleet Command Move: moved to planet {planet.PlanetId}.";
                return true;
            }

            feedback = $"Fleet Command Move: no conditions for any change found.";
            return false;
        }
    }
}
