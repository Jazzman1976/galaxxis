using Galaxxis.Core.Classes.Buildings.ProducingBuildings;
using Galaxxis.Data.Models;
using System.Collections.Generic;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static partial class PlanetCache
    {
        public static void RemoveProducingBuildings(this Planet planet)
        {
            planet.RemoveShipyard();
            planet.RemoveDefenseSystem();
        }

        #region Shipyard --------------------------------------------------------------------------

        private static readonly Dictionary<int, Shipyard> _planetShipyard
            = new Dictionary<int, Shipyard>();

        public static Shipyard GetShipyard(this Planet planet, bool forceReload = false)
        {
            if (!_planetShipyard.ContainsKey(planet.PlanetId) || forceReload)
            {
                _planetShipyard[planet.PlanetId] = new Shipyard(planet);
            }
            return _planetShipyard[planet.PlanetId];
        }

        public static void RemoveShipyard(this Planet planet)
        {
            _planetShipyard.Remove(planet.PlanetId);
        }

        #endregion

        #region Defense System --------------------------------------------------------------------

        private static readonly Dictionary<int, DefenseSystem> _planetDefenseSystem
            = new Dictionary<int, DefenseSystem>();

        public static DefenseSystem GetDefenseSystem(this Planet planet, bool forceReload = false)
        {
            if (!_planetDefenseSystem.ContainsKey(planet.PlanetId) || forceReload)
            {
                _planetDefenseSystem[planet.PlanetId] = new DefenseSystem(planet);
            }
            return _planetDefenseSystem[planet.PlanetId];
        }

        public static void RemoveDefenseSystem(this Planet planet)
        {
            _planetDefenseSystem.Remove(planet.PlanetId);
        }

        #endregion
    }
}
