using Schedule.SchedulerService.Common;

namespace Schedule.SchedulerService;

public class FullDayScheduler(int interval) : SlotScheduler(interval)
{
}
