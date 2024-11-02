using Schedule.SchedulerService;
using Schedule.SchedulerService.Common;
using Schedule.TimeSlots;
using Schedule.TimeSlots.Common;

namespace Schedule.Tests;

public class SchedulerTests
{
    [Theory]
    [InlineData(15)]  // 15-minute intervals
    [InlineData(30)]  // 30-minute intervals
    [InlineData(60)]  // 1-hour intervals
    public void ValidIntervalsShouldCreateScheduler(int interval)
    {
        var scheduler = new TestSlotScheduler(interval);
        Assert.Equal(interval, scheduler.Interval);
    }

    [Theory]
    [InlineData(0)]   // Zero interval
    [InlineData(-30)] // Negative interval
    [InlineData(7)]   // Doesn't divide evenly into 60
    public void InvalidIntervalsShouldThrowException(int interval)
    {
        Assert.Throws<ArgumentException>(() => new TestSlotScheduler(interval));
    }

    [Fact]
    public void BasicSchedulerShouldCreateCorrectNumberOfSlots()
    {
        var scheduler = new TestSlotScheduler(30); // 30-minute intervals
        var slots = scheduler.TimeSlotsForTheSchedule();

        Assert.Equal(24, slots.Count); // Should have all 24 hours
        Assert.All(slots.Values, hourSlots =>
            Assert.Equal(2, hourSlots.Count)); // Each hour should have 2 30-minute slots
    }

    [Fact]
    public void SchedulerShouldCreateOnlyAvailableSlots()
    {
        var scheduler = new TestSlotScheduler(30); // 30-minute intervals
        var slots = scheduler.TimeSlotsForTheSchedule();
        
        Assert.All(slots.Values, 
            hourSlots => hourSlots.ForEach(slot => Assert.IsType<AvailableTimeSlot>(slot))); // Each newly created slot should be available
    }

    [Theory]
    [InlineData(9, 17)]  // Standard 9-5
    [InlineData(0, 24)]  // Full day
    [InlineData(13, 21)] // Afternoon/Evening shift
    public void WorkingDaySchedulerShouldRespectWorkingHours(int startHour, int endHour)
    {
        var workingDay = new WorkingDay(
            TimeSpan.FromHours(startHour),
            TimeSpan.FromHours(endHour),
            new List<TimeSlot>()
        );

        var scheduler = new WorkingDayScheduler(workingDay, 60);
        var slots = scheduler.TimeSlotsForTheSchedule();

        // Verify no slots before start hour
        for (int hour = 0; hour < startHour; hour++)
        {
            Assert.False(slots.ContainsKey(hour));
        }

        // Verify no slots after end hour
        for (int hour = endHour; hour < 24; hour++)
        {
            Assert.False(slots.ContainsKey(hour));
        }

        // Verify slots exist during working hours
        for (int hour = startHour; hour < endHour; hour++)
        {
            Assert.True(slots.ContainsKey(hour));
        }
    }

    [Fact]
    public void WorkingDaySchedulerShouldHandlePausesCorrectly()
    {
        var pauses = new List<TimeSlot>
        {
            new AvailableTimeSlot(
                TimeSpan.FromHours(12),    // 12 PM
                TimeSpan.FromHours(13)     // 1 PM
            ),
            new AvailableTimeSlot(
                TimeSpan.FromHours(15),    // 3 PM
                TimeSpan.FromHours(15.5)   // 3:30 PM
            )
        };

        var workingDay = new WorkingDay(
            TimeSpan.FromHours(9),     // 9 AM
            TimeSpan.FromHours(17),    // 5 PM
            pauses
        );

        var scheduler = new WorkingDayScheduler(workingDay, 30); // 30-minute intervals
        var slots = scheduler.TimeSlotsForTheSchedule();

        // Verify lunch hour is completely excluded
        Assert.False(slots.ContainsKey(12));

        // Verify 3 PM hour has only one 30-minute slot
        Assert.Single(slots[15]);

        // Verify normal working hours have expected number of slots
        Assert.Equal(2, slots[10].Count); // Should have two 30-minute slots
    }

    [Fact]
    public void SlotsShouldNotExtendBeyond24Hours()
    {
        var scheduler = new TestSlotScheduler(60);
        var slots = scheduler.TimeSlotsForTheSchedule();

        foreach (var hourSlots in slots.Values)
        {
            foreach (var slot in hourSlots)
            {
                Assert.True(slot.EndTime <= TimeSpan.FromHours(24));
            }
        }
    }

    [Fact]
    public void WorkingDaySchedulerShouldHandleEdgeCases()
    {
        var workingDay = new WorkingDay(
            TimeSpan.FromHours(23),    // 11 PM
            TimeSpan.FromHours(24),    // Midnight
            new List<TimeSlot>()
        );

        var scheduler = new WorkingDayScheduler(workingDay, 15); // 15-minute intervals
        var slots = scheduler.TimeSlotsForTheSchedule();

        Assert.True(slots.ContainsKey(23));
        Assert.Equal(4, slots[23].Count); // Should have four 15-minute slots

        // Verify all slots are within bounds
        foreach (var slot in slots[23])
        {
            Assert.True(slot.StartTime >= TimeSpan.FromHours(23));
            Assert.True(slot.EndTime <= TimeSpan.FromHours(24));
        }
    }

    [Fact]
    public void OverlappingPausesShouldBeMerged()
    {
        var pauses = new List<TimeSlot>
        {
            new AvailableTimeSlot(
                TimeSpan.FromHours(12),
                TimeSpan.FromHours(13)
            ),
            new AvailableTimeSlot(
                TimeSpan.FromHours(12.5),
                TimeSpan.FromHours(13.5)
            )
        };

        var workingDay = new WorkingDay(
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(17),
            pauses
        );

        var scheduler = new WorkingDayScheduler(workingDay, 30);
        var slots = scheduler.TimeSlotsForTheSchedule();

        // Verify that hours affected by overlapping pauses are handled correctly
        Assert.False(slots.ContainsKey(12));
        Assert.Single(slots[13]); // Should only have one 30-minute slot
    }

    private class TestSlotScheduler : SlotScheduler
    {
        public TestSlotScheduler(int interval) : base(interval) { }
    }
}
