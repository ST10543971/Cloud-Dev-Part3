using Microsoft.EntityFrameworkCore;
using VenueBookingApp.Models;

namespace VenueBookingApp.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(VenueBookingContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.EventTypes.AnyAsync())
        {
            context.EventTypes.AddRange(
                new EventType { Name = "Conference" },
                new EventType { Name = "Wedding" },
                new EventType { Name = "Concert" },
                new EventType { Name = "Exhibition" },
                new EventType { Name = "Private Function" });
            await context.SaveChangesAsync();
        }

        if (await context.Venues.AnyAsync())
        {
            return;
        }
        var eventTypes = await context.EventTypes
            .ToDictionaryAsync(eventType => eventType.Name);

        var venues = new List<Venue>
        {
            new() { Name = "Grand Hall", Location = "Cape Town", Capacity = 500, EventTypeId = eventTypes["Conference"].EventTypeId },
            new() { Name = "Skyline Arena", Location = "Johannesburg", Capacity = 1200, EventTypeId = eventTypes["Concert"].EventTypeId },
            new() { Name = "Harbour View Centre", Location = "Durban", Capacity = 350, EventTypeId = eventTypes["Wedding"].EventTypeId }
        };

        context.Venues.AddRange(venues);
        await context.SaveChangesAsync();

        var events = new List<Event>
        {
            new() { EventName = "Business Expo", EventDate = DateTime.Today.AddDays(14), VenueId = venues[0].VenueId },
            new() { EventName = "Wedding Showcase", EventDate = DateTime.Today.AddDays(21), VenueId = venues[1].VenueId },
            new() { EventName = "Live Jazz Night", EventDate = DateTime.Today.AddDays(7), VenueId = venues[2].VenueId }
        };

        context.Events.AddRange(events);
        await context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            new() { CustomerName = "Ava Ndlovu", NumberOfTickets = 2, EventId = events[0].EventId },
            new() { CustomerName = "Liam Jacobs", NumberOfTickets = 4, EventId = events[1].EventId },
            new() { CustomerName = "Mia Daniels", NumberOfTickets = 3, EventId = events[2].EventId }
        };

        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync();
    }
}