using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// To update from database open Nuget Manager Console and execute:
/// Scaffold-DbContext "Server=HAWK\SQLEXPRESS;Database=GalaxxisPrivat;User Id=GalaxxisPrivatUser;Password=***REMOVED***;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context GalaxxisContext -force
/// Scaffold-DbContext "Server=Wall-E;Database=Galaxxis;User Id=GalaxxisUser;Password=***REMOVED***;" Microsoft.EntityFrameworkCore.SqlServer -NoPluralize -OutputDir Models -Context GalaxxisContext -force
/// </summary>
namespace Galaxxis.Data
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[g] = create galaxies");
            Console.WriteLine("[s] = create systems");
            Console.WriteLine("[p] = create planets");
            string input = Console.ReadLine();


            #region create galaxies

            if (input == "g" || input == "g")
            {
                // step 1: create 3D coordinates for galaxies
                bool successGalaxies = UniverseCreator.Create3D(2, 10, 10, out List<int[]> galaxyCoords);
                if (!successGalaxies)
                {
                    Console.WriteLine("step 1 creation failed!");
                    return;
                }

                // step 2: get universe
                using (GalaxxisContext context = new GalaxxisContext())
                {
                    Universe universe = context.Universe
                        .Include(u => u.Galaxy)
                        .ThenInclude(g => g.System)
                        .ThenInclude(s => s.Planet)
                        .ThenInclude(p => p.Moon)
                        .First();

                    if (universe == null)
                    {
                        Console.WriteLine("step 2 creation failed!");
                        return;
                    }

                    // step 3.1: add first coords to existing galaxy
                    universe.Galaxy.First().GalaxyCoordinateX = galaxyCoords[0][0];
                    universe.Galaxy.First().GalaxyCoordinateY = galaxyCoords[0][1];
                    universe.Galaxy.First().GalaxyCoordinateZ = galaxyCoords[0][2];
                    galaxyCoords.RemoveAt(0);

                    // step 3.2: add other galaxies
                    foreach (int[] coords in galaxyCoords)
                    {
                        universe.Galaxy.Add(new Galaxy
                        {
                            GalaxyCoordinateX = coords[0],
                            GalaxyCoordinateY = coords[1],
                            GalaxyCoordinateZ = coords[2],
                            GalaxyName = $"Galaxy {coords[0]}.{coords[1]}.{coords[2]}"
                        });
                    }
                    Console.WriteLine("step 3: Galaxies created.");

                    // save
                    context.SaveChanges();
                }
            }

            #endregion


            #region systems

            else if (input == "s" || input == "S")
            {
                using (GalaxxisContext context = new GalaxxisContext())
                {
                    List<Galaxy> galaxyList = context.Galaxy
                        .Include(g => g.System)
                        .ThenInclude(s => s.Planet)
                        .ThenInclude(p => p.Moon)
                        .ToList();

                    // add systems
                    foreach (Galaxy galaxy in galaxyList)
                    {
                        // step 1: get coords
                        bool successSystems = UniverseCreator.Create3D(2, 10, 10, out List<int[]> systemCoords);
                        if (!successSystems)
                        {
                            Console.WriteLine("step 1 creation failed!");
                            return;
                        }

                        // step 2.1: add first coords to existing system
                        if (galaxy.GalaxyId == 1)
                        {
                            galaxy.System.First(s => s.SystemGalaxyId == 1).SystemCoordinateX = systemCoords[0][0];
                            galaxy.System.First(s => s.SystemGalaxyId == 1).SystemCoordinateY = systemCoords[0][1];
                            galaxy.System.First(s => s.SystemGalaxyId == 1).SystemCoordinateZ = systemCoords[0][2];
                            systemCoords.RemoveAt(0);
                        }

                        // step 2.2: add other systems
                        foreach (int[] coords in systemCoords)
                        {


                            galaxy.System.Add(new Models.System
                            {
                                SystemCoordinateX = coords[0],
                                SystemCoordinateY = coords[1],
                                SystemCoordinateZ = coords[2],
                                SystemName = $"System {coords[0]}.{coords[1]}.{coords[2]}"
                            });
                        }
                        Console.WriteLine("step 2: Systems created.");

                        // save
                        context.SaveChanges();
                    }
                }
            }

            #endregion


            #region Planets

            else if (input == "p" || input == "P")
            {
                using (GalaxxisContext context = new GalaxxisContext())
                {
                    List<Models.System> planetsList = context.System
                        .Include(s => s.Planet)
                        .ThenInclude(p => p.Moon)
                        .ToList();

                    // add systems
                    foreach (Models.System system in planetsList)
                    {
                        // step 1: get coords
                        bool successPlanets = UniverseCreator.Create3D(2, 10, 10, out List<int[]> planetCoords);
                        if (!successPlanets)
                        {
                            Console.WriteLine("step 1 creation failed!");
                            return;
                        }

                        // step 2.1: add first coords to existing system
                        if (system.SystemId == 1)
                        {
                            system.Planet.First(p => p.PlanetSystemId == 1).PlanetCoordinateX = planetCoords[0][0];
                            system.Planet.First(p => p.PlanetSystemId == 1).PlanetCoordinateX = planetCoords[0][1];
                            system.Planet.First(p => p.PlanetSystemId == 1).PlanetCoordinateX = planetCoords[0][2];
                            planetCoords.RemoveAt(0);
                        }

                        // step 2.2: add other systems
                        foreach (int[] coords in planetCoords)
                        {
                            system.Planet.Add(new Planet
                            {
                                //#warning user id of nobody account!
                                PlanetUserId = 2,
                                PlanetCoordinateX = coords[0],
                                PlanetCoordinateY = coords[1],
                                PlanetCoordinateZ = coords[2],
                                PlanetName = $"Planet {coords[0]}.{coords[1]}.{coords[2]}"
                            });
                        }
                        Console.WriteLine("step 2: Planets created.");

                        // save
                        context.SaveChanges();
                    }
                }
            }

            #endregion


            else
            {
                Console.WriteLine("programm exited without action.");
            }
        }
    }
}
