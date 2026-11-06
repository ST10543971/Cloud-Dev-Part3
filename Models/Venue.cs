using System.ComponentModel.DataAnnotations;

namespace VenueBookingApp.Models;

public class Venue
{
    public int VenueId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Location { get; set; } = string.Empty;

    [Range(1, 100000)]
    public int Capacity { get; set; }

    [Display(Name = "Available")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "Event Type")]
    public int EventTypeId { get; set; }

    public EventType? EventType { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();

}