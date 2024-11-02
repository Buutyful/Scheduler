using Schedule.TimeSlots.Common;

public record AvailableTimeSlot(TimeSpan StartTime, TimeSpan EndTime) : TimeSlot(StartTime, EndTime);

