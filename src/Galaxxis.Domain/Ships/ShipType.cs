namespace Galaxxis.Domain.Ships;

/// <summary>
/// The ship classes of the concept, from smallest to largest.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel ships table. Only <see cref="Explorer"/> and <see cref="Carrier"/> have rules yet
/// (see <see cref="ShipCatalog"/>); the other classes are listed so stored data and future features use
/// consistent names.
/// </remarks>
public enum ShipType
{
    /// <summary>Aufklärer.</summary>
    Explorer,

    /// <summary>Frachter.</summary>
    Carrier,

    /// <summary>Jäger.</summary>
    Fighter,

    /// <summary>Kreuzer.</summary>
    Cruiser,

    /// <summary>Schlachtschiff.</summary>
    Battleship,

    /// <summary>Sternenzerstörer.</summary>
    Destroyer,

    /// <summary>Kampfstern.</summary>
    Battlestar,

    /// <summary>Raumstation.</summary>
    Battlestation,
}
