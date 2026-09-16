using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Resources;
using Galaxxis.Domain.Universes;

namespace Galaxxis.Persistence.Game.Entities;

/// <summary>
/// A planet: the place where players store resources and build buildings.
/// </summary>
/// <remarks>
/// <para>
/// <b>One column per value.</b> Resources and buildings are stored as plain columns (<c>Ore</c>,
/// <c>OreMineLevel</c>, <c>OreMineSpeedPercent</c>, …), which keeps the table easy to read and query in SQL.
/// Code works with the grouped Domain types instead: <see cref="Stock"/> and <see cref="Buildings"/> read and
/// write these columns, so game rules never deal with individual columns.
/// </para>
/// <para>
/// <b>Lazy resources.</b> The stock is only correct as of <see cref="ResourcesSettledTick"/>. Before a planet is
/// changed or shown, it is brought up to date with <c>ResourceSettlement</c> (Domain).
/// </para>
/// <para>
/// <b>Concurrency.</b> <see cref="RowVersion"/> makes the database reject a save that is based on an outdated
/// copy of the planet, as a safety net on top of the engine being the only writer.
/// </para>
/// </remarks>
public sealed class Planet
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>The universe the planet belongs to. Stored directly (not only via star system) for simple queries.</summary>
    public int UniverseId { get; set; }

    /// <summary>Navigation to the universe.</summary>
    public Universe Universe { get; set; } = null!;

    /// <summary>The star system the planet belongs to.</summary>
    public int StarSystemId { get; set; }

    /// <summary>Navigation to the star system.</summary>
    public StarSystem StarSystem { get; set; } = null!;

    /// <summary>The owning player; <see langword="null"/> means the planet is unclaimed.</summary>
    public int? OwnerPlayerId { get; set; }

    /// <summary>Navigation to the owning player, if any.</summary>
    public Player? Owner { get; set; }

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Number of building plots.</summary>
    public int Fields { get; set; }

    /// <summary>X position within the star system.</summary>
    public int X { get; set; }

    /// <summary>Y position within the star system.</summary>
    public int Y { get; set; }

    /// <summary>Z position within the star system.</summary>
    public int Z { get; set; }

    /// <summary>The universe tick up to which <see cref="Stock"/> has been settled.</summary>
    public long ResourcesSettledTick { get; set; }

    /// <summary>Concurrency token maintained by SQL Server.</summary>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>The moons of the planet.</summary>
    public List<Moon> Moons { get; set; } = [];

    #region Resource columns -----------------------------------------------------------------------

    /// <summary>Ore in stock.</summary>
    public double Ore { get; set; }

    /// <summary>Energy in stock.</summary>
    public double Energy { get; set; }

    /// <summary>Tools in stock.</summary>
    public double Tools { get; set; }

    /// <summary>Mana in stock.</summary>
    public double Mana { get; set; }

    /// <summary>Crystals in stock.</summary>
    public double Crystals { get; set; }

    /// <summary>Antimatter in stock.</summary>
    public double Antimatter { get; set; }

    /// <summary>Elementium in stock.</summary>
    public double Elementium { get; set; }

    #endregion

    #region Building columns -----------------------------------------------------------------------

    /// <summary>Ore mine level.</summary>
    public int OreMineLevel { get; set; }

    /// <summary>Ore mine production speed in percent.</summary>
    public int OreMineSpeedPercent { get; set; } = 100;

    /// <summary>Power supply level.</summary>
    public int PowerSupplyLevel { get; set; }

    /// <summary>Power supply production speed in percent.</summary>
    public int PowerSupplySpeedPercent { get; set; } = 100;

    /// <summary>Factory level.</summary>
    public int FactoryLevel { get; set; }

    /// <summary>Factory production speed in percent.</summary>
    public int FactorySpeedPercent { get; set; } = 100;

    /// <summary>Mage academy level.</summary>
    public int MageAcademyLevel { get; set; }

    /// <summary>Mage academy production speed in percent.</summary>
    public int MageAcademySpeedPercent { get; set; } = 100;

    /// <summary>Crystal mine level.</summary>
    public int CrystalMineLevel { get; set; }

    /// <summary>Crystal mine production speed in percent.</summary>
    public int CrystalMineSpeedPercent { get; set; } = 100;

    /// <summary>Neutron accelerator level.</summary>
    public int NeutronAcceleratorLevel { get; set; }

    /// <summary>Neutron accelerator production speed in percent.</summary>
    public int NeutronAcceleratorSpeedPercent { get; set; } = 100;

    /// <summary>Elementium synthesizer level.</summary>
    public int ElementiumSynthesizerLevel { get; set; }

    /// <summary>Elementium synthesizer production speed in percent.</summary>
    public int ElementiumSynthesizerSpeedPercent { get; set; } = 100;

    /// <summary>Shipyard level.</summary>
    public int ShipyardLevel { get; set; }

    /// <summary>Whether the shipyard is working on an order (it consumes resources only then).</summary>
    public bool ShipyardIsWorking { get; set; }

    /// <summary>Defense system level.</summary>
    public int DefenseSystemLevel { get; set; }

    /// <summary>Whether the defense system is working on an order (it consumes resources only then).</summary>
    public bool DefenseSystemIsWorking { get; set; }

    #endregion

    #region Domain views (not stored, map to the columns above) ------------------------------------

    /// <summary>The position as a Domain coordinate.</summary>
    public Coordinate Coordinate
    {
        get => new(X, Y, Z);
        set => (X, Y, Z) = (value.X, value.Y, value.Z);
    }

    /// <summary>The resource stock as Domain amounts. Reads and writes the seven resource columns.</summary>
    public ResourceAmounts Stock
    {
        get => new()
        {
            Ore = Ore,
            Energy = Energy,
            Tools = Tools,
            Mana = Mana,
            Crystals = Crystals,
            Antimatter = Antimatter,
            Elementium = Elementium,
        };
        set
        {
            Ore = value.Ore;
            Energy = value.Energy;
            Tools = value.Tools;
            Mana = value.Mana;
            Crystals = value.Crystals;
            Antimatter = value.Antimatter;
            Elementium = value.Elementium;
        }
    }

    /// <summary>
    /// All buildings as the Domain snapshot the economy rules work with. Reads and writes the building columns.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Reading: a stored speed is not a valid 10 % step.</exception>
    public PlanetBuildings Buildings
    {
        get => new()
        {
            OreMine = new ResourceBuildingState(OreMineLevel, new ProductionSpeed(OreMineSpeedPercent)),
            PowerSupply = new ResourceBuildingState(PowerSupplyLevel, new ProductionSpeed(PowerSupplySpeedPercent)),
            Factory = new ResourceBuildingState(FactoryLevel, new ProductionSpeed(FactorySpeedPercent)),
            MageAcademy = new ResourceBuildingState(MageAcademyLevel, new ProductionSpeed(MageAcademySpeedPercent)),
            CrystalMine = new ResourceBuildingState(CrystalMineLevel, new ProductionSpeed(CrystalMineSpeedPercent)),
            NeutronAccelerator = new ResourceBuildingState(NeutronAcceleratorLevel, new ProductionSpeed(NeutronAcceleratorSpeedPercent)),
            ElementiumSynthesizer = new ResourceBuildingState(ElementiumSynthesizerLevel, new ProductionSpeed(ElementiumSynthesizerSpeedPercent)),
            Shipyard = new OrderBuildingState(ShipyardLevel, ShipyardIsWorking),
            DefenseSystem = new OrderBuildingState(DefenseSystemLevel, DefenseSystemIsWorking),
        };
        set
        {
            (OreMineLevel, OreMineSpeedPercent) = (value.OreMine.Level, value.OreMine.Speed.Percent);
            (PowerSupplyLevel, PowerSupplySpeedPercent) = (value.PowerSupply.Level, value.PowerSupply.Speed.Percent);
            (FactoryLevel, FactorySpeedPercent) = (value.Factory.Level, value.Factory.Speed.Percent);
            (MageAcademyLevel, MageAcademySpeedPercent) = (value.MageAcademy.Level, value.MageAcademy.Speed.Percent);
            (CrystalMineLevel, CrystalMineSpeedPercent) = (value.CrystalMine.Level, value.CrystalMine.Speed.Percent);
            (NeutronAcceleratorLevel, NeutronAcceleratorSpeedPercent) = (value.NeutronAccelerator.Level, value.NeutronAccelerator.Speed.Percent);
            (ElementiumSynthesizerLevel, ElementiumSynthesizerSpeedPercent) = (value.ElementiumSynthesizer.Level, value.ElementiumSynthesizer.Speed.Percent);
            (ShipyardLevel, ShipyardIsWorking) = (value.Shipyard.Level, value.Shipyard.IsWorking);
            (DefenseSystemLevel, DefenseSystemIsWorking) = (value.DefenseSystem.Level, value.DefenseSystem.IsWorking);
        }
    }

    #endregion
}
