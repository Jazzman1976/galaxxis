using Galaxxis.Core.Extensions;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;
using System.Diagnostics;
using System.Linq;

namespace Galaxxis.Core.Engine
{
    public sealed partial class Processor
    {
        #region Do Event Action Switch ------------------------------------------------------------

        /// <summary>
        /// Main definitions of all code to be processed for every possible event.
        /// </summary>
        private void Do_EventAction(Event @event, GalaxxisContext context)
        {
            switch (Enum.Parse(typeof(EventNameEnum), @event.EventName))
            {
                #region Build event actions -------------------------------------------------------

                case EventNameEnum.BuildOreMine:
                    Do_BuildOreMine(@event, context);
                    break;

                case EventNameEnum.BuildPowerSupply:
                    Do_BuildPowerSupply(@event, context);
                    break;

                case EventNameEnum.BuildFactory:
                    Do_BuildFactory(@event, context);
                    break;

                case EventNameEnum.BuildMageAcademy:
                    Do_BuildMageAcademy(@event, context);
                    break;

                case EventNameEnum.BuildCrystalMine:
                    Do_BuildCrystalMine(@event, context);
                    break;

                case EventNameEnum.BuildNeutronAccelerator:
                    Do_BuildNeutronAccelerator(@event, context);
                    break;

                case EventNameEnum.BuildElementiumSynthesizer:
                    Do_BuildElementiumSynthesizer(@event, context);
                    break;

                case EventNameEnum.BuildShipyard:
                    Do_BuildShipyard(@event, context);
                    break;

                #endregion

                #region Change event actions ------------------------------------------------------

                case EventNameEnum.ChangeOreMineUsage:
                    Do_ChangeOreMineUsage(@event, context);
                    break;

                case EventNameEnum.ChangePowerSupplyUsage:
                    Do_ChangePowerSupplyUsage(@event, context);
                    break;

                case EventNameEnum.ChangeFactoryUsage:
                    Do_ChangeFactoryUsage(@event, context);
                    break;

                case EventNameEnum.ChangeMageAcademyUsage:
                    Do_ChangeMageAcademyUsage(@event, context);
                    break;

                case EventNameEnum.ChangeCrystalMineUsage:
                    Do_ChangeCrystalMineUsage(@event, context);
                    break;

                case EventNameEnum.ChangeNeutronAcceleratorUsage:
                    Do_ChangeNeutronAcceleratorUsage(@event, context);
                    break;

                case EventNameEnum.ChangeElementiumSynthesizerUsage:
                    Do_ChangeElementiumSynthesizerUsage(@event, context);
                    break;

                #endregion

                case EventNameEnum.ProduceShipExplorer:
                    Do_ProduceShipExplorer(@event, context);
                    break;

                case EventNameEnum.ProduceShipCarrier:
                    Do_ProduceShipCarrier(@event, context);
                    break;
            }
        }

        #endregion


        #region Do Build Event Actions ------------------------------------------------------------

        private void Do_BuildOreMine(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetOreMine++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Ore Mine build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildPowerSupply(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetPowerSupply++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Power Supply build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildFactory(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetFactory++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Factory build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildMageAcademy(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetMageAcademy++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Mage Academy build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildCrystalMine(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetCrystalMine++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Chrystal Mine build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildNeutronAccelerator(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetNeutronAccelerator++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Neutron Accelerator build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildElementiumSynthesizer(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetElementiumSynthesizer++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Elementium Synthesizer build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildShipyard(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetShipyard++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Shipyard build on planet {planet.PlanetId}.");
            }
        }

        #endregion --------------------------------------------------------------------------------


        #region Do Change Event Actions -----------------------------------------------------------

        private void Do_ChangeOreMineUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                double newValue = double.Parse(@event.EventData);
                planet.PlanetOreMineUsage = newValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Ore Mine usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangePowerSupplyUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                double newValue = double.Parse(@event.EventData);
                planet.PlanetPowerSupplyUsage = newValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Power Supply usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangeFactoryUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                double newValue = double.Parse(@event.EventData);
                planet.PlanetFactoryUsage = newValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Power Supply usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangeMageAcademyUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                double newValue = double.Parse(@event.EventData);
                planet.PlanetMageAcademyUsage = newValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Mage Academy usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangeCrystalMineUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                double newValue = double.Parse(@event.EventData);
                planet.PlanetCrystalMineUsage = newValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Crystal Mine usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangeNeutronAcceleratorUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                double newValue = double.Parse(@event.EventData);
                planet.PlanetNeutronAcceleratorUsage = newValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Neutron Accelerator usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangeElementiumSynthesizerUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                double newValue = double.Parse(@event.EventData);
                planet.PlanetElementiumSynthesizerUsage = newValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Elementium Synthesizer usage changed on planet {planet.PlanetId}.");
            }
        }

        #endregion

        private void Do_ProduceShipExplorer(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                User user = context.User.FirstOrDefault(u => u.UserId.Equals(planet.PlanetUserId));
                planet.UpdateRessources();
                context.Ship.Add(new Ship
                {
                    ShipUserId = planet.PlanetUserId,
                    ShipPlanetId = planetId,
                    ShipType = ShipTypeEnum.Explorer.ToString(),
                    ShipTactical = user.IsOffensive()
                        ? TacticalEnum.Offensive.ToString()
                        : TacticalEnum.Defensive.ToString(),
                    ShipWeaponSlots = user.IsOffensive() ? 2 : 1,
                    ShipArmorSlots = user.IsOffensive() ? 1 : 2,
                    ShipItemSlots = 2,
                    ShipStructurePoints = 50,
                    ShipDriveStrength = 1000,
                    ShipPowerConsumption = 1,
                    ShipStorageCapacity = 50,
                    ShipCpuPower = 9,
                    ShipAccuracy = user.IsOffensive() ? 45 : 40,
                    ShipShieldPower = user.IsOffensive() ? 20 : 25,
                    ShipAttackPower = user.IsOffensive() ? 40 : 35,
                    ShipArmour = user.IsOffensive() ? 10 : 15
                }); ;
            }
        }

        private void Do_ProduceShipCarrier(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                User user = context.User.FirstOrDefault(u => u.UserId.Equals(planet.PlanetUserId));
                planet.UpdateRessources();
                context.Ship.Add(new Ship
                {
                    ShipUserId = planet.PlanetUserId,
                    ShipPlanetId = planetId,
                    ShipType = ShipTypeEnum.Carrier.ToString(),
                    ShipTactical = user.IsOffensive()
                        ? TacticalEnum.Offensive.ToString()
                        : TacticalEnum.Defensive.ToString(),
                    ShipWeaponSlots = user.IsOffensive() ? 2 : 1,
                    ShipArmorSlots = user.IsOffensive() ? 2 : 3,
                    ShipItemSlots = 3,
                    ShipStructurePoints = 80,
                    ShipDriveStrength = 800,
                    ShipPowerConsumption = 2,
                    ShipStorageCapacity = 500,
                    ShipCpuPower = 8,
                    ShipAccuracy = user.IsOffensive() ? 40 : 35,
                    ShipShieldPower = user.IsOffensive() ? 20 : 25,
                    ShipAttackPower = user.IsOffensive() ? 64 : 56,
                    ShipArmour = user.IsOffensive() ? 16 : 24
                }); ;
            }
        }
    }
}
