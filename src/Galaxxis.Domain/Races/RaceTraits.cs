namespace Galaxxis.Domain.Races;

/// <summary>
/// Whether a race builds its ships and defenses for attack or for protection.
/// </summary>
/// <remarks>
/// Concept: KonzeptWord §4.3. The strategy decides which variant of a ship or defense a race builds:
/// offensive variants have more weapon slots and attack, defensive variants more armor and shields
/// (ConzeptExcel, ships and defenses tables).
/// </remarks>
public enum Strategy
{
    /// <summary>Offensiv.</summary>
    Offensive,

    /// <summary>Defensiv.</summary>
    Defensive,
}

/// <summary>
/// The kind of damage a race's weapons deal. Each weapon technology is strong (150 % damage) against
/// one armor type and weak (75 % damage) against another.
/// </summary>
/// <remarks>Concept: KonzeptWord §4.2 and §7.1.5. Used by the combat system (not implemented yet).</remarks>
public enum WeaponTechnology
{
    /// <summary>Kinetische Waffen: projectiles. Strong against anti-magic armor, weak against structural armor.</summary>
    Kinetic,

    /// <summary>Magische Waffen. Strong against compensating armor, weak against anti-magic armor.</summary>
    Magic,

    /// <summary>Energetische Waffen: lasers and similar. Strong against structural armor, weak against compensating armor.</summary>
    Energetic,
}

/// <summary>
/// The kind of armor a race uses.
/// </summary>
/// <remarks>Concept: KonzeptWord §4.2 and §7.1.6. Used by the combat system (not implemented yet).</remarks>
public enum ArmorType
{
    /// <summary>Strukturelle Rüstung: steel plating.</summary>
    Structural,

    /// <summary>Antimagische Rüstung: protective spells and barriers.</summary>
    AntiMagic,

    /// <summary>Kompensierende Rüstung: energy absorbers and converters.</summary>
    Compensating,
}
