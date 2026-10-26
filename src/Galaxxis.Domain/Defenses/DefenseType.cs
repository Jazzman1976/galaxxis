namespace Galaxxis.Domain.Defenses;

/// <summary>
/// The defense classes of the concept, from smallest to largest.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel defenses table. Only <see cref="LightManualCannon"/> has rules yet
/// (see <see cref="DefenseCatalog"/>).
/// </remarks>
public enum DefenseType
{
    /// <summary>Kleines Manuelles Geschütz.</summary>
    LightManualCannon,

    /// <summary>Großes Manuelles Geschütz.</summary>
    HeavyManualCannon,

    /// <summary>Kleines Automatisches Geschütz.</summary>
    LightAutomaticCannon,

    /// <summary>Großes Automatisches Geschütz.</summary>
    HeavyAutomaticCannon,

    /// <summary>Kleines Computergestütztes Geschütz.</summary>
    LightComputerAidedCannon,

    /// <summary>Großes Computergestütztes Geschütz.</summary>
    HeavyComputerAidedCannon,

    /// <summary>Gefechtsturm.</summary>
    LightTurret,

    /// <summary>Gefechtsstation.</summary>
    HeavyTurret,
}
