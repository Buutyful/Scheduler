// See https://aka.ms/new-console-template for more information
using ErrorOr;
using Schedule.SchedulerService.Common;
using Schedule.TimeSlots;
using Schedule.TimeSlots.Common;

Console.WriteLine("Hello, World!");




public class DaySchedule(ISlotScheduler scheduler)
{
    private readonly ISlotScheduler _slotScheduler = scheduler;
    private readonly IReadOnlyDictionary<int, List<TimeSlot>> _daySlots = scheduler.TimeSlotsForTheSchedule();

    public ErrorOr<BookedTimeSlot> TryBookSlot(TimeSpan timeToBook, Guid userId)
    {
        if (!_daySlots.TryGetValue(timeToBook.Hours, out var slots))
        {
            return Error.Failure("no slots bookable at the given time");
        }

        var availableSlot = slots.OfType<AvailableTimeSlot>()
            .FirstOrDefault(s => s.Contains(timeToBook));

        if (availableSlot is null)
        {
            return Error.Failure("no slots bookable at the given time");
        }

        var index = slots.IndexOf(availableSlot);
        var bookedSlot = TimeSlotMethods.BookTimeSlot(userId, availableSlot);

        slots[index] = bookedSlot;

        return bookedSlot;
    }
    public IEnumerable<TimeSlot> GetSlotsByHour(int hour)
    {
        if (!_daySlots.TryGetValue(hour, out var slots))
        {
            return [];
        }
        return slots;
    }
    public IEnumerable<TimeSlot> GetAllAvailableSlots() => 
        _daySlots.Values.SelectMany(slots => slots.OfType<AvailableTimeSlot>()).ToList();
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


