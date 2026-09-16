using Galaxxis.Domain.Buildings;

namespace Galaxxis.Domain.Tests.Buildings;

public class ProductionSpeedTests
{
    [Theory]
    [InlineData(0, 0.0)]
    [InlineData(10, 0.1)]
    [InlineData(60, 0.6)]
    [InlineData(100, 1.0)]
    public void Valid_speeds_are_multiples_of_ten_percent(int percent, double expectedFactor)
    {
        var speed = new ProductionSpeed(percent);

        Assert.Equal(percent, speed.Percent);
        Assert.Equal(expectedFactor, speed.Factor, 10);
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(5)]
    [InlineData(55)]
    [InlineData(110)]
    public void Other_speeds_are_rejected(int percent)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProductionSpeed(percent));
    }
}
