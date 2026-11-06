using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VenueBookingApp.Models;
using Microsoft.AspNetCore.Http;

namespace VenueBookingApp.Controllers;

public class EventsController : Controller
{
    private readonly VenueBookingContext context;

    public EventsController(VenueBookingContext context)
    {
        this.context = context;
    }
    public async Task<IActionResult> Index()
    {
        var events = context.Events
            .Include(@event => @event.Venue)
            .OrderBy(@event => @event.EventDate);

        return View(await events.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var @event = await context.Events
            .Include(item => item.Venue)
            .Include(item => item.Bookings)
            .FirstOrDefaultAsync(item => item.EventId == id);

        if (@event is null)
        {
            return NotFound();
        }

        return View(@event);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateVenuesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EventId,EventName,EventDate,VenueId")] Event @event)
    {
        if (!ModelState.IsValid)
        {
            await PopulateVenuesAsync(@event.VenueId);
            return View(@event);
        }

        // Prevent double booking: ensure no other event exists for the same venue on the same calendar day
        var eventDate = @event.EventDate.Date;
        var nextDay = eventDate.AddDays(1);
        var conflictExists = await context.Events
            .AnyAsync(e => e.VenueId == @event.VenueId
                           && e.EventDate >= eventDate
                           && e.EventDate < nextDay);

        if (conflictExists)
        {
            ModelState.AddModelError(string.Empty, "The selected venue is already booked for the chosen date and time, please select a different booking");
            await PopulateVenuesAsync(@event.VenueId);
            return View(@event);
        }

        // image upload disabled (Azure storage removed) - ImageUrl will be null by default

        context.Add(@event);
        try
        {
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to save event. Please check your input and try again.");
            await PopulateVenuesAsync(@event.VenueId);
            return View(@event);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var @event = await context.Events.FindAsync(id);
        if (@event is null)
        {
            return NotFound();
        }

        await PopulateVenuesAsync(@event.VenueId);
        return View(@event);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,VenueId")] Event @event, IFormFile? ImageFile)
    {
        if (id != @event.EventId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateVenuesAsync(@event.VenueId);
            return View(@event);
        }

        // Prevent double booking when editing: exclude the current event by id
        var eventDate = @event.EventDate.Date;
        var nextDay = eventDate.AddDays(1);
        var conflictExists = await context.Events
            .AnyAsync(e => e.VenueId == @event.VenueId
                           && e.EventDate >= eventDate
                           && e.EventDate < nextDay
                           && e.EventId != @event.EventId);

        if (conflictExists)
        {
            ModelState.AddModelError(string.Empty, "The selected venue is already booked for the chosen date and time.");
            await PopulateVenuesAsync(@event.VenueId);
            return View(@event);
        }

        // image handling removed

        try
        {
            context.Update(@event);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await EventExistsAsync(@event.EventId))
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to save event changes. Please check your input and try again.");
            await PopulateVenuesAsync(@event.VenueId);
            return View(@event);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var @event = await context.Events
            .Include(item => item.Venue)
            .FirstOrDefaultAsync(item => item.EventId == id);

        if (@event is null)
        {
            return NotFound();
        }

        return View(@event);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Prevent deleting an event that has bookings
        var hasBookings = await context.Bookings.AnyAsync(b => b.EventId == id);
        var @event = await context.Events
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (@event is null)
        {
            return NotFound();
        }

        if (hasBookings)
        {
            ModelState.AddModelError(string.Empty, "Cannot delete this event because there are existing bookings associated with it.");
            return View(@event);
        }

        // image deletion disabled; just remove the entity
        context.Events.Remove(@event);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateVenuesAsync(object? selectedVenue = null)
    {
        var venues = await context.Venues
            .OrderBy(venue => venue.Name)
            .ToListAsync();

        ViewData["VenueId"] = new SelectList(venues, nameof(Venue.VenueId), nameof(Venue.Name), selectedVenue);
    }

    private Task<bool> EventExistsAsync(int id)
    {
        return context.Events.AnyAsync(item => item.EventId == id);
    }
}