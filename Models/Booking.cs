using System.ComponentModel.DataAnnotations;

namespace VenueBookingApp.Models;

public class Booking
{
    public int BookingId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    [Display(Name = "Tickets Booked")]
    public int NumberOfTickets { get; set; }

    [Display(Name = "Event")]
    public int EventId { get; set; }
    public Event? Event { get; set; }
}