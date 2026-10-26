namespace Galaxxis.Domain.Growth;

/// <summary>
/// The guide values the concept uses as the starting point (level 1) of most growth rates.
/// </summary>
/// <remarks>
/// Concept: KonzeptWord §6 ("Richtwerte für die Basiswerte"). Resource production uses its own,
/// larger base values because it is designed per 24 hours (see the building rule classes).
/// </remarks>
public static class BaseValue
{
    /// <summary>"Sehr wenig": very few.</summary>
    public const double VeryFew = 5;

    /// <summary>"Wenig": few.</summary>
    public const double Few = 10;

    /// <summary>"Durchschnittlich": average.</summary>
    public const double Normal = 15;

    /// <summary>"Hoch": much.</summary>
    public const double Much = 20;

    /// <summary>"Sehr hoch": very much.</summary>
    public const double VeryMuch = 25;
}
