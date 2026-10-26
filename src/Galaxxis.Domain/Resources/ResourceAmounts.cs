using Galaxxis.Domain.Growth;

namespace Galaxxis.Domain.Resources;

/// <summary>
/// One number per resource, for example a stock, a cost, a daily production or a set of factors.
/// </summary>
/// <remarks>
/// <para>
/// Almost every game rule deals with all seven resources at once. This type keeps such code short and
/// readable: <c>stock - cost</c> instead of seven separate subtractions. It is an immutable value;
/// every operation returns a new instance.
/// </para>
/// <para>
/// Resources that are not set are 0, so a cost can be written as
/// <c>new ResourceAmounts { Ore = 10, Mana = 10, Crystals = 5 }</c>.
/// </para>
/// </remarks>
public readonly record struct ResourceAmounts
{
    /// <summary>Amount of ore.</summary>
    public double Ore { get; init; }

    /// <summary>Amount of energy.</summary>
    public double Energy { get; init; }

    /// <summary>Amount of tools.</summary>
    public double Tools { get; init; }

    /// <summary>Amount of mana.</summary>
    public double Mana { get; init; }

    /// <summary>Amount of crystals.</summary>
    public double Crystals { get; init; }

    /// <summary>Amount of antimatter.</summary>
    public double Antimatter { get; init; }

    /// <summary>Amount of elementium.</summary>
    public double Elementium { get; init; }

    /// <summary>0 for every resource.</summary>
    public static ResourceAmounts Zero => default;

    /// <summary>1 for every resource. The neutral starting point for multiplication factors.</summary>
    public static ResourceAmounts One => Uniform(1);

    /// <summary>Creates amounts with the same value for every resource.</summary>
    /// <param name="value">The value for all seven resources.</param>
    /// <returns>The new amounts.</returns>
    public static ResourceAmounts Uniform(double value) => new()
    {
        Ore = value,
        Energy = value,
        Tools = value,
        Mana = value,
        Crystals = value,
        Antimatter = value,
        Elementium = value,
    };

    /// <summary>Gets the amount of a single resource.</summary>
    /// <param name="type">The resource.</param>
    /// <returns>The amount of that resource.</returns>
    public double this[ResourceType type] => type switch
    {
        ResourceType.Ore => Ore,
        ResourceType.Energy => Energy,
        ResourceType.Tools => Tools,
        ResourceType.Mana => Mana,
        ResourceType.Crystals => Crystals,
        ResourceType.Antimatter => Antimatter,
        ResourceType.Elementium => Elementium,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown resource type."),
    };

    /// <summary>Returns a copy in which one resource has a different amount.</summary>
    /// <param name="type">The resource to change.</param>
    /// <param name="value">The new amount of that resource.</param>
    /// <returns>The changed copy.</returns>
    public ResourceAmounts With(ResourceType type, double value) => type switch
    {
        ResourceType.Ore => this with { Ore = value },
        ResourceType.Energy => this with { Energy = value },
        ResourceType.Tools => this with { Tools = value },
        ResourceType.Mana => this with { Mana = value },
        ResourceType.Crystals => this with { Crystals = value },
        ResourceType.Antimatter => this with { Antimatter = value },
        ResourceType.Elementium => this with { Elementium = value },
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown resource type."),
    };

    /// <summary>
    /// Treats every amount as the level-1 base value of a growth rate and returns the grown values.
    /// </summary>
    /// <param name="factor">How steeply the values grow.</param>
    /// <param name="level">The level to grow to.</param>
    /// <returns>The grown amounts. See <see cref="GrowthRate.ValueAt"/>.</returns>
    /// <example>Build costs of an ore mine at level 20: <c>baseCosts.GrownTo(GrowthFactor.Construction, 20)</c>.</example>
    public ResourceAmounts GrownTo(GrowthFactor factor, int level) => Map(value => new GrowthRate(value, factor).ValueAt(level));

    /// <summary>Returns a copy in which every negative amount is replaced by 0.</summary>
    /// <returns>The copy without negative amounts.</returns>
    public ResourceAmounts ClampedToZero() => Map(value => Math.Max(0, value));

    /// <summary>
    /// Checks whether these amounts are at least as large as <paramref name="required"/> for every
    /// resource, for example whether a stock covers a build cost.
    /// </summary>
    /// <param name="required">The amounts that are needed.</param>
    /// <returns><see langword="true"/> if no resource falls short.</returns>
    public bool Covers(ResourceAmounts required)
    {
        ResourceAmounts available = this;
        return ResourceTypes.All.All(type => available[type] >= required[type]);
    }

    /// <summary>
    /// Calculates how much of each resource is missing to pay <paramref name="required"/> from these amounts.
    /// </summary>
    /// <param name="required">The amounts that are needed, for example a build cost.</param>
    /// <returns>The shortfall per resource; 0 for every resource that is sufficient.</returns>
    public ResourceAmounts MissingFor(ResourceAmounts required) => (required - this).ClampedToZero();

    /// <summary>Multiplies each resource with the matching resource of <paramref name="factors"/>.</summary>
    /// <param name="factors">One factor per resource, for example race production factors.</param>
    /// <returns>The multiplied amounts.</returns>
    public ResourceAmounts MultipliedBy(ResourceAmounts factors) => new()
    {
        Ore = Ore * factors.Ore,
        Energy = Energy * factors.Energy,
        Tools = Tools * factors.Tools,
        Mana = Mana * factors.Mana,
        Crystals = Crystals * factors.Crystals,
        Antimatter = Antimatter * factors.Antimatter,
        Elementium = Elementium * factors.Elementium,
    };

    /// <summary>Adds two amounts resource by resource.</summary>
    /// <param name="left">The first amounts.</param>
    /// <param name="right">The second amounts.</param>
    /// <returns>The sum.</returns>
    public static ResourceAmounts operator +(ResourceAmounts left, ResourceAmounts right) => new()
    {
        Ore = left.Ore + right.Ore,
        Energy = left.Energy + right.Energy,
        Tools = left.Tools + right.Tools,
        Mana = left.Mana + right.Mana,
        Crystals = left.Crystals + right.Crystals,
        Antimatter = left.Antimatter + right.Antimatter,
        Elementium = left.Elementium + right.Elementium,
    };

    /// <summary>Subtracts two amounts resource by resource. The result may be negative.</summary>
    /// <param name="left">The amounts to subtract from.</param>
    /// <param name="right">The amounts to subtract.</param>
    /// <returns>The difference.</returns>
    public static ResourceAmounts operator -(ResourceAmounts left, ResourceAmounts right) => left + right * -1;

    /// <summary>Multiplies every resource by the same number.</summary>
    /// <param name="amounts">The amounts.</param>
    /// <param name="factor">The number to multiply with.</param>
    /// <returns>The scaled amounts.</returns>
    public static ResourceAmounts operator *(ResourceAmounts amounts, double factor) => amounts.Map(value => value * factor);

    /// <summary>Formats the amounts for logs and test messages.</summary>
    /// <returns>A readable representation, for example "Ore=10, Energy=0, ...".</returns>
    public override string ToString()
    {
        ResourceAmounts amounts = this;
        return string.Join(", ", ResourceTypes.All.Select(type => $"{type}={amounts[type]:0.###}"));
    }

    private ResourceAmounts Map(Func<double, double> transform) => new()
    {
        Ore = transform(Ore),
        Energy = transform(Energy),
        Tools = transform(Tools),
        Mana = transform(Mana),
        Crystals = transform(Crystals),
        Antimatter = transform(Antimatter),
        Elementium = transform(Elementium),
    };
}
