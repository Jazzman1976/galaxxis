namespace Galaxxis.Domain.Resources;

/// <summary>
/// The seven resources of Galaxxis.
/// </summary>
/// <remarks>
/// Concept: KonzeptWord §3. The resources fall into three categories and three kinds of use:
/// <list type="table">
///   <listheader><term>Resource</term><description>Category / use</description></listheader>
///   <item><term><see cref="Ore"/></term><description>I (base) / construction</description></item>
///   <item><term><see cref="Energy"/></term><description>I (base) / supply</description></item>
///   <item><term><see cref="Tools"/></term><description>I (base) / goods production</description></item>
///   <item><term><see cref="Mana"/></term><description>II (special) / construction and supply</description></item>
///   <item><term><see cref="Crystals"/></term><description>II (special) / construction and goods production</description></item>
///   <item><term><see cref="Antimatter"/></term><description>II (special) / supply and goods production</description></item>
///   <item><term><see cref="Elementium"/></term><description>III / construction, supply and goods production</description></item>
/// </list>
/// "Supply" resources are the ones buildings consume continuously while they run
/// (see <see cref="ResourceTypes.Supply"/>).
/// </remarks>
public enum ResourceType
{
    /// <summary>Erz. Category I, used for construction.</summary>
    Ore,

    /// <summary>Energie. Category I, consumed as supply.</summary>
    Energy,

    /// <summary>Werkzeuge. Category I, used for goods production (ships, defenses).</summary>
    Tools,

    /// <summary>Mana. Category II, used for construction and consumed as supply.</summary>
    Mana,

    /// <summary>Kristalle. Category II, used for construction and goods production.</summary>
    Crystals,

    /// <summary>Antimaterie. Category II, consumed as supply and used for goods production.</summary>
    Antimatter,

    /// <summary>Elementium. Category III, used for everything.</summary>
    Elementium,
}

/// <summary>
/// Groups of <see cref="ResourceType"/> values that the rules refer to.
/// </summary>
public static class ResourceTypes
{
    /// <summary>All seven resources, in the order of <see cref="ResourceType"/>.</summary>
    public static IReadOnlyList<ResourceType> All { get; } =
    [
        ResourceType.Ore,
        ResourceType.Energy,
        ResourceType.Tools,
        ResourceType.Mana,
        ResourceType.Crystals,
        ResourceType.Antimatter,
        ResourceType.Elementium,
    ];

    /// <summary>
    /// The supply resources: the only resources that buildings consume continuously while they run.
    /// Ore, tools and crystals are only spent on construction and production orders.
    /// </summary>
    public static IReadOnlyList<ResourceType> Supply { get; } =
    [
        ResourceType.Energy,
        ResourceType.Mana,
        ResourceType.Antimatter,
        ResourceType.Elementium,
    ];
}
