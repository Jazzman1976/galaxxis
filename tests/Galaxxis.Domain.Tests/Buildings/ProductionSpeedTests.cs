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

    [Fact]
    public void Building_state_can_be_changed_one_building_at_a_time()
    {
        PlanetBuildings buildings = PlanetBuildings.Empty
            .WithLevel(BuildingType.OreMine, 4)
            .WithSpeed(BuildingType.OreMine, new ProductionSpeed(30))
            .WithLevel(BuildingType.Shipyard, 2)
            .WithWorking(BuildingType.Shipyard, true);

        Assert.Equal(new ResourceBuildingState(4, new ProductionSpeed(30)), buildings.OreMine);
        Assert.Equal(new OrderBuildingState(2, IsWorking: true), buildings.Shipyard);
        Assert.Equal(PlanetBuildings.Empty.Factory, buildings.Factory);
        Assert.Throws<ArgumentOutOfRangeException>(() => buildings.WithSpeed(BuildingType.Shipyard, ProductionSpeed.Full));
    }
}
