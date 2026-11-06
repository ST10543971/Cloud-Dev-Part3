using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VenueBookingApp.Models;

namespace VenueBookingApp.Controllers;

public class BookingsController(VenueBookingContext context) : Controller
{
    public async Task<IActionResult> Index(string? searchString)
    {
        var bookings = context.Bookings
            .Include(booking => booking.Event)
            .ThenInclude(@event => @event!.Venue)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            ViewData["CurrentFilter"] = searchString;
            bookings = bookings.Where(b => b.Event != null && EF.Functions.Like(b.Event!.EventName, $"%{searchString}%"));
        }

        bookings = bookings.OrderByDescending(booking => booking.BookingId);

        return View(await bookings.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var booking = await context.Bookings
            .Include(item => item.Event)
            .ThenInclude(@event => @event!.Venue)
            .FirstOrDefaultAsync(item => item.BookingId == id);

        if (booking is null)
        {
            return NotFound();
        }

        return View(booking);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateEventsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("BookingId,CustomerName,NumberOfTickets,EventId")] Booking booking)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEventsAsync(booking.EventId);
            return View(booking);
        }

        // Prevent creating a booking for an event where the event's venue is double-booked
        var selectedEvent = await context.Events.FindAsync(booking.EventId);
        if (selectedEvent is null)
        {
            ModelState.AddModelError(string.Empty, "The selected event does not exist.");
            await PopulateEventsAsync(booking.EventId);
            return View(booking);
        }

        // compare by calendar day using a range to avoid provider translation issues
        var eventDate = selectedEvent.EventDate.Date;
        var nextDay = eventDate.AddDays(1);
        var venueConflict = await context.Events
            .AnyAsync(e => e.VenueId == selectedEvent.VenueId
                           && e.EventDate >= eventDate
                           && e.EventDate < nextDay
                           && e.EventId != selectedEvent.EventId);

        if (venueConflict)
        {
            ModelState.AddModelError(string.Empty, "This event's venue is already booked at the same date and time. Please choose a different event or contact an administrator.");
            await PopulateEventsAsync(booking.EventId);
            return View(booking);
        }

        // Prevent duplicate booking by the same customer for the same event
        var customerNormalized = (booking.CustomerName ?? string.Empty).Trim().ToLowerInvariant();
        var duplicate = await context.Bookings
            .AnyAsync(b => b.EventId == booking.EventId && b.CustomerName.ToLower() == customerNormalized);

        if (duplicate)
        {
            ModelState.AddModelError(string.Empty, "A booking for this customer already exists for the selected event.");
            await PopulateEventsAsync(booking.EventId);
            return View(booking);
        }

        context.Add(booking);
        try
        {
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to save booking. Please check your input and try again.");
            await PopulateEventsAsync(booking.EventId);
            return View(booking);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var booking = await context.Bookings.FindAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        await PopulateEventsAsync(booking.EventId);
        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("BookingId,CustomerName,NumberOfTickets,EventId")] Booking booking)
    {
        if (id != booking.BookingId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateEventsAsync(booking.EventId);
            return View(booking);
        }

        // Prevent editing a booking when the selected event's venue is double-booked
        var selectedEvent = await context.Events.FindAsync(booking.EventId);
        if (selectedEvent is null)
        {
            ModelState.AddModelError(string.Empty, "The selected event does not exist.");
            await PopulateEventsAsync(booking.EventId);
            return View(booking);
        }

        var eventDate = selectedEvent.EventDate.Date;
        var nextDay = eventDate.AddDays(1);
        var venueConflict = await context.Events
            .AnyAsync(e => e.VenueId == selectedEvent.VenueId
                           && e.EventDate >= eventDate
                           && e.EventDate < nextDay
                           && e.EventId != selectedEvent.EventId);

        if (venueConflict)
        {
            ModelState.AddModelError(string.Empty, "Cannot save booking because the event's venue is double-booked at the same date and time.");
            await PopulateEventsAsync(booking.EventId);
            return View(booking);
        }

        // Prevent duplicate booking by the same customer for the same event when editing
        var customerNormalized = (booking.CustomerName ?? string.Empty).Trim().ToLowerInvariant();
        var duplicate = await context.Bookings
            .AnyAsync(b => b.EventId == booking.EventId && b.CustomerName.ToLower() == customerNormalized && b.BookingId != booking.BookingId);

        if (duplicate)
        {
            ModelState.AddModelError(string.Empty, "A booking for this customer already exists for the selected event.");
            await PopulateEventsAsync(booking.EventId);
            return View(booking);
        }

        try
        {
            context.Update(booking);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await BookingExistsAsync(booking.BookingId))
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to save booking changes. Please check your input and try again.");
            await PopulateEventsAsync(booking.EventId);
            return View(booking);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var booking = await context.Bookings
            .Include(item => item.Event)
            .ThenInclude(@event => @event!.Venue)
            .FirstOrDefaultAsync(item => item.BookingId == id);

        if (booking is null)
        {
            return NotFound();
        }

        return View(booking);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Load booking with its event so we can check the event date
        var booking = await context.Bookings
            .Include(b => b.Event)
            .ThenInclude(e => e!.Venue)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking is null)
        {
            return NotFound();
        }

        // Block deletion for bookings belonging to upcoming events (event date >= today)
        var eventDate = booking.Event?.EventDate.Date;
        if (eventDate is not null && eventDate >= DateTime.Today)
        {
            ModelState.AddModelError(string.Empty, "Cannot delete this booking because the event is upcoming. Remove or change the booking after the event date.");
            return View(booking);
        }

        context.Bookings.Remove(booking);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateEventsAsync(object? selectedEvent = null)
    {
        var events = await context.Events
            .Include(@event => @event.Venue)
            .OrderBy(@event => @event.EventDate)
            .Select(@event => new
            {
                @event.EventId,
                DisplayName = @event.EventName + " - " + @event.Venue!.Name + " (" + @event.EventDate.ToString("dd MMM yyyy HH:mm") + ")"
            })
            .ToListAsync();

        ViewData["EventId"] = new SelectList(events, "EventId", "DisplayName", selectedEvent);
    }

    private Task<bool> BookingExistsAsync(int id)
    {
        return context.Bookings.AnyAsync(item => item.BookingId == id);
    }

    [HttpGet]
    public async Task<JsonResult> CheckEventConflict(int eventId)
    {
        var selectedEvent = await context.Events.FindAsync(eventId);
        if (selectedEvent is null)
        {
            return Json(new { conflict = true, message = "The selected event does not exist." });
        }

        var eventDate = selectedEvent.EventDate.Date;
        var nextDay = eventDate.AddDays(1);
        var venueConflict = await context.Events
            .AnyAsync(e => e.VenueId == selectedEvent.VenueId
                           && e.EventDate >= eventDate
                           && e.EventDate < nextDay
                           && e.EventId != selectedEvent.EventId);

        if (venueConflict)
        {
            return Json(new { conflict = true, message = "This event's venue is already booked at the same date and time." });
        }

        return Json(new { conflict = false, message = string.Empty });
    }
}