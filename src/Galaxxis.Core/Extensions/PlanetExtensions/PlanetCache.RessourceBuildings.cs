using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.RessourceBuildings;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static partial class PlanetCache
    {
        /// <summary>
        /// Removes all resource buildings from their dictionaries. 
        /// All data is updated with new data if accessed.
        /// </summary>
        /// <param name="planet"></param>
        public static void RemoveRessourceBuildings(this Planet planet)
        {
            planet.RemoveCrystalMine();
            planet.RemoveElementiumSynthesizer();
            planet.RemoveFactory();
            planet.RemoveMageAcademy();
            planet.RemoveNeutronAccelerator();
            planet.RemoveOreMine();
            planet.RemovePowerSupply();
        }

        #region Ore Mine --------------------------------------------------------------------------

        /// <summary>
        /// Get the planet's ore mine from the dictionary.
        /// </summary>
        public static OreMine GetOreMine(this Planet planet, bool forceReload = false)
        {
            if (!_planetOreMine.ContainsKey(planet.PlanetId) || forceReload)
            {
                _planetOreMine[planet.PlanetId] = new OreMine(planet);
            }
            return _planetOreMine[planet.PlanetId];
        }

        /// <summary>
        /// Removes the instance from the dictionary. So it is updated with new data if accessed.
        /// </summary>
        public static void RemoveOreMine(this Planet planet)
        {
            _planetOreMine.Remove(planet.PlanetId);
        }

        private static readonly Dictionary<int, OreMine> _planetOreMine
            = new Dictionary<int, OreMine>();

        #endregion


        #region Power Supply ----------------------------------------------------------------------

        /// <summary>
        /// Get the planet's power supply from the dictionary.
        /// </summary>
        public static PowerSupply GetPowerSupply(this Planet planet, bool forceReload = false)
        {
            if (!_planetPowerSupply.ContainsKey(planet.PlanetId) || forceReload)
            {
                _planetPowerSupply[planet.PlanetId] = new PowerSupply(planet);
            }
            return _planetPowerSupply[planet.PlanetId];
        }

        /// <summary>
        /// Removes the instance from the dictionary. So it is updated with new data if accessed.
        /// </summary>
        public static void RemovePowerSupply(this Planet planet)
        {
            _planetPowerSupply.Remove(planet.PlanetId);
        }

        private static readonly Dictionary<int, PowerSupply> _planetPowerSupply
            = new Dictionary<int, PowerSupply>();

        #endregion


        #region Factory ---------------------------------------------------------------------------

        /// <summary>
        /// Get the planet's factory from the dictionary.
        /// </summary>
        public static Factory GetFactory(this Planet planet, bool forceReload = false)
        {
            if (!_planetFactory.ContainsKey(planet.PlanetId) || forceReload)
            {
                _planetFactory[planet.PlanetId] = new Factory(planet);
            }
            return _planetFactory[planet.PlanetId];
        }

        /// <summary>
        /// Removes the instance from the dictionary. So it is updated with new data if accessed.
        /// </summary>
        public static void RemoveFactory(this Planet planet)
        {
            _planetFactory.Remove(planet.PlanetId);
        }

        private static readonly Dictionary<int, Factory> _planetFactory
            = new Dictionary<int, Factory>();

        #endregion


        #region Mage Academy ----------------------------------------------------------------------

        /// <summary>
        /// Get the planet's mage academy from the dictionary.
        /// </summary>
        public static MageAcademy GetMageAcademy(this Planet planet, bool forceReload = false)
        {
            if (!_planetMageAcademy.ContainsKey(planet.PlanetId) || forceReload)
            {
                _planetMageAcademy[planet.PlanetId] = new MageAcademy(planet);
            }
            return _planetMageAcademy[planet.PlanetId];
        }

        /// <summary>
        /// Removes the instance from the dictionary. So it is updated with new data if accessed.
        /// </summary>
        public static void RemoveMageAcademy(this Planet planet)
        {
            _planetMageAcademy.Remove(planet.PlanetId);
        }

        private static readonly Dictionary<int, MageAcademy> _planetMageAcademy
            = new Dictionary<int, MageAcademy>();

        #endregion


        #region Crystal Mine ----------------------------------------------------------------------

        /// <summary>
        /// Get the planet's crystal mine from the dictionary.
        /// </summary>
        public static CrystalMine GetCrystalMine(this Planet planet, bool forceReload = false)
        {
            if (!_planetCrystalMine.ContainsKey(planet.PlanetId) || forceReload)
            {
                _planetCrystalMine[planet.PlanetId] = new CrystalMine(planet);
            }
            return _planetCrystalMine[planet.PlanetId];
        }

        /// <summary>
        /// Removes the instance from the dictionary. So it is updated with new data if accessed.
        /// </summary>
        public static void RemoveCrystalMine(this Planet planet)
        {
            _planetCrystalMine.Remove(planet.PlanetId);
        }

        private static readonly Dictionary<int, CrystalMine> _planetCrystalMine
            = new Dictionary<int, CrystalMine>();

        #endregion


        #region Neutron Accelerator ---------------------------------------------------------------

        /// <summary>
        /// Get the planet's neutron accelerator from the dictionary.
        /// </summary>
        public static NeutronAccelerator GetNeutronAccelerator(
            this Planet planet, bool forceReload = false)
        {
            if (!_planetNeutronAccelerator.ContainsKey(planet.PlanetId) || forceReload)
            {
                _planetNeutronAccelerator[planet.PlanetId] = new NeutronAccelerator(planet);
            }
            return _planetNeutronAccelerator[planet.PlanetId];
        }

        /// <summary>
        /// Removes the instance from the dictionary. So it is updated with new data if accessed.
        /// </summary>
        public static void RemoveNeutronAccelerator(this Planet planet)
        {
            _planetNeutronAccelerator.Remove(planet.PlanetId);
        }

        private static readonly Dictionary<int, NeutronAccelerator> _planetNeutronAccelerator
            = new Dictionary<int, NeutronAccelerator>();

        #endregion


        #region Elementium Synthesizer ------------------------------------------------------------

        /// <summary>
        /// Get the planet's elementium synthesizer from the dictionary.
        /// </summary>
        public static ElementiumSynthesizer GetElementiumSynthesizer(
            this Planet planet, bool forceReload = false)
        {
            if (!_planetElementiumSynthesizer.ContainsKey(planet.PlanetId) || forceReload)
            {
                _planetElementiumSynthesizer[planet.PlanetId] = new ElementiumSynthesizer(planet);
            }
            return _planetElementiumSynthesizer[planet.PlanetId];
        }

        /// <summary>
        /// Removes the instance from the dictionary. So it is updated with new data if accessed.
        /// </summary>
        public static void RemoveElementiumSynthesizer(this Planet planet)
        {
            _planetElementiumSynthesizer.Remove(planet.PlanetId);
        }

        private static readonly Dictionary<int, ElementiumSynthesizer> _planetElementiumSynthesizer
            = new Dictionary<int, ElementiumSynthesizer>();

        #endregion


        #region Other -----------------------------------------------------------------------------

        /// <summary>
        /// Get amount of ticks gone since last UpdateRessources
        /// </summary>
        public static long GetTicksGone(this Planet planet)
        {
            Planet thePlanet = PlanetProvider.GetPlanet(planet.PlanetId);
            long ticksGone
                = Ticker.Instance.Ticks
                - thePlanet.PlanetDataTick;
            return ticksGone;
        }

        /// <summary>
        /// Checks if planet is currently building anything
        /// </summary>
        public static bool IsBuilding(this Planet planet)
        {
            return EventProvider
                .GetEvents(
                    planet.PlanetId,
                    EventLocationEnum.Planet,
                    EventTypeEnum.Build)
                .Any();
        }

        /// <summary>
        /// Get all events that are queued for this planet.
        /// </summary>
        public static List<Event> GetEvents(this Planet planet)
        {
            return EventProvider.GetEvents(planet.PlanetId, EventLocationEnum.Planet);
        }

        /// <summary>
        /// Calculates all ressources on the planet depending on
        /// income and consumption. Does not store calculated planet to db. <br/>
        /// ! Heavy performance costs !
        /// </summary>
        public static void UpdateRessources(this Planet planet)
        {
            // ticks gone since last update
            long ticksGone = planet.GetTicksGone();

            // for each tick process growth of ressources
            for (long tickstart = ticksGone; tickstart > 0; tickstart--)
            {
                #region Ore -----------------------------------------------------------------------
                {
                    // set calculated stock
                    double stock = planet.PlanetOre + planet.GetOreMine().AmountPerTick;
                    if (stock >= 0)
                    {
                        planet.PlanetOre = stock;
                    }
                }
                #endregion

                #region Energy --------------------------------------------------------------------
                {
                    // set calculated stock
                    double stock = planet.PlanetEnergy + planet.GetPowerSupply().AmountPerTick;
                    if (stock >= 0)
                    {
                        planet.PlanetEnergy = stock;
                    }
                }
                #endregion

                #region Tools ---------------------------------------------------------------------
                {
                    // set calculated stock
                    double stock = planet.PlanetTools + planet.GetFactory().AmountPerTick;
                    if (stock >= 0)
                    {
                        planet.PlanetTools = stock;
                    }
                }
                #endregion

                #region Mana ----------------------------------------------------------------------
                {
                    // set calculated stock
                    double stock = planet.PlanetMana + planet.GetMageAcademy().AmountPerTick;
                    if (stock >= 0)
                    {
                        planet.PlanetMana = stock;
                    }
                }
                #endregion

                #region Chrystals -----------------------------------------------------------------
                {
                    // set calculated stock
                    double stock = planet.PlanetCrystals + planet.GetCrystalMine().AmountPerTick;
                    if (stock >= 0)
                    {
                        planet.PlanetCrystals = stock;
                    }
                }
                #endregion

                #region Antimatter ----------------------------------------------------------------
                {
                    // set calculated stock
                    double stock
                        = planet.PlanetAntimatter + planet.GetNeutronAccelerator().AmountPerTick;
                    if (stock >= 0)
                    {
                        planet.PlanetAntimatter = stock;
                    }
                }
                #endregion

                #region Elementium ----------------------------------------------------------------
                {
                    // set calculated stock
                    double stock
                        = planet.PlanetElementium
                        + planet.GetElementiumSynthesizer().AmountPerTick;
                    if (stock >= 0)
                    {
                        planet.PlanetElementium = stock;
                    }
                }
                #endregion

                // increase data tick
                planet.PlanetDataTick++;
            }
        }

        #endregion
    }
}
