namespace Galaxxis.Tools.LoadTest;

/// <summary>
/// A summary of many measured values (count, average, percentiles, maximum), for the load test report.
/// </summary>
/// <param name="Count">Number of values.</param>
/// <param name="Average">Arithmetic mean.</param>
/// <param name="Median">50th percentile.</param>
/// <param name="P95">95th percentile: 95 % of the values are at or below it.</param>
/// <param name="P99">99th percentile.</param>
/// <param name="Max">Largest value.</param>
internal sealed record Statistics(int Count, double Average, double Median, double P95, double P99, double Max)
{
    /// <summary>The summary of no values at all.</summary>
    public static Statistics Empty { get; } = new(0, 0, 0, 0, 0, 0);

    /// <summary>Summarises the values.</summary>
    /// <param name="values">The measured values.</param>
    /// <returns>The summary, or <see cref="Empty"/> if there are no values.</returns>
    public static Statistics Of(IEnumerable<double> values)
    {
        double[] sorted = values.Order().ToArray();
        if (sorted.Length == 0)
        {
            return Empty;
        }

        return new Statistics(sorted.Length, sorted.Average(), Percentile(sorted, 0.50), Percentile(sorted, 0.95), Percentile(sorted, 0.99), sorted[^1]);
    }

    /// <summary>Nearest-rank percentile of sorted values.</summary>
    private static double Percentile(double[] sorted, double fraction) =>
        sorted[Math.Clamp((int)Math.Ceiling(fraction * sorted.Length) - 1, 0, sorted.Length - 1)];
}
