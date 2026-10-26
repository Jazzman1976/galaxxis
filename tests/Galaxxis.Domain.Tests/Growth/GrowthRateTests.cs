using Galaxxis.Domain.Growth;

namespace Galaxxis.Domain.Tests.Growth;

/// <summary>
/// The growth formula must reproduce the concept spreadsheet (ConzeptExcel, sheet "Wachstums Raten").
/// </summary>
public class GrowthRateTests
{
    [Theory]
    // Resources, factor 1
    [InlineData(BaseValue.VeryFew, GrowthFactor.Resources, 1, 5)]
    [InlineData(BaseValue.VeryFew, GrowthFactor.Resources, 20, 1810)]
    [InlineData(BaseValue.VeryFew, GrowthFactor.Resources, 30, 4210)]
    // Supply, factor 2
    [InlineData(BaseValue.Normal, GrowthFactor.Supply, 2, 45)]
    [InlineData(BaseValue.Normal, GrowthFactor.Supply, 20, 10845)]
    // Research, factor 3
    [InlineData(BaseValue.Much, GrowthFactor.Research, 10, 4880)]
    // Construction, factor 4
    [InlineData(BaseValue.Normal, GrowthFactor.Construction, 20, 21675)]
    [InlineData(BaseValue.VeryMuch, GrowthFactor.Construction, 30, 84125)]
    // Build time, factor 5
    [InlineData(BaseValue.VeryMuch, GrowthFactor.BuildTime, 2, 150)]
    [InlineData(BaseValue.VeryMuch, GrowthFactor.BuildTime, 30, 105150)]
    public void Value_matches_the_concept_spreadsheet(double baseValue, GrowthFactor factor, int level, double expected)
    {
        Assert.Equal(expected, new GrowthRate(baseValue, factor).ValueAt(level));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Levels_below_one_have_no_value(int level)
    {
        Assert.Equal(0, new GrowthRate(BaseValue.Normal, GrowthFactor.Construction).ValueAt(level));
    }

    [Fact]
    public void A_base_value_of_zero_stays_zero_at_every_level()
    {
        Assert.Equal(0, new GrowthRate(0, GrowthFactor.Construction).ValueAt(25));
    }
}
