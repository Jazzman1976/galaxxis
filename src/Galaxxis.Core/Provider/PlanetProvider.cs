using Galaxxis.Core.Exceptions;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Galaxxis.Core.Provider
{
    public static class PlanetProvider
    {
        /// <summary>
        /// Get a planet with all moons, it's system, galaxy and universe
        /// </summary>
        /// <param name="updateRessources">default: false</param>
        public static Planet GetPlanet(
            int id,
            bool updateRessources = false,
            GalaxxisContext context = null)
        {
            // get planet from context
            Planet planet = null;
            if (context == null)
            {
                // create own context and repeat
                using (GalaxxisContext newContext = new GalaxxisContext())
                {
                    planet = GetPlanet(id, updateRessources, newContext);
                }
            }
            else
            {
                planet = context.Planet
                    // get moons
                    .Include(planet => planet.Moon)
                    // get system
                    .Include(planet => planet.PlanetSystem)
                    // get galaxy
                    .ThenInclude(system => system.SystemGalaxy)
                    // get universe
                    .ThenInclude(galaxy => galaxy.GalaxyUniverse)
                    // return first planet with data
                    .FirstOrDefault(planet => planet.PlanetId.Equals(id));
            }

            // assure planet
            if (planet == null)
            {
                throw new PlanetNotFoundException();
            }

            // update ressources
            if (updateRessources)
            {
                planet.UpdateRessources();
            }

            // exit
            return planet;
        }

        public static List<Planet> GetPlanets(int systemId)
        {
            return null;
        }

        public static Planet FindRandomNobodyPlanetInUniverse(int universeId)
        {
            using (GalaxxisContext context = new GalaxxisContext())
            {
                // get all planets of 'nobody'
                IQueryable<Planet> nobodiesPlantes = context.Planet
                    .Include(p => p.PlanetSystem)
                        .ThenInclude(s => s.SystemGalaxy)
                            .ThenInclude(g => g.GalaxyUniverse)
                    .Where(
                        p => p.PlanetSystem.SystemGalaxy.GalaxyUniverse.UniverseId == universeId
                          && p.PlanetUserId == 2);

                // get a random planet out of these planets
                int countOfPlanets = nobodiesPlantes.Count();
                Random random = new();

                Planet randomPlanet = nobodiesPlantes.ToArray()[random.Next(countOfPlanets)];

                return randomPlanet;
            }
        }

        public static List<Planet> GetPlanetsOfUser(int userId,
            bool includeMoons = false,
            GalaxxisContext context = null)
        {
            // get planets of user
            if (context == null)
            {
                // create own context and repeat
                using (GalaxxisContext newContext = new GalaxxisContext())
                {
                    return GetPlanetsOfUser(userId, includeMoons, newContext);
                }
            }
            IQueryable<Planet> query = context.Planet
                .Where(planet => planet.PlanetUserId == userId);
            if (includeMoons)
            {
                query = query.Include(planet => planet.Moon);
            }
            return query.ToList();
        }

        public static bool AssignPlanetToUser(
            int planetId, int userId, GalaxxisContext context = null)
        {
            // get planet
            Planet planet = GetPlanet(planetId, context: context);
            // check if planet is already assigned to user
            if (planet.PlanetUserId == userId)
            {
                return false; // already assigned
            }
            // assign planet to user
            planet.PlanetUserId = userId;
            // save changes
            if (context == null)
            {
                using (GalaxxisContext newContext = new GalaxxisContext())
                {
                    newContext.Planet.Update(planet);
                    newContext.SaveChanges();
                }
            }
            else
            {
                context.Planet.Update(planet);
                context.SaveChanges();
            }
            return true;
        }
    }
}
