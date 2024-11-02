using Schedule.TimeSlots.Common;

namespace Schedule.TimeSlots;

public record BookedTimeSlot(Guid UserId, TimeSpan StartTime, TimeSpan EndTime) : TimeSlot(StartTime, EndTime);