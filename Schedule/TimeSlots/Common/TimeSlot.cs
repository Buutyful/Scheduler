namespace Schedule.TimeSlots.Common;

public abstract record TimeSlot : IComparer<TimeSlot>
{
    public TimeSpan StartTime { get; }
    public TimeSpan EndTime { get; }
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