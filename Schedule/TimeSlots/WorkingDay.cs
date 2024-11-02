using Schedule.TimeSlots.Common;

public record WorkingDay : TimeSlot
{     
    public IReadOnlyCollection<TimeSlot> Pauses { get; } = [];
    public WorkingDay(
        TimeSpan startTime,
        TimeSpan endTime,
        List<TimeSlot> pauses) : base(startTime, endTime)
    {
        if (startTime >= TimeSpan.FromHours(24) || endTime > TimeSpan.FromHours(24))
            throw new ArgumentException("Working day times must be within 24 hours");
        Pauses = pauses;
    }
}

