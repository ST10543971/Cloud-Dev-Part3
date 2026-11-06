using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using VenueBookingApp.Models;

namespace VenueBookingApp.Controllers;

public class VenuesController : Controller
{
    private readonly VenueBookingContext context;

    public VenuesController(VenueBookingContext context)
    {
        this.context = context;
    }
    public async Task<IActionResult> Index(int? eventTypeId, DateTime? startDate, DateTime? endDate, bool? availableOnly)
    {
        var venuesQuery = context.Venues
            .Include(venue => venue.EventType)
            .AsQueryable();

        if (eventTypeId.HasValue)
        {
            venuesQuery = venuesQuery.Where(venue => venue.EventTypeId == eventTypeId.Value);
        }

        if (startDate.HasValue)
        {
            venuesQuery = venuesQuery.Where(venue =>
                venue.Events.All(@event => @event.EventDate >= startDate.Value));
        }

        if (endDate.HasValue)
        {
            venuesQuery = venuesQuery.Where(venue =>
                venue.Events.All(@event => @event.EventDate <= endDate.Value));
        }

        if (availableOnly == true)
        {
            venuesQuery = venuesQuery.Where(venue => venue.IsAvailable);
        }

        ViewBag.EventTypes = await context.EventTypes
            .OrderBy(eventType => eventType.Name)
            .ToListAsync();

        return View(await venuesQuery.OrderBy(venue => venue.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var venue = await context.Venues
            .Include(item => item.Events)
            .FirstOrDefaultAsync(item => item.VenueId == id);

        if (venue is null)
        {
            return NotFound();
        }

        return View(venue);
    }

    public async Task<IActionResult> Create()
    {
        await LoadEventTypesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("VenueId,Name,Location,Capacity,IsAvailable,EventTypeId")] Venue venue)
    {
        if (!ModelState.IsValid)
        {
            await LoadEventTypesAsync();
            return View(venue);
        }
        // image upload disabled (Azure storage removed) - ImageUrl will be null by default

        try
        {
            context.Add(venue);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
         
            ModelState.AddModelError(string.Empty, "Unable to save venue. Please check your input and try again.");
            await LoadEventTypesAsync();
            return View(venue);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var venue = await context.Venues.FindAsync(id);
        if (venue is null)
        {
            return NotFound();
        }

        await LoadEventTypesAsync();
        return View(venue);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("VenueId,Name,Location,Capacity,IsAvailable,EventTypeId")] Venue venue)
    {
        if (id != venue.VenueId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadEventTypesAsync();
            return View(venue);
        }

        // image handling removed

        try
        {
            context.Update(venue);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await VenueExistsAsync(venue.VenueId))
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to save venue changes. Please check your input and try again.");
            await LoadEventTypesAsync();
            return View(venue);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var venue = await context.Venues.FirstOrDefaultAsync(item => item.VenueId == id);
        if (venue is null)
        {
            return NotFound();
        }

        return View(venue);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Prevent deleting a venue that has events with bookings
        var eventIds = await context.Events
            .Where(e => e.VenueId == id)
            .Select(e => e.EventId)
            .ToListAsync();

        var hasBookings = false;
        if (eventIds.Any())
        {
            hasBookings = await context.Bookings.AnyAsync(b => eventIds.Contains(b.EventId));
        }

        var venue = await context.Venues.FindAsync(id);
        if (venue is null)
        {
            return NotFound();
        }

        if (hasBookings)
        {
            ModelState.AddModelError(string.Empty, "Cannot delete this venue because there are bookings for events at this venue. Remove those bookings or reassign events before deleting the venue.");
            return View(venue);
        }

        try
        {
            // image deletion disabled; just remove the entity
            context.Venues.Remove(venue);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to delete venue. Please try again later.");
            return View(venue);
        }
    }

    private Task<bool> VenueExistsAsync(int id)
    {
        return context.Venues.AnyAsync(item => item.VenueId == id);
    }

    private async Task LoadEventTypesAsync()
    {
        ViewBag.EventTypes = await context.EventTypes
            .OrderBy(eventType => eventType.Name)
            .ToListAsync();
    }
}