namespace Galaxxis.Domain.Resources;

/// <summary>
/// The base income ("Grundertrag"): resources every planet receives per day, even without buildings.
/// </summary>
/// <remarks>
/// <para>
/// Concept: ConzeptExcel "Kosten und Erträge", column "Grundertrag (pro 24h)"; KonzeptWord §6 says a
/// base income is added on top of building production.
/// </para>
/// <para>
/// Design rule (confirmed 2026-09-16): the base income is <b>never</b> reduced, neither by the
/// production speed the player sets nor by supply shortages. It is what guarantees that a planet can
/// always recover from an empty stock, so a bad economic decision can never lock a player out.
/// The race production factors still apply to it.
/// </para>
/// <para>
/// Unclaimed planets (without owner) receive the base income too, without race factors, so there is always
/// something to plunder and plundered planets fill up again (confirmed 2026-09-17).
/// </para>
/// </remarks>
public static class BaseIncome
{
    /// <summary>Base income per day for every resource.</summary>
    public static ResourceAmounts PerDay { get; } = new()
    {
        // Category I resources: 50 per day.
        Ore = 50,
        Energy = 50,
        Tools = 50,

        // Category II resources: 30 per day.
        Mana = 30,
        Crystals = 30,
        Antimatter = 30,

        // Category III: 10 per day.
        Elementium = 10,
    };
}
