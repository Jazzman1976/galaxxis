namespace Galaxxis.Domain.Buildings;

/// <summary>
/// The production speed of a resource building, set by the player in steps of 10 percent.
/// </summary>
/// <remarks>
/// <para>
/// Game design (confirmed 2026-09-16): running every building at full speed is deliberately not
/// sustainable, because buildings consume supply resources. The player chooses the speed of each
/// resource building and thereby specialises the planet. Production <b>and</b> consumption scale with
/// the speed in the same ratio: at 60 % a building produces and consumes 60 % of its full amounts.
/// </para>
/// <para>
/// Only multiples of 10 between 0 and 100 are valid. The game never changes a speed on its own.
/// </para>
/// </remarks>
public readonly record struct ProductionSpeed
{
    /// <summary>The size of one adjustment step in percent.</summary>
    public const int StepPercent = 10;

    /// <summary>Creates a production speed.</summary>
    /// <param name="percent">A multiple of 10 between 0 and 100.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value is outside 0–100 or not a multiple of 10.</exception>
    public ProductionSpeed(int percent)
    {
        if (percent is < 0 or > 100 || percent % StepPercent != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(percent), percent, "A production speed must be a multiple of 10 between 0 and 100 percent.");
        }

        Percent = percent;
    }

    /// <summary>The speed in percent: 0, 10, 20, …, 100.</summary>
    public int Percent { get; }

    /// <summary>The speed as a multiplier between 0 and 1, used in calculations.</summary>
    public double Factor => Percent / 100.0;

    /// <summary>100 %: the building runs at full speed. The default for a new building.</summary>
    public static ProductionSpeed Full => new(100);

    /// <summary>0 %: the building is switched off and neither produces nor consumes.</summary>
    public static ProductionSpeed Stopped => new(0);

    /// <summary>Formats the speed, for example "60 %".</summary>
    /// <returns>The speed in percent.</returns>
    public override string ToString() => $"{Percent} %";
}
