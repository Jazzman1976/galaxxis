using Galaxxis.Core.Provider;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxxis.Data.Events;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static partial class PlanetCommands
    {
        public static string Attack(this Planet planet, List<Ship> fleet, int planetIdTarget)
        {
            MoveEventTypeData data = new MoveEventTypeData(
                EventLocationEnum.Planet,
                planet.PlanetId,
                EventLocationEnum.Planet,
                planetIdTarget,
                fleet.Select(ship => ship.ShipId).ToArray());

            AttackEvent attackEvent = new AttackEvent(
                EventLocationEnum.Planet,
                planet.PlanetId,
                100,
                data);

            Processor.Instance.EventQueue.Enqueue(attackEvent);

            return "attack committed";
        }
    }
}
