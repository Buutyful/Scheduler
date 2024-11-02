using Schedule.SchedulerService;
using Schedule.SchedulerService.Common;
using Schedule.TimeSlots;

namespace Schedule.Tests;

public class DayScheduleTests
{
    private readonly ISlotScheduler _scheduler;
    private readonly Guid _userId;

    public DayScheduleTests()
    {
        _scheduler = new FullDayScheduler(20); // 20-minute intervals
        _userId = Guid.NewGuid();
    }

    public static IEnumerable<object[]> ValidBookingTimes()
    {
        yield return new object[] { new TimeSpan(15, 0, 0) };
        yield return new object[] { new TimeSpan(15, 20, 0) };
        yield return new object[] { new TimeSpan(15, 40, 0) };
        yield return new object[] { new TimeSpan(23, 40, 0) };
        yield return new object[] { new TimeSpan(0, 0, 0) };
    }

    [Theory]
    [MemberData(nameof(ValidBookingTimes))]
    public void TryBookSlot_WithValidTime_ShouldSucceed(TimeSpan timeToBook)
    {
        // Arrange
        var daySchedule = new DaySchedule(_scheduler);

        // Act
        var result = daySchedule.TryBookSlot(timeToBook, _userId);

        // Assert
        Assert.True(result.IsError == false);
        var bookedSlot = result.Value;
        Assert.True(bookedSlot.Contains(timeToBook));
        Assert.Equal(_userId, bookedSlot.UserId);
    }

    [Fact]
    public void TryBookSlot_WithNonExistingHour_ShouldReturnError()
    {
        // Arrange
        var daySchedule = new DaySchedule(_scheduler);
        var invalidTime = new TimeSpan(25, 0, 0); // Invalid hour

        // Act
        var result = daySchedule.TryBookSlot(invalidTime, _userId);

        // Assert
        Assert.True(result.IsError);
    }

    [Fact]
    public void TryBookSlot_WithAlreadyBookedSlot_ShouldReturnError()
    {
        // Arrange
        var daySchedule = new DaySchedule(_scheduler);
        var timeToBook = new TimeSpan(15, 0, 0);

        // Act
        var firstBooking = daySchedule.TryBookSlot(timeToBook, _userId);
        var secondBooking = daySchedule.TryBookSlot(timeToBook, Guid.NewGuid());

        // Assert
        Assert.True(firstBooking.IsError == false);
        Assert.True(secondBooking.IsError);
    }

    [Fact]
    public void GetSlotsByHour_WithValidHour_ShouldReturnSlots()
    {
        // Arrange
        var daySchedule = new DaySchedule(_scheduler);
        var hour = 15;

        // Act
        var slots = daySchedule.GetSlotsByHour(hour).ToList();

        // Assert
        Assert.NotEmpty(slots);
        Assert.All(slots, slot => Assert.Equal(hour, slot.StartTime.Hours));
    }

    [Fact]
    public void GetSlotsByHour_WithInvalidHour_ShouldReturnEmpty()
    {
        // Arrange
        var daySchedule = new DaySchedule(_scheduler);
        var invalidHour = 25;

        // Act
        var slots = daySchedule.GetSlotsByHour(invalidHour).ToList();

        // Assert
        Assert.Empty(slots);
    }

    [Fact]
    public void GetAllAvailableSlots_ShouldReturnOnlyAvailableSlots()
    {
        // Arrange
        var daySchedule = new DaySchedule(_scheduler);
        var timeToBook = new TimeSpan(15, 0, 0);
        daySchedule.TryBookSlot(timeToBook, _userId);

        // Act
        var availableSlots = daySchedule.GetAllAvailableSlots().ToList();

        // Assert
        Assert.All(availableSlots, slot => Assert.IsType<AvailableTimeSlot>(slot));
        Assert.DoesNotContain(availableSlots,
            slot => slot.Contains(timeToBook));
    }

    [Theory]
    [MemberData(nameof(ValidBookingTimes))]
    public void TryBookSlot_ShouldUpdateSlotsList(TimeSpan timeToBook)
    {
        // Arrange
        var daySchedule = new DaySchedule(_scheduler);

        // Act
        var bookingResult = daySchedule.TryBookSlot(timeToBook, _userId);
        var hourSlots = daySchedule.GetSlotsByHour(timeToBook.Hours).ToList();

        // Assert
        Assert.Contains(hourSlots, slot =>
            slot is BookedTimeSlot bookedSlot &&
            bookedSlot.Contains(timeToBook) &&
            bookedSlot.UserId == _userId);
    }
    [Fact]
    public void ConsecutiveSlots_ShouldNotOverlap()
    {
        // Arrange
        var slot1 = new AvailableTimeSlot(
            TimeSpan.FromHours(15),
            TimeSpan.FromHours(15) + TimeSpan.FromMinutes(20));
        var slot2 = new AvailableTimeSlot(
            TimeSpan.FromHours(15) + TimeSpan.FromMinutes(20),
            TimeSpan.FromHours(15) + TimeSpan.FromMinutes(40));
        var boundaryTime = TimeSpan.FromHours(15) + TimeSpan.FromMinutes(20);

        // Assert
        Assert.False(slot1.Contains(boundaryTime),
            "First slot should not contain boundary time");
        Assert.True(slot2.Contains(boundaryTime),
            "Second slot should contain boundary time");
        Assert.False(slot1.IsOverLap(slot2),
            "Consecutive slots should not overlap");
    }
    [Fact]
    public void WhenSlotIsBooked_ItShouldNotBeAvailable()
    {
        // Arrange
        var daySchedule = new DaySchedule(_scheduler);
        var timeToBook = new TimeSpan(15, 0, 0);

        // Act
        var beforeBooking = daySchedule.GetAllAvailableSlots()
            .Any(slot => slot.Contains(timeToBook));
        var bookingResult = daySchedule.TryBookSlot(timeToBook, _userId);
        var afterBooking = daySchedule.GetAllAvailableSlots()
            .Any(slot => slot.Contains(timeToBook));

        // Assert
        Assert.True(beforeBooking, "Slot should be available before booking");
        Assert.False(bookingResult.IsError, "Booking should succeed");
        Assert.False(afterBooking, "Slot should not be available after booking");
    }
}