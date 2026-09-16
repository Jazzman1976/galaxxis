namespace Galaxxis.Domain.Growth;

/// <summary>
/// How steeply a value grows from one level to the next. The numeric value of each member is the
/// factor used in the growth formula (see <see cref="GrowthRate"/>).
/// </summary>
/// <remarks>
/// Concept: KonzeptWord §6.1–6.4 and ConzeptExcel sheet "Wachstums Raten".
/// The higher the factor, the more expensive higher levels become compared to level 1.
/// </remarks>
public enum GrowthFactor
{
    /// <summary>Resource production per day (factor 1). Concept: KonzeptWord §6.1.</summary>
    Resources = 1,

    /// <summary>Continuous upkeep (consumption) per day (factor 2). Concept: KonzeptWord §6.2.</summary>
    Supply = 2,

    /// <summary>Researching the next technology level (factor 3). Concept: KonzeptWord §6.3.</summary>
    Research = 3,

    /// <summary>Construction costs of the next building level (factor 4). Concept: KonzeptWord §6.4.</summary>
    Construction = 4,

    /// <summary>Construction duration in seconds (factor 5). Concept: ConzeptExcel "Wachstums Raten", column "Bauzeit".</summary>
    BuildTime = 5,
}
