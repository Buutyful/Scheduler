// See https://aka.ms/new-console-template for more information
using ErrorOr;
using Schedule.SchedulerService.Common;
using Schedule.TimeSlots;
using Schedule.TimeSlots.Common;

Console.WriteLine("Hello, World!");




public class Day(ISlotScheduler scheduler)
{
    private readonly ISlotScheduler _slotScheduler = scheduler;
    private readonly IReadOnlyDictionary<int, List<TimeSlot>> _daySlots = scheduler.TimeSlotsForTheSchedule();


    public ErrorOr<BookedTimeSlot> BookSlot(TimeSpan timeToBook, Guid userId)
    {
        if (!_daySlots.TryGetValue(timeToBook.Hours, out var slots) &&
            slots!.OfType<AvailableTimeSlot>()
            .Where(s => s.Contains(timeToBook)).Any())
        {
            return Error.Failure("no slots bookable at the given time");
        }

        for (int i = 0; i < slots!.Count; i++)
        {
            if (slots[i] is not AvailableTimeSlot available) continue;

            if (available.Contains(timeToBook))
            {
                var booked = new BookedTimeSlot(available, userId);
                //here u can raise events
                slots[i] = booked;
                return booked;
            }
        }
        return Error.Failure();
    }
}




//add all kid of time slots u need, like prenotations and others

public static class TimeSlotMethods
{
    //here u can inject services, loggers, raise domain events etc..
    public static BookedTimeSlot BookTimeSlot(Guid bookingUser, AvailableTimeSlot openSlot) =>
        new BookedTimeSlot(bookingUser, openSlot.StartTime, openSlot.EndTime);
    public static AvailableTimeSlot CancelBooking(Guid bookingUser, BookedTimeSlot openSlot) =>
        new AvailableTimeSlot(openSlot.StartTime, openSlot.EndTime);
}


