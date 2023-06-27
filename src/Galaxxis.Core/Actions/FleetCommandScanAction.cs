using Galaxxis.Core.Engine;
using Galaxxis.Core.Provider;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Actions
{
    internal class FleetCommandScanAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.FleetCommandScan;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                // scan target planet
                ScanEventTypeData data = new ScanEventTypeData(@event.EventData);
                Planet planet = PlanetProvider.GetPlanet(data.ScanLocationId, true, context);
                data.ScannedOre = planet.PlanetOre;
                data.ScannedEnergy = planet.PlanetEnergy;
                data.ScannedTools = planet.PlanetTools;
                data.ScannedMana = planet.PlanetMana;
                data.ScannedCrystals = planet.PlanetCrystals;
                data.ScannedAntimatter = planet.PlanetAntimatter;
                data.ScannedElementium = planet.PlanetElementium;

                // log scan result
                context.EventLog.Add(new EventLog(@event, data.GetXml));

                // enqueue return event
                MoveEventTypeData moveData = new MoveEventTypeData(
                    data.ScanLocation, data.ScanLocationId, // from
                    data.FromLocation, data.FromLocationId, // to
                    data.ScanningShipIds);
                Event returnEvent = new FleetCommandMoveEvent(
                    EventLocationEnum.Planet, @event.EventLocationId,
                    100,
                    moveData);
                Processor.Instance.EventQueue.Enqueue(returnEvent);

                feedback = $"{DateStamp}:Fleet Command Scan: planet {planet.PlanetId} scanned.";
                return true;
            }

            feedback = $"Fleet Command Scan: no conditions for any change found.";
            return false;
        }
    }
}
