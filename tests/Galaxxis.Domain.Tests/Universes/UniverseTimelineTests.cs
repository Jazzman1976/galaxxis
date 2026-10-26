using Galaxxis.Domain.Universes;

namespace Galaxxis.Domain.Tests.Universes;

public class UniverseTimelineTests
{
    private static readonly DateTimeOffset Epoch = new(2026, 9, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void The_tick_follows_the_wall_clock()
    {
        var timeline = new UniverseTimeline(Epoch, TimeSpan.FromSeconds(1), TimeSpan.Zero, PausedSince: null);

        Assert.Equal(0, timeline.TickAt(Epoch));
        Assert.Equal(0, timeline.TickAt(Epoch.AddMilliseconds(999)));
        Assert.Equal(90, timeline.TickAt(Epoch.AddSeconds(90.5)));
        Assert.Equal(0, timeline.TickAt(Epoch.AddHours(-1)));
    }

    [Fact]
    public void Finished_pauses_are_subtracted()
    {
        var timeline = new UniverseTimeline(Epoch, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(10), PausedSince: null);

        Assert.Equal(300, timeline.TickAt(Epoch.AddMinutes(15)));
    }

    [Fact]
    public void Time_stands_still_during_a_pause()
    {
        var timeline = new UniverseTimeline(Epoch, TimeSpan.FromSeconds(1), TimeSpan.Zero, PausedSince: Epoch.AddSeconds(60));

        Assert.True(timeline.IsPaused);
        Assert.Equal(60, timeline.TickAt(Epoch.AddSeconds(60)));
        Assert.Equal(60, timeline.TickAt(Epoch.AddDays(3)));
    }

    [Theory]
    [InlineData(1000, 25, 25)]    // 25 seconds with 1-second ticks
    [InlineData(10_000, 25, 3)]   // rounded up: never finishes early
    [InlineData(500, 1, 2)]
    [InlineData(1000, 0, 0)]
    public void Durations_convert_to_ticks_rounded_up(int tickMilliseconds, int seconds, long expectedTicks)
    {
        var timeline = new UniverseTimeline(Epoch, TimeSpan.FromMilliseconds(tickMilliseconds), TimeSpan.Zero, null);

        Assert.Equal(expectedTicks, timeline.TicksFor(TimeSpan.FromSeconds(seconds)));
    }

    [Fact]
    public void The_real_time_of_a_future_tick_includes_finished_pauses()
    {
        var timeline = new UniverseTimeline(Epoch, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5), PausedSince: null);

        Assert.Equal(Epoch.AddMinutes(5).AddSeconds(125), timeline.RealTimeOf(125));
        Assert.Equal(125, timeline.TickAt(timeline.RealTimeOf(125)));
    }

    [Fact]
    public void Ticks_convert_back_to_real_time()
    {
        var timeline = new UniverseTimeline(Epoch, TimeSpan.FromMilliseconds(500), TimeSpan.Zero, null);

        Assert.Equal(TimeSpan.FromSeconds(30), timeline.DurationOf(60));
        Assert.Equal(TimeSpan.Zero, timeline.DurationOf(-5));
    }
}
