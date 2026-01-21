using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Lead
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }

    public Guid SeekerId { get; set; }

    public Guid ListerId { get; set; }

    public string? Status { get; set; }

    public string? Message { get; set; }

    public DateTime? AppointmentDate { get; set; }

    public string? ListerNote { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Lister { get; set; } = null!;

    public virtual Listing Listing { get; set; } = null!;

    public virtual User Seeker { get; set; } = null!;
}
