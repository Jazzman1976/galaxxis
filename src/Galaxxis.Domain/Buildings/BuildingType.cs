namespace Galaxxis.Domain.Buildings;

/// <summary>
/// All buildings that can currently be built on a planet.
/// </summary>
/// <remarks>
/// <para>
/// <b>Principle: every building that works consumes resources.</b> Producing, researching, processing:
/// whenever a building does something, it consumes supply resources, and only then. A building without
/// consumption values is a design error (guarded by a test).
/// </para>
/// There are two kinds (see <see cref="BuildingCatalog"/>), which differ only in <em>when</em> they work:
/// <list type="bullet">
///   <item><b>Resource buildings</b> work continuously: they produce one resource at the production speed the player sets.</item>
///   <item><b>Order buildings</b> work on one order at a time (today: producing ships or defenses; later also research or processing).</item>
/// </list>
/// The concept lists further buildings (storage, research lab, sensor phalanx, …) that do not exist yet.
/// </remarks>
public enum BuildingType
{
    /// <summary>Erzmine. Resource building producing ore.</summary>
    OreMine,

    /// <summary>Kraftwerk. Resource building producing energy.</summary>
    PowerSupply,

    /// <summary>Fabrik. Resource building producing tools.</summary>
    Factory,

    /// <summary>Magier Akademie. Resource building producing mana.</summary>
    MageAcademy,

    /// <summary>Kristallmine. Resource building producing crystals.</summary>
    CrystalMine,

    /// <summary>Neutronenbeschleuniger. Resource building producing antimatter.</summary>
    NeutronAccelerator,

    /// <summary>Elementium Synthetisierer. Resource building producing elementium.</summary>
    ElementiumSynthesizer,

    /// <summary>Schiffswerft. Order building producing ships.</summary>
    Shipyard,

    /// <summary>Verteidigungsanlage. Order building producing defenses.</summary>
    DefenseSystem,
}
