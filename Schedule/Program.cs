// See https://aka.ms/new-console-template for more information
using Schedule.SchedulerService.Common;

Console.WriteLine("Hello, World!");




public class Day(SlotScheduler scheduler)
{
    private readonly SlotScheduler _slotScheduler = scheduler;
    private readonly IReadOnlyDictionary<int, IReadOnlyList<AvailableTimeSlot>> _daySlots = scheduler.TimeSlotsForTheDay();
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

