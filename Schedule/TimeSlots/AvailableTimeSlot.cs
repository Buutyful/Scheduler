using Schedule.TimeSlots.Common;

namespace Schedule.TimeSlots;

public record AvailableTimeSlot(TimeSpan StartTime, TimeSpan EndTime) : TimeSlot(StartTime, EndTime);