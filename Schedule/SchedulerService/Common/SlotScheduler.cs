namespace Schedule.SchedulerService.Common;

public abstract class SlotScheduler
{
    public int Interval { get; }

    public SlotScheduler(int interval = 60)
    {
        if (interval <= 0)
            throw new ArgumentException("Interval must be positive", nameof(interval));
        if (60 % interval != 0)
            throw new ArgumentException("Interval must divide evenly into 60 minutes", nameof(interval));

        Interval = interval;
    }
    public virtual IReadOnlyDictionary<int, IReadOnlyList<AvailableTimeSlot>> TimeSlotsForTheDay()
    {
        var timeSlots = new List<AvailableTimeSlot>();
        var startTime = TimeSpan.Zero;

        // Generate time slots until we reach or exceed 24 hours
        while (startTime < TimeSpan.FromHours(24))
        {
            var endTime = startTime.Add(TimeSpan.FromMinutes(Interval));

            if (endTime >= TimeSpan.FromHours(24))
                endTime = TimeSpan.FromHours(24);

            timeSlots.Add(new AvailableTimeSlot(startTime, endTime));

            if (endTime == TimeSpan.FromHours(24)) break;
            startTime = endTime;
        }

        return GroupSlotsByHour(timeSlots);
    }

    private static IReadOnlyDictionary<int, IReadOnlyList<AvailableTimeSlot>> GroupSlotsByHour(
    IEnumerable<AvailableTimeSlot> timeSlots)
    {
        var hourlySlots = new Dictionary<int, List<AvailableTimeSlot>>();
        for (int hour = 0; hour < 24; hour++)
        {
            hourlySlots[hour] = new List<AvailableTimeSlot>();
        }

        foreach (var slot in timeSlots)
        {
            
            int hour = slot.StartTime.Hours;
            hourlySlots[hour].Add(slot);
        }

        return hourlySlots.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<AvailableTimeSlot>)kvp.Value.AsReadOnly());
    }
}