using Microsoft.EntityFrameworkCore;

namespace VenueBookingApp.Models;

public class VenueBookingContext(DbContextOptions<VenueBookingContext> options) : DbContext(options)
{
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venue>()
            .HasMany(venue => venue.Events)
            .WithOne(@event => @event.Venue)
            .HasForeignKey(@event => @event.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventType>()
            .HasMany(eventType => eventType.Venues)
            .WithOne(venue => venue.EventType)
            .HasForeignKey(venue => venue.EventTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasMany(@event => @event.Bookings)
            .WithOne(booking => booking.Event)
            .HasForeignKey(booking => booking.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}