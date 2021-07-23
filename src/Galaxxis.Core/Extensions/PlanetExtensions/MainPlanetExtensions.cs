using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.RessourceBuildings;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static class MainPlanetExtensions
    {
        /// <summary>
        /// Returns a new OreMine instance
        /// </summary>
        public static OreMine GetOreMine(this Planet planet)
        {
            return new OreMine(planet);
        }

        /// <summary>
        /// Returns a new PowerSupply instance
        /// </summary>
        public static PowerSupply GetPowerSupply(this Planet planet)
        {
            return new PowerSupply(planet);
        }

        /// <summary>
        /// Returns a new Factory instance
        /// </summary>
        public static Factory GetFactory(this Planet planet)
        {
            return new Factory(planet);
        }

        /// <summary>
        /// Returns a new MageAcademy instance
        /// </summary>
        public static MageAcademy GetMageAcademy(this Planet planet)
        {
            return new MageAcademy(planet);
        }

        /// <summary>
        /// Returns a new ChrystalMine instance
        /// </summary>
        public static CrystalMine GetCrystalMine(this Planet planet)
        {
            return new CrystalMine(planet);
        }

        /// <summary>
        /// Returns a new NeutronAccelerator instance
        /// </summary>
        public static NeutronAccelerator GetNeutronAccelerator(this Planet planet)
        {
            return new NeutronAccelerator(planet);
        }

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
        /// Get all events that are queued.
        /// </summary>
        public static List<Event> GetEvents(this Planet planet)
        {
            return EventProvider.GetEvents(planet.PlanetId, EventLocationEnum.Planet);
        }

        /// <summary>
        /// Calculates all ressources on the planet depending on
        /// income and consumption. Does not store calculated planet to db.
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

                // increase data tick
                planet.PlanetDataTick++;
            }
        }
    }
}
