using Schedule.SchedulerService.Common;

namespace Schedule.SchedulerService;

public class WorkingDayScheduler(WorkingDay day, int interval) : SlotScheduler(interval)
{
    public WorkingDay WorkingDay { get; } = day ?? throw new ArgumentNullException(nameof(day));

    public override IReadOnlyDictionary<int, IReadOnlyList<AvailableTimeSlot>> TimeSlotsForTheDay()
    {
        var allDaySlots = base.TimeSlotsForTheDay();
        var workingHourSlots = new Dictionary<int, List<AvailableTimeSlot>>();

        // Calculate the actual end hour for processing
        int endHour = WorkingDay.EndTime.Hours;
        if (WorkingDay.EndTime == TimeSpan.FromHours(24))
        {
            endHour = 23; // Process up to hour 23 if end time is 24:00
        }

        // Process hours within working day range
        for (int hour = WorkingDay.StartTime.Hours; hour <= endHour; hour++)
        {
            var availableSlots = new List<AvailableTimeSlot>();
            foreach (var slot in allDaySlots[hour])
            {
                // Skip slots entirely outside working hours
                if (slot.EndTime <= WorkingDay.StartTime || slot.StartTime >= WorkingDay.EndTime)
                    continue;

                // Adjust slot boundaries to working hours
                var adjustedStart = TimeSpan.FromTicks(Math.Max(slot.StartTime.Ticks, WorkingDay.StartTime.Ticks));
                var adjustedEnd = TimeSpan.FromTicks(Math.Min(slot.EndTime.Ticks, WorkingDay.EndTime.Ticks));
                var adjustedSlot = new AvailableTimeSlot(adjustedStart, adjustedEnd);

                // Check if slot overlaps with any pause
                if (!WorkingDay.Pauses.Any(pause => adjustedSlot.IsOverLap(pause)))
                {
                    availableSlots.Add(adjustedSlot);
                }
            }

            if (availableSlots.Any())
            {
                workingHourSlots[hour] = availableSlots;
            }
        }

        return workingHourSlots.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<AvailableTimeSlot>)kvp.Value.AsReadOnly());
    }
}