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
            }
        }

        #endregion


        #region Do Build Event Actions ------------------------------------------------------------

        private void Do_BuildOreMine(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetOreMine++;
                planet.RemoveOreMine();
                Debug.WriteLine($"{DateStamp}: Ore Mine build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildPowerSupply(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetPowerSupply++;
                planet.RemovePowerSupply();
                Debug.WriteLine($"{DateStamp}: Power Supply build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildFactory(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetFactory++;
                planet.RemoveFactory();
                Debug.WriteLine($"{DateStamp}: Factory build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildMageAcademy(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetMageAcademy++;
                planet.RemoveMageAcademy();
                Debug.WriteLine($"{DateStamp}: Mage Academy build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildCrystalMine(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetCrystalMine++;
                planet.RemoveCrystalMine();
                Debug.WriteLine($"{DateStamp}: Chrystal Mine build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildNeutronAccelerator(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetNeutronAccelerator++;
                planet.RemoveNeutronAccelerator();
                Debug.WriteLine($"{DateStamp}: Neutron Accelerator build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildElementiumSynthesizer(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetElementiumSynthesizer++;
                planet.RemoveElementiumSynthesizer();
                Debug.WriteLine($"{DateStamp}: Elementium Synthesizer build on planet {planet.PlanetId}.");
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
                Debug.WriteLine(
                    $"{DateStamp}: Elementium Synthesizer usage changed on planet {planet.PlanetId}.");
            }
        }

        #endregion
    }
}
