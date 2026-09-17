using Galaxxis.Domain.Fleets;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Tests.Fleets;

public class FleetRulesTests
{
    [Fact]
    public void A_fleet_with_enough_storage_takes_everything()
    {
        var stock = new ResourceAmounts { Ore = 100, Mana = 50, Elementium = 5 };

        Assert.Equal(stock, FleetRules.Plunder(stock, storageCapacity: 1000));
    }

    [Fact]
    public void Storage_is_shared_equally_when_every_resource_has_enough()
    {
        ResourceAmounts loot = FleetRules.Plunder(ResourceAmounts.Uniform(1000), storageCapacity: 700);

        Assert.Equal(ResourceAmounts.Uniform(100), loot);
    }

    [Fact]
    public void Scarce_resources_are_taken_completely_and_their_share_goes_to_the_others()
    {
        // 7 resources, capacity 700: elementium (10) and mana (40) are taken completely,
        // the remaining 650 are split among the five others: 130 each.
        ResourceAmounts stock = ResourceAmounts.Uniform(1000) with { Elementium = 10, Mana = 40 };

        ResourceAmounts loot = FleetRules.Plunder(stock, storageCapacity: 700);

        Assert.Equal(ResourceAmounts.Uniform(130) with { Elementium = 10, Mana = 40 }, loot);
        Assert.Equal(700, ResourceTypes.All.Sum(resource => loot[resource]), 9);
    }

    [Fact]
    public void Nothing_is_taken_without_storage()
    {
        Assert.Equal(ResourceAmounts.Zero, FleetRules.Plunder(ResourceAmounts.Uniform(500), storageCapacity: 0));
    }
}
