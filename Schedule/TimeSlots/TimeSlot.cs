namespace Schedule.TimeSlots;

public abstract record TimeSlot : IComparer<TimeSlot>
{
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    protected TimeSlot(TimeSpan startTime, TimeSpan endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Invalid time slot: End time must be after start time");

        StartTime = startTime;
        EndTime = endTime;
    }
    public bool IsOverLap(TimeSlot? other)
    {
        if (other is null) return false;

        return StartTime < other.EndTime && EndTime > other.StartTime;
    }
    public bool Contains(TimeSpan time) => StartTime <= time && EndTime > time;
    public int Compare(TimeSlot? x, TimeSlot? y)
    {
        if (x == null || y == null)
        {
            throw new ArgumentNullException("TimeSlot instances cannot be null");
        }

        int startComparison = x.StartTime.CompareTo(y.StartTime);
        if (startComparison != 0)
        {
            return startComparison;
        }

        return x.EndTime.CompareTo(y.EndTime);
    }
}

public record AvailableTimeSlot(TimeSpan StartTime, TimeSpan EndTime) : TimeSlot(StartTime, EndTime);

public record BookedTimeSlot(Guid UserId, TimeSpan StartTime, TimeSpan EndTime) : TimeSlot(StartTime, EndTime)
{
    public BookedTimeSlot(AvailableTimeSlot availableSlot, Guid userId) : this(userId, availableSlot.StartTime, availableSlot.EndTime) { }
};

public record WorkingDay : TimeSlot
{
    public IReadOnlyCollection<TimeSlot> Pauses { get; init; } = [];
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


