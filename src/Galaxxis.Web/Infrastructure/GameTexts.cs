using System.Globalization;
using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Web.Infrastructure;

/// <summary>
/// German display texts and number formats of the game pages.
/// </summary>
/// <remarks>
/// The game rules use English identifiers (<see cref="BuildingType.OreMine"/>); players read German. All translations
/// are kept here in one place, so a later localization only has to replace this class.
/// </remarks>
public static class GameTexts
{
    private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>The German name of a building.</summary>
    /// <param name="building">The building type.</param>
    /// <returns>For example "Erzmine".</returns>
    public static string Name(BuildingType building) => building switch
    {
        BuildingType.OreMine => "Erzmine",
        BuildingType.PowerSupply => "Kraftwerk",
        BuildingType.Factory => "Fabrik",
        BuildingType.MageAcademy => "Magierakademie",
        BuildingType.CrystalMine => "Kristallmine",
        BuildingType.NeutronAccelerator => "Neutronenbeschleuniger",
        BuildingType.ElementiumSynthesizer => "Elementiumsynthesizer",
        BuildingType.Shipyard => "Schiffswerft",
        BuildingType.DefenseSystem => "Verteidigungsanlage",
        _ => building.ToString(),
    };

    /// <summary>The German name of a resource.</summary>
    /// <param name="resource">The resource type.</param>
    /// <returns>For example "Erz".</returns>
    public static string Name(ResourceType resource) => resource switch
    {
        ResourceType.Ore => "Erz",
        ResourceType.Energy => "Energie",
        ResourceType.Tools => "Werkzeuge",
        ResourceType.Mana => "Mana",
        ResourceType.Crystals => "Kristalle",
        ResourceType.Antimatter => "Antimaterie",
        ResourceType.Elementium => "Elementium",
        _ => resource.ToString(),
    };

    /// <summary>The German name of a race.</summary>
    /// <param name="race">The race.</param>
    /// <returns>For example "Zwerge".</returns>
    public static string Name(Race race) => race switch
    {
        Race.Human => "Menschen",
        Race.Dwarf => "Zwerge",
        Race.Elf => "Elfen",
        Race.Goblin => "Goblins",
        Race.Troll => "Trolle",
        Race.Orc => "Orks",
        _ => race.ToString(),
    };

    /// <summary>
    /// A resource amount for display: rounded down to whole units (a player can only spend whole units they already
    /// have), with German thousands separators.
    /// </summary>
    /// <param name="amount">The amount.</param>
    /// <returns>For example "12.345".</returns>
    public static string Amount(double amount) => Math.Floor(amount).ToString("N0", German);

    /// <summary>A rate per day for display, with one decimal place (small rates would otherwise show as 0).</summary>
    /// <param name="perDay">The amount per day.</param>
    /// <returns>For example "1.234,5".</returns>
    public static string RatePerDay(double perDay) => perDay.ToString("N1", German);

    /// <summary>A duration for display.</summary>
    /// <param name="duration">The duration.</param>
    /// <returns>"hh:mm:ss", preceded by the number of days if it is a day or longer, for example "2 T 03:04:05".</returns>
    public static string Duration(TimeSpan duration)
    {
        TimeSpan positive = duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
        string time = positive.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
        return positive.Days > 0 ? $"{positive.Days} T {time}" : time;
    }

    /// <summary>
    /// A number for scripts and data attributes: always with a dot as decimal separator, regardless of the page culture.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>For example "1234.5678".</returns>
    public static string ForScript(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}
