namespace VenueBookingApp.Models;

public class VenueDashboardViewModel
{
    public int VenueCount { get; set; }
    public int EventCount { get; set; }
    public int BookingCount { get; set; }
    public List<Event> UpcomingEvents { get; set; } = new();
    public List<Booking> RecentBookings { get; set; } = new();
}