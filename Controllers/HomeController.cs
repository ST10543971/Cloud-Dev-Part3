using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VenueBookingApp.Models;

namespace VenueBookingApp.Controllers;

public class HomeController(VenueBookingContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new VenueDashboardViewModel
        {
            VenueCount = await context.Venues.CountAsync(),
            EventCount = await context.Events.CountAsync(),
            BookingCount = await context.Bookings.CountAsync(),
            UpcomingEvents = await context.Events
                .Include(@event => @event.Venue)
                .Where(@event => @event.EventDate >= DateTime.Today)
                .OrderBy(@event => @event.EventDate)
                .Take(5)
                .ToListAsync(),
            RecentBookings = await context.Bookings
                .Include(booking => booking.Event)
                .ThenInclude(@event => @event!.Venue)
                .OrderByDescending(booking => booking.BookingId)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
