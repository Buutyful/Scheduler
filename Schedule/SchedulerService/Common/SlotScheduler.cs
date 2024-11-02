using Schedule.TimeSlots;
using Schedule.TimeSlots.Common;

namespace Schedule.SchedulerService.Common;

public interface ISlotScheduler
{
    public IReadOnlyDictionary<int, List<TimeSlot>> TimeSlotsForTheSchedule();
}

public abstract class SlotScheduler : ISlotScheduler
{
    protected static readonly TimeSpan DayLength = TimeSpan.FromHours(24);
    public int Interval { get; }

    protected SlotScheduler(int interval = 60)
    {
        if (interval <= 0)
            throw new ArgumentException("Interval must be positive", nameof(interval));
        if (60 % interval != 0)
            throw new ArgumentException("Interval must divide evenly into 60 minutes", nameof(interval));
        Interval = interval;
    }

    public virtual IReadOnlyDictionary<int, List<TimeSlot>> TimeSlotsForTheSchedule()
    {
        var timeSlots = GenerateTimeSlots().ToList();
        return GroupSlotsByHour(timeSlots).AsReadOnly();
    }

    private IEnumerable<TimeSlot> GenerateTimeSlots()
    {
        var startTime = TimeSpan.Zero;
        while (startTime < DayLength)
        {
            var endTime = startTime.Add(TimeSpan.FromMinutes(Interval));
            if (endTime >= DayLength)
                endTime = DayLength;

            yield return new AvailableTimeSlot(startTime, endTime);

            if (endTime == DayLength)
                yield break;

            startTime = endTime;
        }
    }

    private static Dictionary<int, List<TimeSlot>> GroupSlotsByHour(
        IEnumerable<TimeSlot> timeSlots)
    {
        var hourlySlots = Enumerable.Range(0, 24)
            .ToDictionary(hour => hour, _ => new List<TimeSlot>());

        foreach (var slot in timeSlots)
        {
            hourlySlots[slot.StartTime.Hours].Add(slot);
        }

        return hourlySlots;
    }
}