using Galaxxis.Data.Enums;
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
            string input = "";
            while (input != "exit")
            {
                Console.WriteLine("Welcome to Galaxxis Universe Creator!");
                Console.WriteLine("Please choose an action:");
                Console.WriteLine("[a] = create admin user");
                Console.WriteLine("[n] = create nobody user");
                Console.WriteLine("[u] = create usiverse");
                Console.WriteLine("[g] = create galaxies");
                Console.WriteLine("[s] = create systems");
                Console.WriteLine("[p] = create planets");
                Console.WriteLine("[m] = create moons");
                Console.WriteLine("Press any other key to exit.");
                input = Console.ReadKey(true).KeyChar.ToString();
                Console.WriteLine("");
                Console.WriteLine($"You chose: '{input}', please wait...");
                Console.WriteLine("");

                switch (input)
                {
                    case "a":
                    case "A":
                        CreateAdmin();
                        break;
                    case "n":
                    case "N":
                        CreateNobody();
                        break;
                    case "u":
                    case "U":
                        CreateUniverse();
                        break;
                    case "g":
                    case "G":
                        CreateGalaxies();
                        break;
                    case "s":
                    case "S":
                        CreateSystems();
                        break;
                    case "p":
                    case "P":
                        CreatePlanets();
                        break;
                    case "m":
                    case "M":
                        CreateMoons();
                        break;
                    default:
                        input = "exit";
                        break;
                }
            }
        }

        private static void CreateAdmin()
        {
            Console.WriteLine("Please enter the firstname of the admin user:");
            string adminFirstname = Console.ReadLine();
            if (string.IsNullOrEmpty(adminFirstname))
            {
                Console.WriteLine("Admin name cannot be empty!");
                return;
            }
            Console.WriteLine("Please enter the lastname of the admin user:");
            string adminLastname = Console.ReadLine();
            if (string.IsNullOrEmpty(adminLastname))
            {
                Console.WriteLine("Admin lastname cannot be empty!");
                return;
            }
            Console.WriteLine("Please enter the email of the admin user:");
            string adminEmail = Console.ReadLine();
            if (string.IsNullOrEmpty(adminEmail))
            {
                Console.WriteLine("Admin email cannot be empty!");
                return;
            }
            Console.WriteLine("Please select the race of the admin user:");
            Console.WriteLine("[h] = Human");
            Console.WriteLine("[e] = Elf");
            Console.WriteLine("[d] = Dwarf");
            Console.WriteLine("[o] = Orc");
            Console.WriteLine("[t] = Troll");
            Console.WriteLine("Press any other key to exit.");
            string input = Console.ReadKey(true).KeyChar.ToString();
            Console.WriteLine("");
            Console.WriteLine($"You chose: '{input}', please wait...");
            Console.WriteLine("");

            switch (input)
            {
                case "h":
                case "H":
                    input = "Human";
                    break;
                case "e":
                case "E":
                    input = "Elf";
                    break;
                case "d":
                case "D":
                    input = "Dwarf";
                    break;
                case "o":
                case "O":
                    input = "Orc";
                    break;
                case "t":
                case "T":
                    input = "Troll";
                    break;
                default:
                    return; // exit
            }

            using (GalaxxisContext context = new GalaxxisContext())
            {
                // check if admin user already exists
                User existingAdmin = context.User.FirstOrDefault(u => u.UserId == 1);
                if (existingAdmin != null)
                {
                    Console.WriteLine("Admin user already exists! Please delete the existing admin user first.");
                }

                // create new admin user
                User adminUser = new User()
                {
                    UserFirstname = adminFirstname,
                    UserLastname = adminLastname,
                    UserEmail = adminEmail,
                    UserRace = input
                };

                context.User.Add(adminUser);
                try
                {
                    context.SaveChanges();
                    Console.WriteLine("Admin user created successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the admin user: {ex.Message}");
                }
            }

            return;
        }

        private static void CreateNobody()
        {
            using (GalaxxisContext context = new GalaxxisContext())
            {
                // check if nobody user already exists
                User existingNobody = context.User.FirstOrDefault(u => u.UserId == 2);
                if (existingNobody != null)
                {
                    Console.WriteLine("Nobody user already exists! Please delete the existing nobody user first.");
                    return;
                }
                // create new nobody user
                User nobodyUser = new User()
                {
                    UserFirstname = "Nobody",
                    UserLastname = "Nobody",
                    UserEmail = "nobody@example.com",
                    UserRace = "Human"
                };

                try
                {
                    context.User.Add(nobodyUser);
                    context.SaveChanges();
                    Console.WriteLine("Nobody user created successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the nobody user: {ex.Message}");
                }
            }
        }

        private static void CreateUniverse()
        {
            // create a new universe without any galaxies, systems, planets or moons but contain all the necessary tables and references to other tables

            using (GalaxxisContext context = new GalaxxisContext())
            {
                Universe universe = new Universe
                {
                    UniverseName = "Galaxxis Universe",
                    UniverseTimePerTick = 1000, // 1 second per tick
                    UniverseTicks = 0 // start with 0 ticks
                };
                context.Universe.Add(universe);
                context.SaveChanges();
            }

            Console.WriteLine("Universe created.");
            Console.WriteLine("You can now create galaxies, systems, planets and moons in this universe.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.WriteLine("");

            return;
        }

        private static void CreateGalaxies()
        {
            // step 1: create 3D coordinates for galaxies
            bool successGalaxies = UniverseCreator.Create3D(2, 10, 10, out List<int[]> galaxyCoords);
            if (!successGalaxies)
            {
                Console.WriteLine("Creation failed in step 1!");
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
                    Console.WriteLine("Creation failed! Universe required!");
                    return;
                }

                //// step 3.1: add first coords to existing galaxy
                //universe.Galaxy.First().GalaxyCoordinateX = galaxyCoords[0][0];
                //universe.Galaxy.First().GalaxyCoordinateY = galaxyCoords[0][1];
                //universe.Galaxy.First().GalaxyCoordinateZ = galaxyCoords[0][2];
                //galaxyCoords.RemoveAt(0);

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
                
                // save
                context.SaveChanges();
            }

            Console.WriteLine("Galaxies created.");
            Console.WriteLine("You can now create systems, planets and moons in these galaxies.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.WriteLine("");

            return;
        }

        private static void CreateSystems()
        {
            using (GalaxxisContext context = new GalaxxisContext())
            {
                List<Galaxy> galaxyList = context.Galaxy
                    .Include(g => g.System)
                    .ThenInclude(s => s.Planet)
                    .ThenInclude(p => p.Moon)
                    .ToList();

                // add systems
                try
                {
                    foreach (Galaxy galaxy in galaxyList)
                    {
                        // step 1: get coords
                        bool successSystems = UniverseCreator.Create3D(2, 10, 10, out List<int[]> systemCoords);
                        if (!successSystems)
                        {
                            throw new Exception("Creation failed!");
                        }

                        //// step 2.1: add first coords to existing system
                        //if (galaxy.GalaxyId == 1)
                        //{
                        //    galaxy.System.First(s => s.SystemGalaxyId == 1).SystemCoordinateX = systemCoords[0][0];
                        //    galaxy.System.First(s => s.SystemGalaxyId == 1).SystemCoordinateY = systemCoords[0][1];
                        //    galaxy.System.First(s => s.SystemGalaxyId == 1).SystemCoordinateZ = systemCoords[0][2];
                        //    systemCoords.RemoveAt(0);
                        //}

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
                    }

                    // save
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating systems: {ex.Message}");
                    return;
                }    
            }

            Console.WriteLine("Systems created.");
            Console.WriteLine("You can now create planets and moons in these systems.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.WriteLine("");

            return;
        }

        private static void CreatePlanets()
        {
            using (GalaxxisContext context = new GalaxxisContext())
            {
                List<Models.System> planetsList = context.System
                    .Include(s => s.Planet)
                    .ThenInclude(p => p.Moon)
                    .ToList();

                // TODO: create admin, user need a field "is admin", usually ID = 1
                // TODO: create nobody user for planets whithout owner, user need a field "is nobody", usually ID = 2

                // add systems
                try
                {
                    foreach (Models.System system in planetsList)
                    {
                        // step 1: get coords
                        bool successPlanets = UniverseCreator.Create3D(2, 10, 10, out List<int[]> planetCoords);
                        if (!successPlanets)
                        {
                            throw new Exception("Creation failed!");
                        }

                        //// step 2.1: add first coords to existing system
                        //if (system.SystemId == 1)
                        //{
                        //    system.Planet.First(p => p.PlanetSystemId == 1).PlanetCoordinateX = planetCoords[0][0];
                        //    system.Planet.First(p => p.PlanetSystemId == 1).PlanetCoordinateX = planetCoords[0][1];
                        //    system.Planet.First(p => p.PlanetSystemId == 1).PlanetCoordinateX = planetCoords[0][2];
                        //    planetCoords.RemoveAt(0);
                        //}

                        // step 2.2: add other systems
                        foreach (int[] coords in planetCoords)
                        {
                            system.Planet.Add(new Planet
                            {
                                //#warning user id of nobody account!
                                // userId 1 => admin account
                                // userId 2 => nobody account
                                // the nobody account is needed because it is not nullable
                                // if 'nobody' owns a planet etc. the nobody userId is used!
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
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating planets: {ex.Message}");
                    return;
                }

                return;
            }
        }

        private static void CreateMoons()
        {
            /* iterate over all planets and create moons for each planet
             * not all plaets will have moons, so we can use a random number of moons
             * in one of 10 cases there will be a moon
             * in one of 100 cases there will be 2 moons
             */

            using (GalaxxisContext context = new GalaxxisContext())
            {
                List<Planet> planetsList = context.Planet
                    .Include(p => p.Moon)
                    .ToList();
                try
                {
                    foreach (Planet planet in planetsList)
                    {
                        // step 1: get random number of moons
                        Random rnd = new Random();
                        int roll = rnd.Next(1, 100);
                        int moonCount = roll <= 1 ? 2 : roll <= 10 ? 1 : 0; // 1% chance for 2 moons, 10% chance for 1 moon, 89% chance for no moon
                        if (moonCount == 0)
                        {
                            continue; // no moon for this planet
                        }
                        // step 2: create moons
                        for (int i = 0; i < moonCount; i++)
                        {
                            planet.Moon.Add(new Moon
                            {
                                MoonName = $"Moon {i + 1} of {planet.PlanetName}"
                            });
                        }
                    }
                    // save
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating moons: {ex.Message}");
                    return;
                }

                Console.WriteLine("Moons created.");

                return;
            }
        }
    }
}
