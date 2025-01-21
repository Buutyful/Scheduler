using Schedule.TimeSlots;

namespace Schedule.SchedulerService;

public class WorkingDayScheduler : SlotScheduler
{
    public WorkingDay WorkingDay { get; }

    public WorkingDayScheduler(WorkingDay day, int interval) : base(interval)
    {
        WorkingDay = day ?? throw new ArgumentNullException(nameof(day));
    }

    public override IReadOnlyDictionary<int, List<TimeSlot>> TimeSlotsForTheSchedule()
    {
        var allDaySlots = base.TimeSlotsForTheSchedule();
        var workingHourSlots = new Dictionary<int, List<TimeSlot>>();

        int endHour = WorkingDay.EndTime == DayLength
            ? 23
            : WorkingDay.EndTime.Hours;

        for (int hour = WorkingDay.StartTime.Hours; hour <= endHour; hour++)
        {
            if (!allDaySlots.TryGetValue(hour, out var hourSlots))
                continue;

            var availableSlots = GetAvailableSlotsForHour(hourSlots);
            if (availableSlots.Count != 0)
            {
                workingHourSlots[hour] = availableSlots;
            }
        }

        return workingHourSlots.AsReadOnly();
    }

    private List<TimeSlot> GetAvailableSlotsForHour(List<TimeSlot> hourSlots)
    {
        var availableSlots = new List<TimeSlot>();

        foreach (var slot in hourSlots)
        {
            if (IsSlotOutsideWorkingHours(slot))
                continue;

            var adjustedSlot = AdjustSlotToWorkingHours(slot);
            if (!WorkingDay.Pauses.Any(pause => adjustedSlot.IsOverLap(pause)))
            {
                availableSlots.Add(adjustedSlot);
            }
        }

        return availableSlots;
    }

    private bool IsSlotOutsideWorkingHours(TimeSlot slot) =>
        slot.EndTime <= WorkingDay.StartTime || slot.StartTime >= WorkingDay.EndTime;

    private TimeSlot AdjustSlotToWorkingHours(TimeSlot slot)
    {
        var adjustedStart = TimeSpan.FromTicks(Math.Max(slot.StartTime.Ticks, WorkingDay.StartTime.Ticks));
        var adjustedEnd = TimeSpan.FromTicks(Math.Min(slot.EndTime.Ticks, WorkingDay.EndTime.Ticks));
        return new AvailableTimeSlot(adjustedStart, adjustedEnd);
    }
}