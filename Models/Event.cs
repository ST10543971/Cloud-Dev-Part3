using System.ComponentModel.DataAnnotations;

namespace VenueBookingApp.Models;

public class Event
{
    public int EventId { get; set; }

    [Required]
    [StringLength(120)]
    public string EventName { get; set; } = string.Empty;

    [DataType(DataType.DateTime)]
    [Display(Name = "Event Date and Time")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime EventDate { get; set; }

    [Display(Name = "Venue")]
    public int VenueId { get; set; }

    public Venue? Venue { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

}