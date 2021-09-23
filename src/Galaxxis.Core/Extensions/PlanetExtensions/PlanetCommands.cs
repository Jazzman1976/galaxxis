using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static partial class PlanetCommands
    {
        public static string Attack(this Planet planet, List<Ship> fleet, int planetIdTarget)
        {
            Event attackEvent = EventProvider.NewEvent(
                EventLocationEnum.Planet,
                planet.PlanetId,
                EventNameEnum.Attack,
                100,
                EventTypeEnum.Move,
                $"attacking|{planetIdTarget}|{string.Join(";", fleet.Select(ship => ship.ShipId.ToString()))}");

            Processor.Instance.EventQueue.Enqueue(attackEvent);

            return "attack committed";
        }
    }
}
