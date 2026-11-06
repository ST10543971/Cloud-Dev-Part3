using System.ComponentModel.DataAnnotations;

namespace VenueBookingApp.Models;

public class EventType
{
    public int EventTypeId { get; set; }

    [Required]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Venue> Venues { get; set; } = new List<Venue>();
}
