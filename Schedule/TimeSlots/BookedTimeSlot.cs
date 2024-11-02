using Schedule.TimeSlots.Common;

public record BookedTimeSlot(Guid UserId, TimeSpan StartTime, TimeSpan EndTime) : TimeSlot(StartTime, EndTime);

