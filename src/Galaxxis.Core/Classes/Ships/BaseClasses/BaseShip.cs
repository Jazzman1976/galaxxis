using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Ships.Interfaces;
using Galaxxis.Core.Extensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxxis.Core.Classes.Ships.BaseClasses
{
    public abstract class BaseShip : IShip
    {
        public int PlanetId { get; }
        public ShipTypeEnum ShipType { get; }
        public TacticalEnum Tactical { get; }

        public abstract int CostTools { get; }
        public abstract int CostCrystals { get; }
        public abstract int CostAntimatter { get; }
        public abstract int CostElementium { get; }

        public BaseShip(int planetId, ShipTypeEnum shipType, TacticalEnum tactical)
        {
            this.PlanetId = planetId;
            this.ShipType = shipType;
            this.Tactical = tactical;
        }
    }
}
